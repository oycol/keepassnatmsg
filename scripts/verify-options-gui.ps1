param(
    [string]$kpExe = "C:\Program Files\KeePass Password Safe 2\KeePass.exe",
    [string]$newtonsoftDll,
    [string]$ciDll,
    [string]$outputDir = "e2e-artifacts"
)

$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

if (-not (Test-Path $kpExe)) { throw "KeePass.exe not found at $kpExe" }
if (-not (Test-Path $newtonsoftDll)) { throw "Newtonsoft.Json.dll not found at $newtonsoftDll" }
if (-not (Test-Path $ciDll)) { throw "Plugin dll not found at $ciDll" }

Write-Host "Loading assemblies into PowerShell session..."
[System.Reflection.Assembly]::LoadFrom($kpExe) | Out-Null
[System.Reflection.Assembly]::LoadFrom($newtonsoftDll) | Out-Null
$pluginAsm = [System.Reflection.Assembly]::LoadFrom($ciDll)

$customConfig = New-Object KeePass.App.Configuration.AceCustomConfig
$opt = New-Object KeePassNatMsg.ConfigOpt($customConfig)
$form = New-Object KeePassNatMsg.Options.OptionsForm($opt)

try {
    Write-Host "Creating form control hierarchy..."
    $form.CreateControl()

    $failures = New-Object 'System.Collections.Generic.List[string]'
    $w = $form.ClientSize.Width
    $h = $form.ClientSize.Height
    Write-Host "Form ClientSize: ${w}x${h}"
    Write-Host "Form Bounds: $($form.Bounds.ToString())"

    if ($w -ne 720 -or $h -ne 590) {
        $failures.Add("Unexpected Form ClientSize: ${w}x${h} (expected 720x590)") | Out-Null
    }

    $bindingFlags = [System.Reflection.BindingFlags]'NonPublic,Instance,Public'

    $lblVersion = $form.GetType().GetField("lblVersion", $bindingFlags).GetValue($form)
    if (-not $lblVersion) { throw "lblVersion control not found" }
    Write-Host "lblVersion text: '$($lblVersion.Text)'"
    if ($lblVersion.Text -ne "KeePassNatMsg v2.4.3") {
        $failures.Add("lblVersion text mismatch: expected 'KeePassNatMsg v2.4.3', got '$($lblVersion.Text)'") | Out-Null
    }

    $pnlCenter = $form.GetType().GetField("pnlVersionCenter", $bindingFlags).GetValue($form)
    if (-not $pnlCenter) { throw "pnlVersionCenter control not found" }
    Write-Host "Version area Location: X=$($pnlCenter.Location.X), Y=$($pnlCenter.Location.Y), Size=$($pnlCenter.Size.Width)x$($pnlCenter.Size.Height)"
    if ($pnlCenter.Location.X -lt 12 -or $pnlCenter.Location.X -gt 24) {
        $failures.Add("Version area must stay at the lower-left; actual X=$($pnlCenter.Location.X)") | Out-Null
    }

    $picFormLogo = $form.GetType().GetField("picFormLogo", $bindingFlags).GetValue($form)
    if (-not $picFormLogo) { throw "picFormLogo control is missing" }
    if (-not $picFormLogo.Image) {
        $failures.Add("picFormLogo.Image is null; logo was not initialized in Designer") | Out-Null
    }
    if ($lblVersion.AutoSize) {
        $failures.Add("lblVersion must use a fixed height so text can be vertically centered with the icon") | Out-Null
    }
    $iconCenterY = $picFormLogo.Top + ($picFormLogo.Height / 2.0)
    $labelCenterY = $lblVersion.Top + ($lblVersion.Height / 2.0)
    Write-Host "Footer vertical centers: icon=$iconCenterY, label=$labelCenterY"
    if ([Math]::Abs($iconCenterY - $labelCenterY) -gt 1) {
        $failures.Add("Version text is not vertically centered with the icon") | Out-Null
    }

    # Verify embedded icon_16 resource inside the compiled DLL
    $resType = $pluginAsm.GetType("KeePassNatMsg.Properties.Resources")
    $resIconProp = if ($resType) { $resType.GetProperty("icon_16", [System.Reflection.BindingFlags]'Static,NonPublic,Public') } else { $null }
    $resIcon = if ($resIconProp) { $resIconProp.GetValue($null, $null) } else { $null }
    if (-not $resIcon) {
        $failures.Add("Compiled assembly does not contain embedded Resources.icon_16 Bitmap") | Out-Null
    } else {
        Write-Host "Embedded Resources.icon_16 verified: $($resIcon.Width)x$($resIcon.Height)"
        if ($resIcon.Width -ne 16 -or $resIcon.Height -ne 16) {
            $failures.Add("Embedded Resources.icon_16 size is $($resIcon.Width)x$($resIcon.Height), expected 16x16") | Out-Null
        }
    }

    # Verify file-level official icon hash
    $officialIconPath = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\KeePassNatMsg\Resources\icon_16.png"))
    $officialIconHash = (Get-FileHash $officialIconPath -Algorithm SHA256).Hash.ToLowerInvariant()
    $expectedIconHash = "f32a7e44faacf4a81dc05fb09f6bc5f9f9d7969877549df1313aaa2939c59ecf"
    Write-Host "Footer icon SHA256: $officialIconHash"
    if ($officialIconHash -ne $expectedIconHash) {
        $failures.Add("icon_16.png is not the pinned official KeePassXC application icon") | Out-Null
    }

    # Verify Database Search Scope radio buttons are all vertically separated
    $rbActive = $form.GetType().GetField("credOnlySearchInSelectedDatabaseRadioButton", $bindingFlags).GetValue($form)
    $rbAll = $form.GetType().GetField("credSearchInAllOpenedDatabasesRadioButton", $bindingFlags).GetValue($form)
    $rbRestrict = $form.GetType().GetField("credRestrictSearchInSpecificDatabaseRadioButton", $bindingFlags).GetValue($form)
    $targetDbCombo = $form.GetType().GetField("comboBoxSearchDatabases", $bindingFlags).GetValue($form)
    $connectionLabel = $form.GetType().GetField("labelConnDb", $bindingFlags).GetValue($form)
    $connectionCombo = $form.GetType().GetField("comboBoxDatabases", $bindingFlags).GetValue($form)

    Write-Host "rbActive Top=$($rbActive.Top), rbAll Top=$($rbAll.Top), rbRestrict Top=$($rbRestrict.Top)"
    if ($rbAll.Top -le $rbActive.Bottom) {
        $failures.Add("Database radio 2 is not vertically below radio 1") | Out-Null
    }
    if ($rbRestrict.Top -le $rbAll.Bottom) {
        $failures.Add("Database radio 3 is not vertically below radio 2") | Out-Null
    }

    $targetGap = $targetDbCombo.Left - $rbRestrict.Right
    $connectionGap = $connectionCombo.Left - $connectionLabel.Right
    Write-Host "Target database input gap: $targetGap px"
    Write-Host "Connection database input gap: $connectionGap px"
    if ($targetGap -lt 32) {
        $failures.Add("Target database input crowds the radio text; gap=$targetGap px, required >=32") | Out-Null
    }
    if ($connectionGap -lt 32) {
        $failures.Add("Connection database input overlaps or crowds its label; gap=$connectionGap px, required >=32") | Out-Null
    }

    $chkAccess = $form.GetType().GetField("credAllowAccessCheckbox", $bindingFlags).GetValue($form)
    $chkUpdates = $form.GetType().GetField("credAllowUpdatesCheckbox", $bindingFlags).GetValue($form)
    Write-Host "chkAccess Top=$($chkAccess.Top), chkUpdates Top=$($chkUpdates.Top)"
    if ($chkUpdates.Top -le $chkAccess.Bottom) {
        $failures.Add("Danger Zone checkboxes are not vertically separated") | Out-Null
    }

    $tipMatching = $form.GetType().GetField("tipMatching", $bindingFlags).GetValue($form)
    if (-not $tipMatching -or -not $tipMatching.Image) {
        $failures.Add("tipMatching icon is missing or has no image") | Out-Null
    }
    else {
        Write-Host "tipMatching icon verified: Size=$($tipMatching.Image.Width)x$($tipMatching.Image.Height)"
    }

    # Verify TabControl count and Favicon Tab
    $tabControl = $form.GetType().GetField("tabControl", $bindingFlags).GetValue($form)
    if (-not $tabControl) { throw "tabControl control not found" }
    Write-Host "TabControl TabCount: $($tabControl.TabCount)"
    if ($tabControl.TabCount -ne 5) {
        $failures.Add("TabControl must have 5 tabs; found $($tabControl.TabCount)") | Out-Null
    }
    $tabFavicon = $form.GetType().GetField("tabFavicon", $bindingFlags).GetValue($form)
    if (-not $tabFavicon) {
        $failures.Add("tabFavicon control not found") | Out-Null
    } else {
        Write-Host "tabFavicon verified: '$($tabFavicon.Text)'"
        $grpFavOptions = $form.GetType().GetField("grpFaviconOptions", $bindingFlags).GetValue($form)
        $grpFavSize = $form.GetType().GetField("grpFaviconSize", $bindingFlags).GetValue($form)
        $grpFavProvider = $form.GetType().GetField("grpFaviconProvider", $bindingFlags).GetValue($form)
        if (-not $grpFavOptions -or -not $grpFavSize -or -not $grpFavProvider) {
            $failures.Add("One or more Favicon Downloader GroupBoxes are missing") | Out-Null
        } else {
            Write-Host "Favicon group boxes: Options Top=$($grpFavOptions.Top), Size Top=$($grpFavSize.Top), Provider Top=$($grpFavProvider.Top)"
            if ($grpFavSize.Top -le $grpFavOptions.Bottom) {
                $failures.Add("grpFaviconSize overlaps or touches grpFaviconOptions") | Out-Null
            }
            if ($grpFavProvider.Top -le $grpFavSize.Bottom) {
                $failures.Add("grpFaviconProvider overlaps or touches grpFaviconSize") | Out-Null
            }

            # Deep geometric & truncation verification:
            $chkList = @("chkFaviconPrefixUrls", "chkFaviconUseTitle", "chkFaviconUpdateModified")
            $tipList = @("lblTipFaviconPrefix", "lblTipFaviconTitle", "lblTipFaviconModified")
            for ($k = 0; $k -lt $chkList.Count; $k++) {
                $c = $form.GetType().GetField($chkList[$k], $bindingFlags).GetValue($form)
                $t = $form.GetType().GetField($tipList[$k], $bindingFlags).GetValue($form)
                if ($c -and $t) {
                    # 1. Collision / Overlap check
                    if ($c.Bottom -gt $t.Top) {
                        $failures.Add("Vertical collision: $($c.Name) (Bottom=$($c.Bottom)) overlaps $($t.Name) (Top=$($t.Top))") | Out-Null
                    }
                    # 2. Text width measurement vs control bounds
                    $textSize = [System.Windows.Forms.TextRenderer]::MeasureText($c.Text, $c.Font)
                    if ($c.Width -lt ($textSize.Width + 16)) {
                        $failures.Add("Text truncated in checkbox $($c.Name): text width=$($textSize.Width) px, control width=$($c.Width) px") | Out-Null
                    }
                }
            }

            # 3. ComboBox items truncation check
            $cmbSize = $form.GetType().GetField("cmbFaviconMaxIconSize", $bindingFlags).GetValue($form)
            if ($cmbSize) {
                Write-Host "Favicon max size combo width: $($cmbSize.Width) px"
                if ($cmbSize.Width -lt 240) {
                    $failures.Add("cmbFaviconMaxIconSize is too narrow; width=$($cmbSize.Width) px, required >=240") | Out-Null
                }
                foreach ($item in $cmbSize.Items) {
                    $itemSize = [System.Windows.Forms.TextRenderer]::MeasureText($item.ToString(), $cmbSize.Font)
                    if ($cmbSize.Width -lt ($itemSize.Width + 24)) {
                        $failures.Add("ComboBox item '$item' is clipped in cmbFaviconMaxIconSize (item width=$($itemSize.Width) px, combo width=$($cmbSize.Width) px)") | Out-Null
                    }
                }
            }

            # 4. Check unescaped ampersands in group titles
            if ($grpFavSize -and $grpFavSize.Text -match '(?<!&)&(?!&)') {
                $failures.Add("grpFaviconSize title contains unescaped ampersand (renders as missing character): '$($grpFavSize.Text)'") | Out-Null
            }
        }
    }

    New-Item -ItemType Directory -Force -Path $outputDir | Out-Null

    # Render each tab individually so all controls across tabs can be visually reviewed
    $tabControl = $form.GetType().GetField("tabControl", $bindingFlags).GetValue($form)
    if ($tabControl) {
        for ($i = 0; $i -lt $tabControl.TabCount; $i++) {
            $tabControl.SelectedIndex = $i
            $form.Refresh()
            $bmpTab = New-Object System.Drawing.Bitmap($form.Width, $form.Height)
            $rectTab = New-Object System.Drawing.Rectangle(0, 0, $form.Width, $form.Height)
            $form.DrawToBitmap($bmpTab, $rectTab)
            $tabName = "tab$i-$($tabControl.TabPages[$i].Name).png"
            $bmpTab.Save((Join-Path $outputDir $tabName), [System.Drawing.Imaging.ImageFormat]::Png)
            $bmpTab.Dispose()
            Write-Host "Saved verified tab screenshot: $tabName"
        }
    }

    # Save primary overall screenshot
    $bmp = New-Object System.Drawing.Bitmap($form.Width, $form.Height)
    $rect = New-Object System.Drawing.Rectangle(0, 0, $form.Width, $form.Height)
    $form.DrawToBitmap($bmp, $rect)
    $shotPath = Join-Path $outputDir "options-form-verified.png"
    $bmp.Save($shotPath, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
    Write-Host "Saved verified OptionsForm GUI screenshot to $shotPath" -ForegroundColor Green

    if ($failures.Count -gt 0) {
        Write-Host "GUI verification failures:" -ForegroundColor Red
        foreach ($failure in $failures) { Write-Host " - $failure" -ForegroundColor Red }
        throw "GUI OptionsForm verification failed with $($failures.Count) issue(s)"
    }

    Write-Host "`nAll critical GUI layout, icon, and version constraints PASSED!" -ForegroundColor Green
}
finally {
    if ($form) {
        $form.Dispose()
    }
}
