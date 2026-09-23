# Direct Named Pipe test script for Windows test host
# Connects directly to \\.\pipe\keepassxc\admin\kpxc_server to test KeePassNatMsg plugin protocol

$ErrorActionPreference = "Stop"

Write-Host "=== Direct Named Pipe Test with KeePassNatMsg ===" -ForegroundColor Cyan

$pipeName = "keepassxc\$env:USERNAME\kpxc_server"
Write-Host "Connecting to named pipe: $pipeName"

$pipe = New-Object System.IO.Pipes.NamedPipeClientStream(".", $pipeName, [System.IO.Pipes.PipeDirection]::InOut, [System.IO.Pipes.PipeOptions]::None)

try {
    $pipe.Connect(5000)
    Write-Host "Connected to KeePassNatMsg Named Pipe successfully!" -ForegroundColor Green

    # Prepare action 'change-public-keys'
    $fakePubBytes = New-Object byte[] 32
    for ($i = 0; $i -lt 32; $i++) { $fakePubBytes[$i] = 1 }
    $pubBase64 = [Convert]::ToBase64String($fakePubBytes)

    $fakeNonceBytes = New-Object byte[] 24
    for ($i = 0; $i -lt 24; $i++) { $fakeNonceBytes[$i] = 3 }
    $nonceBase64 = [Convert]::ToBase64String($fakeNonceBytes)

    $fakeClientId = New-Object byte[] 24
    for ($i = 0; $i -lt 24; $i++) { $fakeClientId[$i] = 2 }
    $clientBase64 = [Convert]::ToBase64String($fakeClientId)

    $reqObj = @{
        action = "change-public-keys"
        publicKey = $pubBase64
        nonce = $nonceBase64
        clientID = $clientBase64
    }
    $json = $reqObj | ConvertTo-Json -Compress
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($json)

    Write-Host "Sending request ($($bytes.Length) bytes): $json"
    $pipe.Write($bytes, 0, $bytes.Length)
    $pipe.Flush()

    Write-Host "Reading response from pipe..."
    $respBuffer = New-Object byte[] 4096
    $pipe.ReadTimeout = 8000
    try {
        $bytesRead = $pipe.Read($respBuffer, 0, $respBuffer.Length)
    } catch {
        throw "Read timed out or failed: $_"
    }
    Write-Host "Read $bytesRead bytes from pipe!" -ForegroundColor Green

    $responseJson = [System.Text.Encoding]::UTF8.GetString($respBuffer, 0, $bytesRead)
    Write-Host "KeePassNatMsg Raw Response:`n$responseJson" -ForegroundColor Green

    # Validate response fields
    if (-not ($responseJson -match '"version"\s*:\s*"2\.7\.0"')) {
        throw "Response does not contain protocol version 2.7.0!"
    }
    if (-not ($responseJson -match '"publicKey"')) {
        throw "Response does not contain publicKey!"
    }
    if (-not ($responseJson -match '"success"\s*:\s*"true"')) {
        throw "Response does not contain success: true!"
    }

    Write-Host "`nDIRECT NAMED PIPE PROTOCOL ROUNDTRIP: 100% PASSED!" -ForegroundColor Green
}
finally {
    if ($pipe) {
        $pipe.Dispose()
    }
    $logPath = "$env:LOCALAPPDATA\KeePassNatMsg\plugin.log"
    if (Test-Path $logPath) {
        Write-Host "`n--- plugin.log from test-direct-pipe ---" -ForegroundColor Yellow
        Get-Content $logPath -Tail 30 | ForEach-Object { Write-Host "  $_" }
    }
}
