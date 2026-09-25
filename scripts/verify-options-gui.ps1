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
[System.Reflection.Assembly]::LoadFrom($ciDll) | Out-Null

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
    if ($lblVersion.Text -ne "KeePassNatMsg v2.3.8") {
        $failures.Add("lblVersion text mismatch: expected 'KeePassNatMsg v2.3.8', got '$($lblVersion.Text)'") | Out-Null
    }

    $pnlCenter = $form.GetType().GetField("pnlVersionCenter", $bindingFlags).GetValue($form)
    if (-not $pnlCenter) { throw "pnlVersionCenter control not found" }
    Write-Host "Version area Location: X=$($pnlCenter.Location.X), Y=$($pnlCenter.Location.Y), Size=$($pnlCenter.Size.Width)x$($pnlCenter.Size.Height)"
    if ($pnlCenter.Location.X -lt 12 -or $pnlCenter.Location.X -gt 24) {
        $failures.Add("Version area must stay at the lower-left; actual X=$($pnlCenter.Location.X)") | Out-Null
    }

    $picFormLogo = $form.GetType().GetField("picFormLogo", $bindingFlags).GetValue($form)
    if (-not $picFormLogo -or -not $picFormLogo.Image) { throw "picFormLogo or its image is missing" }
    if ($lblVersion.AutoSize) {
        $failures.Add("lblVersion must use a fixed height so text can be vertically centered with the icon") | Out-Null
    }
    $iconCenterY = $picFormLogo.Top + ($picFormLogo.Height / 2.0)
    $labelCenterY = $lblVersion.Top + ($lblVersion.Height / 2.0)
    Write-Host "Footer vertical centers: icon=$iconCenterY, label=$labelCenterY"
    if ([Math]::Abs($iconCenterY - $labelCenterY) -gt 1) {
        $failures.Add("Version text is not vertically centered with the icon") | Out-Null
    }

    $officialIconPath = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\KeePassNatMsg\Resources\icon_16.png"))
    $officialIconHash = (Get-FileHash $officialIconPath -Algorithm SHA256).Hash.ToLowerInvariant()
    $expectedIconHash = "f32a7e44faacf4a81dc05fb09f6bc5f9f9d7969877549df1313aaa2939c59ecf"
    Write-Host "Footer icon SHA256: $officialIconHash"
    if ($officialIconHash -ne $expectedIconHash) {
        $failures.Add("icon_16.png is not the pinned official KeePassXC application icon") | Out-Null
    }

    $rbActive = $form.GetType().GetField("credOnlySearchInSelectedDatabaseRadioButton", $bindingFlags).GetValue($form)
    $rbAll = $form.GetType().GetField("credSearchInAllOpenedDatabasesRadioButton", $bindingFlags).GetValue($form)
    $rbRestrict = $form.GetType().GetField("credRestrictSearchInSpecificDatabaseRadioButton", $bindingFlags).GetValue($form)
    $targetDbCombo = $form.GetType().GetField("comboBoxSearchDatabases", $bindingFlags).GetValue($form)
    $connectionLabel = $form.GetType().GetField("labelConnDb", $bindingFlags).GetValue($form)
    $connectionCombo = $form.GetType().GetField("comboBoxDatabases", $bindingFlags).GetValue($form)

    Write-Host "rbActive Top=$($rbActive.Top), rbAll Top=$($rbAll.Top)"
    if ($rbAll.Top -le $rbActive.Bottom) {
        $failures.Add("Database search radio buttons are not vertically separated") | Out-Null
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

    $tipNotify = $form.GetType().GetField("tipNotify", $bindingFlags).GetValue($form)
    if (-not $tipNotify -or -not $tipNotify.Image) {
        $failures.Add("tipNotify icon is missing or has no image") | Out-Null
    }
    else {
        Write-Host "tipNotify icon verified: Size=$($tipNotify.Image.Width)x$($tipNotify.Image.Height)"
    }

    New-Item -ItemType Directory -Force -Path $outputDir | Out-Null
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
