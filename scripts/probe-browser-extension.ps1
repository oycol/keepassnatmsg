param([string]$OutputDir = 'e2e-artifacts')
$ErrorActionPreference = 'Stop'
$expected = 'd5b780e28870deb8da260311bf141ac3a7a88d142b4b9e5c58156270c10ea3f7'
$zip = Join-Path $env:TEMP 'kpxc-browser.zip'
if (-not (Test-Path $zip) -or (Get-FileHash $zip -Algorithm SHA256).Hash.ToLowerInvariant() -ne $expected) { throw 'Verified KeePassXC-Browser 1.10.4 archive not available on runner' }
$browserRoot = Join-Path $env:TEMP 'keepass-playwright-browsers'
$env:PLAYWRIGHT_BROWSERS_PATH = $browserRoot
$chrome = Get-ChildItem -Path $browserRoot -Filter 'chrome.exe' -Recurse -ErrorAction SilentlyContinue | Where-Object { $_.FullName -match 'chromium-' } | Select-Object -First 1 -ExpandProperty FullName
if (-not $chrome) { throw 'Isolated Playwright Chromium executable not available' }
$root = Join-Path $env:TEMP ('keepass-browser-probe-' + [Guid]::NewGuid().ToString('N'))
$extension = Join-Path $root 'extension'
$profile = Join-Path $root 'profile'
New-Item -ItemType Directory -Path $extension, $profile -Force | Out-Null
$process = $null
try {
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [System.IO.Compression.ZipFile]::ExtractToDirectory($zip, $extension)
    $manifest = Get-Content (Join-Path $extension 'manifest.json') -Raw | ConvertFrom-Json
    if ($manifest.version -ne '1.10.4' -or -not (@($manifest.permissions) -contains 'nativeMessaging')) { throw 'Extension version or nativeMessaging permission mismatch' }
    $args = @("--user-data-dir=$profile", "--disable-extensions-except=$extension", "--load-extension=$extension", '--remote-debugging-port=0', '--no-first-run', '--no-default-browser-check', 'about:blank')
    $process = Start-Process -FilePath $chrome -ArgumentList $args -PassThru
    $portFile = Join-Path $profile 'DevToolsActivePort'
    $targets = @()
    for ($i=0; $i -lt 30; $i++) {
        if ($process.HasExited) { throw 'Isolated Chrome exited before exposing extension worker' }
        if (Test-Path $portFile) {
            $port = [int]((Get-Content $portFile -TotalCount 1))
            try { $targets = @(Invoke-RestMethod -Uri "http://127.0.0.1:$port/json/list" -TimeoutSec 2) } catch { }
            if (@($targets | Where-Object { $_.type -eq 'service_worker' -and $_.url -like 'chrome-extension://*/background/background_service.js' }).Count -gt 0) { break }
        }
        Start-Sleep -Milliseconds 500
    }
    $workers = @($targets | Where-Object { $_.type -eq 'service_worker' -and $_.url -like 'chrome-extension://*/background/background_service.js' })
    if ($workers.Count -ne 1) { throw 'Official 1.10.4 extension worker did not load in isolated Chrome profile' }
    $id = ([Uri]$workers[0].url).Host
    if ($id -notmatch '^[a-p]{32}$') { throw 'Extension runtime ID was invalid' }
    New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
    [ordered]@{ version='1.10.4'; zipSha256=$expected; extensionLoaded=$true; runtimeId=$id; profileIsolated=$true; nativeMessagingVerified=$false; getLoginsVerified=$false } | ConvertTo-Json | Set-Content (Join-Path $OutputDir 'extension-probe.json') -Encoding UTF8
    Write-Host "Verified isolated extension service worker; runtime ID=$id. Native messaging and get-logins remain unverified."
}
finally {
    if ($process -and -not $process.HasExited) { Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue }
    if (Test-Path $root) { Remove-Item -Path $root -Recurse -Force -ErrorAction SilentlyContinue }
}
