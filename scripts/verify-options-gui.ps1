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

Write-Host "Instantiated OptionsForm successfully."
$form.StartPosition = [System.Windows.Forms.FormStartPosition]::CenterScreen
$form.Show()
[System.Windows.Forms.Application]::DoEvents()
Start-Sleep -Milliseconds 800
[System.Windows.Forms.Application]::DoEvents()

$w = $form.ClientSize.Width
$h = $form.ClientSize.Height
Write-Host "Form ClientSize: ${w}x${h}"
Write-Host "Form Bounds: $($form.Bounds.ToString())"

if ($w -lt 680 -or $h -lt 550) {
    throw "Form client size is unexpectedly small: ${w}x${h}"
}

New-Item -ItemType Directory -Force -Path $outputDir | Out-Null

# Capture the entire form window screenshot
$bmp = New-Object System.Drawing.Bitmap($form.Width, $form.Height)
$graphics = [System.Drawing.Graphics]::FromImage($bmp)
$graphics.CopyFromScreen($form.Location.X, $form.Location.Y, 0, 0, $form.Size)

$shotPath = Join-Path $outputDir "options-form-verified.png"
$bmp.Save($shotPath, [System.Drawing.Imaging.ImageFormat]::Png)
$graphics.Dispose()
$bmp.Dispose()
$form.Close()
$form.Dispose()

Write-Host "Saved verified OptionsForm GUI screenshot to $shotPath" -ForegroundColor Green
