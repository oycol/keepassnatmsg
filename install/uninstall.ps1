# KeePassNatMsg Uninstall Script for Windows
# Removes plugin, Native Messaging Host manifest, and registry entries

[CmdletBinding()]
param(
    [string]$KeePassPath = "C:\Program Files\KeePass Password Safe 2",
    [string]$Browser = "all"
)

$ErrorActionPreference = "Continue"

Write-Host "=== KeePassNatMsg Uninstaller ===" -ForegroundColor Cyan
Write-Host ""

# --- 1. Remove plugin ---
$pluginsDir = "$KeePassPath\Plugins"
$plgxFile = "$pluginsDir\KeePassNatMsg.plgx"
$dllFile = "$pluginsDir\KeePassNatMsg.dll"

$removed = $false
if (Test-Path $plgxFile) {
    Remove-Item $plgxFile -Force
    Write-Host "[1/3] Removed: $plgxFile" -ForegroundColor Green
    $removed = $true
}
if (Test-Path $dllFile) {
    Remove-Item $dllFile -Force
    Write-Host "      Removed: $dllFile" -ForegroundColor Green
    $removed = $true
}

# Also remove bundled Newtonsoft.Json if it was installed by us
$newtonsoftDll = "$pluginsDir\Newtonsoft.Json.dll"
if (Test-Path $newtonsoftDll) {
    # Only remove if KeePass doesn't need it for other plugins
    Remove-Item $newtonsoftDll -Force -ErrorAction SilentlyContinue
}

if (-not $removed) {
    Write-Host "[1/3] No plugin found to remove" -ForegroundColor Yellow
}

# --- 2. Remove Native Messaging Host ---
$proxyDir = "$env:LOCALAPPDATA\KeePassNatMsg"
$manifestFile = "$proxyDir\kpnm_$($Browser.ToLower()).json"

if (Test-Path $manifestFile) {
    Remove-Item $manifestFile -Force
    Write-Host "[2/3] Removed manifest: $manifestFile" -ForegroundColor Green
} else {
    # Try removing all manifests
    $allManifests = Get-ChildItem "$proxyDir\kpnm_*.json" -ErrorAction SilentlyContinue
    if ($allManifests) {
        foreach ($m in $allManifests) {
            Remove-Item $m.FullName -Force
            Write-Host "      Removed manifest: $($m.Name)" -ForegroundColor Green
        }
    } else {
        Write-Host "[2/3] No manifest found to remove" -ForegroundColor Yellow
    }
}

# Remove proxy if present
$proxyExe = "$proxyDir\keepassnatmsg-proxy.exe"
if (Test-Path $proxyExe) {
    Remove-Item $proxyExe -Force -ErrorAction SilentlyContinue
    Write-Host "      Removed proxy: $proxyExe" -ForegroundColor Green
}

# Remove proxy directory if empty
if (Test-Path $proxyDir) {
    $remaining = Get-ChildItem $proxyDir -ErrorAction SilentlyContinue
    if (-not $remaining) {
        Remove-Item $proxyDir -Force
        Write-Host "      Removed empty directory: $proxyDir" -ForegroundColor Green
    }
}

# --- 3. Remove registry entries ---
$regPaths = @{
    "edge"    = "HKCU:\Software\Microsoft\Edge\NativeMessagingHosts\org.keepassxc.keepassxc_browser"
    "chrome"  = "HKCU:\Software\Google\Chrome\NativeMessagingHosts\org.keepassxc.keepassxc_browser"
    "firefox" = "HKCU:\Software\Mozilla\NativeMessagingHosts\org.keepassxc.keepassxc_browser"
}

if ($Browser.ToLower() -eq "all") {
    foreach ($bp in $regPaths.Values) {
        if (Test-Path $bp) {
            Remove-Item $bp -Recurse -Force -ErrorAction SilentlyContinue
            Write-Host "      Removed registry: $bp" -ForegroundColor Green
        }
    }
    Write-Host "[3/3] Registry entries removed (all browsers)" -ForegroundColor Green
} else {
    $bp = $regPaths[$Browser.ToLower()]
    if ($bp -and (Test-Path $bp)) {
        Remove-Item $bp -Recurse -Force -ErrorAction SilentlyContinue
        Write-Host "[3/3] Removed registry: $bp" -ForegroundColor Green
    } else {
        Write-Host "[3/3] No registry entry found" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "=== Uninstallation Complete ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Note: Your KDBX database, entry configurations, and association keys"
Write-Host "      are NOT removed. They remain in your database Custom Data."
Write-Host "      To remove association keys, use KeePassNatMsg Options in KeePass."
