# KeePassNatMsg Install Script for Windows
# Compatible with KeePass 2.60 and KeePassXC-Browser 1.10.4
# Usage: Run this script in PowerShell as the current user (no admin required)

[CmdletBinding()]
param(
    [string]$KeePassPath = "C:\Program Files\KeePass Password Safe 2",
    [string]$Browser = "edge",
    [switch]$Force
)

$ErrorActionPreference = "Stop"

Write-Host "=== KeePassNatMsg Installer ===" -ForegroundColor Cyan
Write-Host "KeePass 2.x Plugin + Native Messaging Host for KeePassXC-Browser 1.10.4"
Write-Host ""

# --- 1. Verify KeePass installation ---
if (-not (Test-Path "$KeePassPath\KeePass.exe")) {
    Write-Error "KeePass.exe not found at: $KeePassPath`nPlease specify -KeePassPath"
    exit 1
}

Write-Host "[1/5] KeePass found at: $KeePassPath" -ForegroundColor Green

# --- 2. Install plugin ---
$pluginsDir = "$KeePassPath\Plugins"
if (-not (Test-Path $pluginsDir)) {
    New-Item -ItemType Directory -Force -Path $pluginsDir | Out-Null
}

$plgxFile = "$pluginsDir\KeePassNatMsg.plgx"
$dllFile = "$pluginsDir\KeePassNatMsg.dll"

# Check for existing installation
if ((Test-Path $plgxFile -or Test-Path $dllFile) -and -not $Force) {
    Write-Host "[2/5] KeePassNatMsg already installed. Use -Force to reinstall." -ForegroundColor Yellow
} else {
    # Remove old files
    Remove-Item $plgxFile -ErrorAction SilentlyContinue
    Remove-Item $dllFile -ErrorAction SilentlyContinue

    # Look for .plgx or .dll in script directory
    $scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
    $sourcePlgx = Join-Path $scriptDir "KeePassNatMsg.plgx"
    $sourceDll = Join-Path $scriptDir "KeePassNatMsg.dll"

    if (Test-Path $sourcePlgx) {
        Copy-Item $sourcePlgx $plgxFile -Force
        Write-Host "[2/5] Installed plugin (.plgx): $plgxFile" -ForegroundColor Green
    } elseif (Test-Path $sourceDll) {
        # Copy DLL and dependencies
        Copy-Item $sourceDll $dllFile -Force
        $newtonsoftDll = Join-Path $scriptDir "Newtonsoft.Json.dll"
        if (Test-Path $newtonsoftDll) {
            Copy-Item $newtonsoftDll "$pluginsDir\Newtonsoft.Json.dll" -Force
        }
        Write-Host "[2/5] Installed plugin (.dll): $dllFile" -ForegroundColor Green
    } else {
        Write-Warning "[2/5] No plugin file found in $scriptDir"
        Write-Host "        Expected: KeePassNatMsg.plgx or KeePassNatMsg.dll"
        Write-Host "        Download from GitHub Releases or build from source."
    }
}

# --- 3. Install Native Messaging Host ---
$proxyDir = "$env:LOCALAPPDATA\KeePassNatMsg"
if (-not (Test-Path $proxyDir)) {
    New-Item -ItemType Directory -Force -Path $proxyDir | Out-Null
}

# Create Native Messaging manifest
$proxyExe = "$proxyDir\keepassnatmsg-proxy.exe"
$manifest = @{
    name = "org.keepassxc.keepassxc_browser"
    description = "KeePassXC-Browser native messaging host (KeePassNatMsg)"
    type = "stdio"
    path = $proxyExe
}

# KeePassXC-Browser extension IDs for various browsers
switch ($Browser.ToLower()) {
    "edge" {
        $manifest.allowed_origins = @(
            "chrome-extension://usuarokccmpfpckckkfcdobhdaiglfik/"
        )
    }
    "chrome" {
        $manifest.allowed_origins = @(
            "chrome-extension://obcddimikignkfpophjabdkdggkodnnh/"
        )
    }
    "firefox" {
        $manifest.allowed_origins = @(
            "keepassxc-browser@keepassxc.org"
        )
    }
    "all" {
        $manifest.allowed_origins = @(
            "chrome-extension://usuarokccmpfpckckkfcdobhdaiglfik/",
            "chrome-extension://obcddimikignkfpophjabdkdggkodnnh/",
            "keepassxc-browser@keepassxc.org"
        )
    }
    default {
        $manifest.allowed_origins = @(
            "chrome-extension://usuarokccmpfpckckkfcdobhdaiglfik/"
        )
    }
}

$manifestJson = $manifest | ConvertTo-Json -Depth 3
$manifestFile = "$proxyDir\kpnm_$($Browser.ToLower()).json"
$manifestJson | Set-Content $manifestFile -Encoding UTF8

Write-Host "[3/5] Native Messaging manifest created: $manifestFile" -ForegroundColor Green

# --- 4. Register in Windows Registry ---
$regBase = "HKCU:\Software"

$regPaths = @{
    "edge"    = "Software\Microsoft\Edge\NativeMessagingHosts\org.keepassxc.keepassxc_browser"
    "chrome"  = "Software\Google\Chrome\NativeMessagingHosts\org.keepassxc.keepassxc_browser"
    "firefox" = "Software\Mozilla\NativeMessagingHosts\org.keepassxc.keepassxc_browser"
}

if ($Browser.ToLower() -eq "all") {
    foreach ($bp in $regPaths.Values) {
        $fullPath = "HKCU:\$bp"
        if (-not (Test-Path $fullPath)) {
            New-Item -Path $fullPath -Force | Out-Null
        }
        Set-ItemProperty -Path $fullPath -Name "(Default)" -Value $manifestFile
        Write-Host "        Registered: $bp" -ForegroundColor Gray
    }
} else {
    $bp = $regPaths[$Browser.ToLower()]
    if ($bp) {
        $fullPath = "HKCU:\$bp"
        if (-not (Test-Path $fullPath)) {
            New-Item -Path $fullPath -Force | Out-Null
        }
        Set-ItemProperty -Path $fullPath -Name "(Default)" -Value $manifestFile
        Write-Host "[4/5] Registered in registry: $bp" -ForegroundColor Green
    }
}

if ($Browser.ToLower() -eq "all") {
    Write-Host "[4/5] Registered all browsers in registry" -ForegroundColor Green
}

# --- 5. Verify ---
Write-Host "[5/5] Verification:" -ForegroundColor Cyan

$verifyOk = $true

if (Test-Path $plgxFile) {
    Write-Host "  Plugin: OK ($plgxFile)" -ForegroundColor Green
} elseif (Test-Path $dllFile) {
    Write-Host "  Plugin: OK ($dllFile)" -ForegroundColor Green
} else {
    Write-Host "  Plugin: MISSING" -ForegroundColor Red
    $verifyOk = $false
}

if (Test-Path $manifestFile) {
    Write-Host "  Manifest: OK ($manifestFile)" -ForegroundColor Green
} else {
    Write-Host "  Manifest: MISSING" -ForegroundColor Red
    $verifyOk = $false
}

if (Test-Path $proxyExe) {
    Write-Host "  Proxy: OK ($proxyExe)" -ForegroundColor Green
} else {
    Write-Host "  Proxy: NOT FOUND ($proxyExe)" -ForegroundColor Yellow
    Write-Host "          Download keepassnatmsg-proxy.exe from https://github.com/smorks/keepassnatmsg-proxy/releases"
}

Write-Host ""
Write-Host "=== Installation Complete ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:"
Write-Host "  1. Restart KeePass (if running)"
Write-Host "  2. Install KeePassXC-Browser 1.10.4 extension in your browser"
Write-Host "  3. Click Connect in the extension"
Write-Host "  4. Approve the association in KeePass"
Write-Host ""
Write-Host "Note: GUI authorization flow requires real Windows testing."
Write-Host "      CI-verified: build, protocol, crypto, URL matching, framing."
