# Diagnostics: read plugin.log and query get-logins via named pipe
# Run locally on the interactive Windows test host.

[CmdletBinding()]
param(
    [string]$Url = "https://login.microsoftonline.com/"
)

$pipeName = "keepassxc\$env:USERNAME\kpxc_server"
$logPath = "$env:LOCALAPPDATA\KeePassNatMsg\plugin.log"

Write-Host "=== Step 1: Plugin log availability ===" -ForegroundColor Cyan
Write-Host "  plugin.log present: $(Test-Path $logPath)"
Write-Host '  Raw log content withheld (may contain credentials or request payloads).'

Write-Host "`n=== Step 2: Direct Named Pipe key exchange (not a get-logins query) ===" -ForegroundColor Cyan

# 1. Connect pipe and do change-public-keys
$pipe = New-Object System.IO.Pipes.NamedPipeClientStream(".", $pipeName, [System.IO.Pipes.PipeDirection]::InOut)
try {
    $pipe.Connect(5000)
    Write-Host "Connected to named pipe" -ForegroundColor Green

    # Build fake keys
    $privKey = New-Object byte[] 32
    [System.Security.Cryptography.RNGCryptoServiceProvider]::Create().GetBytes($privKey)
    $pubKey = New-Object byte[] 32
    [System.Security.Cryptography.RNGCryptoServiceProvider]::Create().GetBytes($pubKey)
    $nonce1 = New-Object byte[] 24
    [System.Security.Cryptography.RNGCryptoServiceProvider]::Create().GetBytes($nonce1)
    $clientId = New-Object byte[] 24
    [System.Security.Cryptography.RNGCryptoServiceProvider]::Create().GetBytes($clientId)

    $pubBase64    = [Convert]::ToBase64String($pubKey)
    $nonce1Base64 = [Convert]::ToBase64String($nonce1)
    $clientBase64 = [Convert]::ToBase64String($clientId)

    # change-public-keys
    $req1 = @{action="change-public-keys"; publicKey=$pubBase64; nonce=$nonce1Base64; clientID=$clientBase64} | ConvertTo-Json -Compress
    $bytes1 = [System.Text.Encoding]::UTF8.GetBytes($req1)
    $pipe.Write($bytes1, 0, $bytes1.Length)
    $pipe.Flush()

    $buf = New-Object byte[] 65536
    $task = $pipe.ReadAsync($buf, 0, $buf.Length)
    if (-not $task.Wait(8000)) { throw "Timeout on change-public-keys" }
    $resp1Json = [System.Text.Encoding]::UTF8.GetString($buf, 0, $task.Result)
    $resp1 = $resp1Json | ConvertFrom-Json
    if ($resp1.success -ne "true") { throw 'change-public-keys failed (response withheld)' }
    Write-Host 'Direct named-pipe key exchange OK; this does not prove browser IPC or get-logins.' -ForegroundColor Green

    # get-logins requires authenticated NaCl box encryption; this script does not
    # issue it. Log lines (including RECV/RESP) cannot attribute traffic to Chrome.
    Write-Host 'get-logins and browser-origin communication UNVERIFIED.' -ForegroundColor Yellow
}
finally {
    $pipe.Dispose()
}

Write-Host "`n=== Step 3: KeePass matching config availability ===" -ForegroundColor Cyan
$kpConfigPaths = @(
    "$env:APPDATA\KeePass\KeePass.config.xml",
    "C:\Program Files\KeePass Password Safe 2\KeePass.config.xml",
    "$env:LOCALAPPDATA\KeePass\KeePass.config.xml"
)
$foundConfig = @($kpConfigPaths | Where-Object { Test-Path $_ }).Count -gt 0
Write-Host "KeePass config present: $foundConfig"
Write-Host 'Config values withheld; matching behavior is not verified by this check.'
