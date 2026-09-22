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

# 1. Native Messaging uses 4-byte little-endian message length prefix followed by JSON
$requestJson = '{"action":"change-public-keys","publicKey":"2a2a2a2a2a2a2a2a2a2a2a2a2a2a2a2a2a2a2a2a2a2a2a2a2a2a2a2a2a2a2a2a"}'
$jsonBytes = [System.Text.Encoding]::UTF8.GetBytes($requestJson)
$len = $jsonBytes.Length
$lenBytes = [System.BitConverter]::GetBytes([int32]$len)

Write-Host "Sending action 'change-public-keys' ($len bytes) to proxy..."

$psi = New-Object System.Diagnostics.ProcessStartInfo
$psi.FileName = $ProxyPath
$psi.UseShellExecute = $false
$psi.RedirectStandardInput = $true
$psi.RedirectStandardOutput = $true
$psi.RedirectStandardError = $true
$psi.CreateNoWindow = $true

$proc = [System.Diagnostics.Process]::Start($psi)
$stdin = $proc.StandardInput.BaseStream
$stdout = $proc.StandardOutput.BaseStream

# Write length and message
$stdin.Write($lenBytes, 0, 4)
$stdin.Write($jsonBytes, 0, $len)
$stdin.Flush()

# Read 4-byte response length
$respLenBytes = New-Object byte[] 4
$bytesRead = $stdout.Read($respLenBytes, 0, 4)
if ($bytesRead -lt 4) {
    $err = $proc.StandardError.ReadToEnd()
    throw "Failed to read response length from proxy. Stderr: $err"
}

$respLen = [System.BitConverter]::ToInt32($respLenBytes, 0)
Write-Host "Received response length: $respLen bytes" -ForegroundColor Green

# Read response JSON
$respBytes = New-Object byte[] $respLen
$totalRead = 0
while ($totalRead -lt $respLen) {
    $read = $stdout.Read($respBytes, $totalRead, $respLen - $totalRead)
    if ($read -le 0) { break }
    $totalRead += $read
}

$responseJson = [System.Text.Encoding]::UTF8.GetString($respBytes, 0, $totalRead)
Write-Host "Proxy Response:`n$responseJson" -ForegroundColor Green

$proc.Kill()
$proc.Dispose()

# Validate JSON response
if (-not ($responseJson -match '"version"\s*:\s*"2\.7\.0"')) {
    throw "Response does not contain protocol version 2.7.0!"
}
if (-not ($responseJson -match '"publicKey"')) {
    throw "Response does not contain publicKey!"
}

Write-Host "`nAll Native Messaging IPC protocol assertions PASSED on Windows!" -ForegroundColor Green
