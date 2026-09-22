# Standalone IPC test script for Windows test host
# Simulates Chrome Extension <-> keepassnatmsg-proxy.exe <-> KeePassNatMsg named pipe communication

[CmdletBinding()]
param(
    [string]$ProxyPath = "$env:LOCALAPPDATA\KeePassNatMsg\keepassnatmsg-proxy.exe"
)

$ErrorActionPreference = "Stop"

Write-Host "=== Testing Native Messaging Proxy stdio IPC with KeePass ===" -ForegroundColor Cyan

if (-not (Test-Path $ProxyPath)) {
    throw "Proxy executable not found at: $ProxyPath"
}

# 1. Prepare valid change-public-keys request
$fakePubBytes = New-Object byte[] 32
for ($i = 0; $i -lt 32; $i++) { $fakePubBytes[$i] = 1 }
$clientPublicKeyBase64 = [Convert]::ToBase64String($fakePubBytes)

$fakeClientId = New-Object byte[] 24
for ($i = 0; $i -lt 24; $i++) { $fakeClientId[$i] = 2 }
$clientIdBase64 = [Convert]::ToBase64String($fakeClientId)

$requestObj = @{
    action = "change-public-keys"
    publicKey = $clientPublicKeyBase64
    clientID = $clientIdBase64
}
$requestJson = $requestObj | ConvertTo-Json -Compress
$jsonBytes = [System.Text.Encoding]::UTF8.GetBytes($requestJson)
$len = $jsonBytes.Length
$lenBytes = [System.BitConverter]::GetBytes([int32]$len)

Write-Host "Sending action 'change-public-keys' ($len bytes): $requestJson"

$psi = New-Object System.Diagnostics.ProcessStartInfo
$psi.FileName = $ProxyPath
# Enable proxy logging with /log argument if supported
$psi.Arguments = "/log"
$psi.UseShellExecute = $false
$psi.RedirectStandardInput = $true
$psi.RedirectStandardOutput = $true
$psi.RedirectStandardError = $true
$psi.StandardErrorEncoding = [System.Text.Encoding]::Default
$psi.CreateNoWindow = $true

$proc = [System.Diagnostics.Process]::Start($psi)
$stdin = $proc.StandardInput.BaseStream
$stdout = $proc.StandardOutput.BaseStream

try {
    # Write length and message
    $stdin.Write($lenBytes, 0, 4)
    $stdin.Write($jsonBytes, 0, $len)
    $stdin.Flush()

    # Wait up to 5s for exit or response
    $respLenBytes = New-Object byte[] 4
    $task = $stdout.ReadAsync($respLenBytes, 0, 4)
    if (-not $task.Wait(5000)) {
        $err = $proc.StandardError.ReadToEnd()
        throw "Timeout waiting for proxy response. Stderr: $err"
    }

    $bytesRead = $task.Result
    if ($bytesRead -lt 4) {
        $err = $proc.StandardError.ReadToEnd()
        throw "Failed to read 4-byte header from proxy. Stderr: $err"
    }

    $respLen = [System.BitConverter]::ToInt32($respLenBytes, 0)
    Write-Host "Received response length header: $respLen bytes" -ForegroundColor Green

    $respBytes = New-Object byte[] $respLen
    $totalRead = 0
    while ($totalRead -lt $respLen) {
        $rTask = $stdout.ReadAsync($respBytes, $totalRead, $respLen - $totalRead)
        if (-not $rTask.Wait(5000)) { throw "Timeout reading response body from proxy." }
        $r = $rTask.Result
        if ($r -le 0) { break }
        $totalRead += $r
    }

    $responseJson = [System.Text.Encoding]::UTF8.GetString($respBytes, 0, $totalRead)
    Write-Host "Proxy Response:`n$responseJson" -ForegroundColor Green
}
finally {
    if (-not $proc.HasExited) {
        $proc.Kill()
    }
    $proc.Dispose()
}
