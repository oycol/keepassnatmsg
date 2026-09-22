# Diagnostics: read plugin.log and query get-logins via named pipe
# For KeePass on Windows test host (192.168.8.90)

[CmdletBinding()]
param(
    [string]$Url = "https://login.microsoftonline.com/"
)

$pipeName = "keepassxc\$env:USERNAME\kpxc_server"
$logPath = "$env:LOCALAPPDATA\KeePassNatMsg\plugin.log"

Write-Host "=== Step 1: Plugin Log (last 50 lines) ===" -ForegroundColor Cyan
if (Test-Path $logPath) {
    Get-Content $logPath -Tail 50 | ForEach-Object { Write-Host "  $_" }
} else {
    Write-Host "  plugin.log not found" -ForegroundColor Yellow
}

Write-Host "`n=== Step 2: Named Pipe get-logins for: $Url ===" -ForegroundColor Cyan

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
    Write-Host "change-public-keys response: $resp1Json" -ForegroundColor Gray

    $resp1 = $resp1Json | ConvertFrom-Json
    if ($resp1.success -ne "true") { throw "change-public-keys failed: $resp1Json" }
    Write-Host "Key exchange OK, server pubKey: $($resp1.publicKey)" -ForegroundColor Green

    # --- NOTE: get-logins requires full NaCl box encryption.
    # We cannot do real nacl box in PowerShell without a library.
    # Instead, we trigger a get-logins via another TCP approach is not feasible.
    # However, the plugin.log already captures EVERY receive/response, so:
    # We print the last RECV lines related to microsoftonline from the log.
    Write-Host "`n=== Step 3: Search plugin.log for microsoftonline entries ===" -ForegroundColor Cyan
    if (Test-Path $logPath) {
        $lines = Get-Content $logPath
        $relevant = $lines | Where-Object { $_ -match "microsoftonline|RECV|RESP" }
        if ($relevant) {
            $relevant | Select-Object -Last 100 | ForEach-Object { Write-Host "  $_" }
        } else {
            Write-Host "  No microsoftonline entries found in plugin.log" -ForegroundColor Yellow
            Write-Host "  Showing all RECV/RESP from last 100 lines:" -ForegroundColor Yellow
            $lines | Select-Object -Last 100 | Where-Object { $_ -match "RECV|RESP" } | ForEach-Object { Write-Host "  $_" }
        }
    }
}
finally {
    $pipe.Dispose()
}

Write-Host "`n=== Step 4: KeePass Matching Config ===" -ForegroundColor Cyan
# Read KeePass config for matching options
$kpConfigPaths = @(
    "$env:APPDATA\KeePass\KeePass.config.xml",
    "C:\Program Files\KeePass Password Safe 2\KeePass.config.xml",
    "$env:LOCALAPPDATA\KeePass\KeePass.config.xml"
)
foreach ($cp in $kpConfigPaths) {
    if (Test-Path $cp) {
        Write-Host "KeePass config at: $cp" -ForegroundColor Green
        # Look for KeePassNatMsg settings
        $content = Get-Content $cp -Raw
        if ($content -match "KeePassNatMsg") {
            $content | Select-String "SpecificMatchingOnly|HideExpired|AlwaysAllow|KeePassNatMsg" -AllMatches | ForEach-Object { Write-Host "  $_" }
        }
        break
    }
}
