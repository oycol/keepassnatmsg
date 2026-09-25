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

    Write-Host "Sending framed request ($($bytes.Length) bytes)"
    $requestHeader = [BitConverter]::GetBytes([int]$bytes.Length)
    $pipe.Write($requestHeader, 0, 4)
    $pipe.Write($bytes, 0, $bytes.Length)
    $pipe.Flush()

    Write-Host "Reading framed response from pipe..."
    $respHeader = New-Object byte[] 4
    $headerRead = 0
    while ($headerRead -lt 4) {
        $readTask = $pipe.ReadAsync($respHeader, $headerRead, 4 - $headerRead)
        if (-not $readTask.Wait(8000) -or $readTask.Result -le 0) { throw 'Incomplete response header' }
        $headerRead += $readTask.Result
    }
    $respSize = [BitConverter]::ToInt32($respHeader, 0)
    if ($respSize -le 0 -or $respSize -gt 10485760) { throw 'Invalid response length' }
    $respBuffer = New-Object byte[] $respSize
    $bytesRead = 0
    while ($bytesRead -lt $respSize) {
        $readTask = $pipe.ReadAsync($respBuffer, $bytesRead, $respSize - $bytesRead)
        if (-not $readTask.Wait(8000) -or $readTask.Result -le 0) { throw 'Incomplete response body' }
        $bytesRead += $readTask.Result
    }
    $responseJson = [System.Text.Encoding]::UTF8.GetString($respBuffer)
    Write-Host "Received framed response ($bytesRead bytes)" -ForegroundColor Green

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

    # A plaintext lock request must be rejected before it can lock the open test DB.
    $lockJson = '{"action":"lock-database","clientID":"' + $clientBase64 + '","nonce":"' + $nonceBase64 + '"}'
    $lockBytes = [System.Text.Encoding]::UTF8.GetBytes($lockJson)
    $pipe.Write(([BitConverter]::GetBytes([int]$lockBytes.Length)), 0, 4)
    $pipe.Write($lockBytes, 0, $lockBytes.Length)
    $pipe.Flush()
    $lockHeader = New-Object byte[] 4
    $headerRead = 0
    while ($headerRead -lt 4) {
        $lockRead = $pipe.ReadAsync($lockHeader, $headerRead, 4 - $headerRead)
        if (-not $lockRead.Wait(8000) -or $lockRead.Result -le 0) { throw 'No response to unauthenticated lock-database request' }
        $headerRead += $lockRead.Result
    }
    $lockSize = [BitConverter]::ToInt32($lockHeader, 0)
    if ($lockSize -le 0 -or $lockSize -gt 10485760) { throw 'Invalid lock response length' }
    $lockBuffer = New-Object byte[] $lockSize
    $lockReadCount = 0
    while ($lockReadCount -lt $lockSize) {
        $lockRead = $pipe.ReadAsync($lockBuffer, $lockReadCount, $lockSize - $lockReadCount)
        if (-not $lockRead.Wait(8000) -or $lockRead.Result -le 0) { throw 'Incomplete lock response' }
        $lockReadCount += $lockRead.Result
    }
    $lockResponse = [System.Text.Encoding]::UTF8.GetString($lockBuffer) | ConvertFrom-Json
    if ($lockResponse.action -ne 'lock-database' -or $lockResponse.errorCode -ne 4) {
        throw 'Unauthenticated lock-database was not rejected'
    }
    $hashJson = '{"action":"get-databasehash","clientID":"' + $clientBase64 + '","nonce":"' + $nonceBase64 + '"}'
    $hashBytes = [System.Text.Encoding]::UTF8.GetBytes($hashJson)
    $hashFrame = [BitConverter]::GetBytes([int]$hashBytes.Length)
    $pipe.Write($hashFrame, 0, 4)
    $pipe.Write($hashBytes, 0, $hashBytes.Length)
    $pipe.Flush()
    $hashHeader = New-Object byte[] 4
    $hashHeaderRead = 0
    while ($hashHeaderRead -lt 4) {
        $hashRead = $pipe.ReadAsync($hashHeader, $hashHeaderRead, 4 - $hashHeaderRead)
        if (-not $hashRead.Wait(8000) -or $hashRead.Result -le 0) { throw 'Database became unavailable after denied lock' }
        $hashHeaderRead += $hashRead.Result
    }
    $hashSize = [BitConverter]::ToInt32($hashHeader, 0)
    if ($hashSize -le 0 -or $hashSize -gt 10485760) { throw 'Invalid response after denied lock' }
    $hashBody = New-Object byte[] $hashSize
    $hashBodyRead = 0
    while ($hashBodyRead -lt $hashSize) {
        $hashRead = $pipe.ReadAsync($hashBody, $hashBodyRead, $hashSize - $hashBodyRead)
        if (-not $hashRead.Wait(8000) -or $hashRead.Result -le 0) { throw 'Incomplete response after denied lock' }
        $hashBodyRead += $hashRead.Result
    }
    $hashResponse = [System.Text.Encoding]::UTF8.GetString($hashBody) | ConvertFrom-Json
    if ($hashResponse.errorCode -eq 1) { throw 'Unauthenticated request locked the database' }
    Write-Host 'Unauthenticated lock-database request rejected without locking the database.' -ForegroundColor Green
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
