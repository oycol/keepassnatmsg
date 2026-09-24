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
    
    $w = $form.ClientSize.Width
    $h = $form.ClientSize.Height
    Write-Host "Form ClientSize: ${w}x${h}"
    Write-Host "Form Bounds: $($form.Bounds.ToString())"

    if ($w -ne 720 -or $h -ne 590) {
        throw "Unexpected Form ClientSize: ${w}x${h} (expected 720x590)"
    }

    # Reflection verification of the controls
    $bindingFlags = [System.Reflection.BindingFlags]'NonPublic,Instance,Public'
    
    $lblVersion = $form.GetType().GetField("lblVersion", $bindingFlags).GetValue($form)
    if (-not $lblVersion) { throw "lblVersion control not found" }
    Write-Host "lblVersion text: '$($lblVersion.Text)'"
    if ($lblVersion.Text -ne "KeePassNatMsg v2.3.7") {
        throw "lblVersion text mismatch! Expected 'KeePassNatMsg v2.3.7' but got '$($lblVersion.Text)'"
    }

    $pnlCenter = $form.GetType().GetField("pnlVersionCenter", $bindingFlags).GetValue($form)
    if (-not $pnlCenter) { throw "pnlVersionCenter control not found" }
    Write-Host "pnlVersionCenter Location: X=$($pnlCenter.Location.X), Y=$($pnlCenter.Location.Y), Size=$($pnlCenter.Size.Width)x$($pnlCenter.Size.Height)"
    if ($pnlCenter.Location.X -lt 230 -or $pnlCenter.Location.X -gt 250) {
        throw "pnlVersionCenter not horizontally centered! X=$($pnlCenter.Location.X)"
    }

    # Verify Database Search Scope radio buttons are vertically arranged (no horizontal overlap)
    $rbActive = $form.GetType().GetField("credOnlySearchInSelectedDatabaseRadioButton", $bindingFlags).GetValue($form)
    $rbAll = $form.GetType().GetField("credSearchInAllOpenedDatabasesRadioButton", $bindingFlags).GetValue($form)
    Write-Host "rbActive Top=$($rbActive.Top), rbAll Top=$($rbAll.Top)"
    if ($rbAll.Top -le $rbActive.Top) {
        throw "Database search radio buttons are not vertically separated!"
    }

    # Verify Danger zone checkboxes are vertically arranged
    $chkAccess = $form.GetType().GetField("credAllowAccessCheckbox", $bindingFlags).GetValue($form)
    $chkUpdates = $form.GetType().GetField("credAllowUpdatesCheckbox", $bindingFlags).GetValue($form)
    Write-Host "chkAccess Top=$($chkAccess.Top), chkUpdates Top=$($chkUpdates.Top)"
    if ($chkUpdates.Top -le $chkAccess.Top) {
        throw "Danger Zone checkboxes are not vertically separated!"
    }

    # Verify tip icons exist
    $tipNotify = $form.GetType().GetField("tipNotify", $bindingFlags).GetValue($form)
    if (-not $tipNotify -or -not $tipNotify.Image) {
        throw "tipNotify icon is missing or has no image!"
    }
    Write-Host "tipNotify icon verified: Size=$($tipNotify.Image.Width)x$($tipNotify.Image.Height)"

    New-Item -ItemType Directory -Force -Path $outputDir | Out-Null

    # Render form to bitmap
    $bmp = New-Object System.Drawing.Bitmap($form.Width, $form.Height)
    $rect = New-Object System.Drawing.Rectangle(0, 0, $form.Width, $form.Height)
    $form.DrawToBitmap($bmp, $rect)

    $shotPath = Join-Path $outputDir "options-form-verified.png"
    $bmp.Save($shotPath, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()

    Write-Host "Saved verified OptionsForm GUI screenshot to $shotPath" -ForegroundColor Green
    Write-Host "`nAll 6 critical GUI layout and version constraints PASSED!" -ForegroundColor Green
}
finally {
    if ($form) {
        $form.Dispose()
    }
}
