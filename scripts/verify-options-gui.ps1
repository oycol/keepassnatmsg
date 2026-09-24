param(
    [string]$kpExe = "C:\Program Files\KeePass Password Safe 2\KeePass.exe",
    [string]$newtonsoftDll,
    [string]$ciDll,
    [string]$outputDir = "e2e-artifacts"
)

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

if (-not (Test-Path $kpExe)) { throw "KeePass.exe not found at $kpExe" }
if (-not (Test-Path $newtonsoftDll)) { throw "Newtonsoft.Json.dll not found at $newtonsoftDll" }
if (-not (Test-Path $ciDll)) { throw "Plugin dll not found at $ciDll" }

[System.Reflection.Assembly]::LoadFrom($kpExe) | Out-Null
[System.Reflection.Assembly]::LoadFrom($newtonsoftDll) | Out-Null
[System.Reflection.Assembly]::LoadFrom($ciDll) | Out-Null

$cfg = New-Object KeePass.App.Configuration.AppConfig
$opt = New-Object KeePassNatMsg.ConfigOpt($cfg.CustomConfig)
$form = New-Object KeePassNatMsg.Options.OptionsForm($opt)
$form.StartPosition = [System.Windows.Forms.FormStartPosition]::CenterScreen
$form.Show()
[System.Windows.Forms.Application]::DoEvents()
Start-Sleep -Milliseconds 600
[System.Windows.Forms.Application]::DoEvents()

Write-Host "Form ClientSize: $($form.ClientSize.Width)x$($form.ClientSize.Height)"
Write-Host "Form Bounds: $($form.Bounds.ToString())"

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

Write-Host "Saved verified OptionsForm GUI screenshot to $shotPath"
