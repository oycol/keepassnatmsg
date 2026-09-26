$ErrorActionPreference = 'Stop'
$source = Join-Path $PSScriptRoot 'NativeProxy.cs'
$target = Join-Path $PSScriptRoot '..\KeePassNatMsg\Resources\keepassnatmsg-proxy.exe'
$csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe'
if (-not (Test-Path $csc)) { throw "Framework C# compiler unavailable: $csc" }
if (-not (Test-Path $source)) { throw 'Native proxy source missing' }
& $csc /nologo /target:exe /optimize+ "/out:$target" $source
if ($LASTEXITCODE -ne 0 -or -not (Test-Path $target)) { throw 'Native proxy compilation failed' }
Write-Host "Native proxy built from current source (SHA256): $((Get-FileHash $target -Algorithm SHA256).Hash.ToLowerInvariant())"
