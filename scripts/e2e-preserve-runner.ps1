param(
  [ValidateSet('Backup','Restore')][string]$Action,
  [string]$StateDir = "$env:RUNNER_TEMP\keepassnatmsg-e2e-preserve"
)
$ErrorActionPreference = 'Stop'
$stateFile = Join-Path $StateDir 'state.json'
$fixtureMarker = Join-Path $StateDir 'fixture-path.txt'
$pluginDir = 'C:\Program Files\KeePass Password Safe 2\Plugins'
$nativeDir = Join-Path $env:LOCALAPPDATA 'KeePassNatMsg'
$testDir = 'C:\KeePassNatMsg-E2E'
$targets = @(
  (Join-Path $pluginDir 'KeePassNatMsg.plgx'),
  (Join-Path $pluginDir 'KeePassNatMsg.dll'),
  (Join-Path $pluginDir 'Newtonsoft.Json.dll'),
  (Join-Path $nativeDir 'org.keepassxc.keepassxc_browser.json'),
  (Join-Path $nativeDir 'keepassnatmsg-proxy.exe'),
  (Join-Path $nativeDir 'plugin.log'),
  (Join-Path $nativeDir 'proxy.log'),
  (Join-Path $testDir 'test.kdbx'),
  (Join-Path $env:APPDATA 'KeePass\KeePass.config.xml'),
  (Join-Path $env:APPDATA 'KeePass\KeePass.config.xml.bak')
)
$regKeys = @(
  'HKCU:\Software\Google\Chrome\NativeMessagingHosts\org.keepassxc.keepassxc_browser',
  'HKCU:\Software\Microsoft\Edge\NativeMessagingHosts\org.keepassxc.keepassxc_browser'
)
function Get-RegSnapshot($key) {
  if (-not (Test-Path -LiteralPath $key)) { return @{ exists = $false; value = $null } }
  return @{ exists = $true; value = (Get-Item -LiteralPath $key).GetValue('') }
}
if ($Action -eq 'Backup') {
  if (Test-Path $stateFile) { throw 'An earlier E2E backup exists; restore it first before another test' }
  if (@(Get-Process KeePass -ErrorAction SilentlyContinue).Count -gt 0) { throw 'KeePass is already running; refusing to modify its session' }
  New-Item -ItemType Directory -Path $StateDir -Force | Out-Null
  $files = @()
  for ($i=0; $i -lt $targets.Count; $i++) {
    $p = $targets[$i]
    $exists = Test-Path -LiteralPath $p -PathType Leaf
    if ($exists) {
      $copy = Join-Path $StateDir ("file-$i")
      Copy-Item -LiteralPath $p -Destination $copy -ErrorAction Stop
      if ((Get-FileHash -LiteralPath $p -Algorithm SHA256).Hash -ne (Get-FileHash -LiteralPath $copy -Algorithm SHA256).Hash) { throw 'Backup hash mismatch' }
    }
    $files += @{ path=$p; exists=[bool]$exists; copy=("file-$i") }
  }
  $registry = @()
  foreach ($key in $regKeys) { $registry += @{ path=$key; snapshot=(Get-RegSnapshot $key) } }
  @{ files=$files; registry=$registry; completed=$true } | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $stateFile -Encoding UTF8
  Write-Host 'E2E target files and Native Messaging registration backed up with hashes.'
  return
}
if (-not (Test-Path -LiteralPath $stateFile)) { throw 'No complete E2E backup exists; refusing blind cleanup' }
$state = Get-Content -LiteralPath $stateFile -Raw | ConvertFrom-Json
if (-not $state.completed) { throw 'Incomplete E2E backup' }
$failures = @()
if (Test-Path -LiteralPath $fixtureMarker) {
  try {
    $fixture = (Get-Content -LiteralPath $fixtureMarker -Raw).Trim()
    if ([IO.Path]::GetFileName($fixture) -notmatch '^keepass-cidr-e2e-[a-f0-9]{32}\.kdbx$' -or [IO.Path]::GetDirectoryName($fixture) -ne $testDir) { throw 'Invalid fixture marker' }
    Remove-Item -LiteralPath $fixture -Force -ErrorAction SilentlyContinue
    if (Test-Path -LiteralPath $fixture) { throw 'Synthetic fixture still exists' }
  } catch { $failures += 'synthetic database cleanup failed' }
}
foreach ($file in $state.files) {
  try {
    if ($file.exists) {
      New-Item -ItemType Directory -Path (Split-Path $file.path) -Force | Out-Null
      Copy-Item -LiteralPath (Join-Path $StateDir $file.copy) -Destination $file.path -Force -ErrorAction Stop
      if ((Get-FileHash -LiteralPath $file.path -Algorithm SHA256).Hash -ne (Get-FileHash -LiteralPath (Join-Path $StateDir $file.copy) -Algorithm SHA256).Hash) { throw 'Restored hash mismatch' }
    } else {
      Remove-Item -LiteralPath $file.path -Force -ErrorAction Stop
      if (Test-Path -LiteralPath $file.path) { throw 'Newly created file still exists' }
    }
  } catch {
    if (-not $file.exists -and -not (Test-Path -LiteralPath $file.path)) { continue }
    $failures += ('file-' + [string]$file.copy + ' restore failure')
  }
}
foreach ($item in $state.registry) {
  try {
    if ($item.snapshot.exists) {
      New-Item -Path $item.path -Force | Out-Null
      (Get-Item -LiteralPath $item.path).SetValue('', [string]$item.snapshot.value)
    } else {
      Remove-Item -LiteralPath $item.path -Recurse -Force -ErrorAction SilentlyContinue
    }
    $actual = Get-RegSnapshot $item.path
    if ([bool]$actual.exists -ne [bool]$item.snapshot.exists -or ($item.snapshot.exists -and [string]$actual.value -cne [string]$item.snapshot.value)) { throw 'Registry read-back mismatch' }
  } catch { $failures += ('registry-' + [string]([array]::IndexOf($state.registry, $item)) + ' restore failure') }
}
if ($failures.Count) { throw ('Restore incomplete (' + $failures.Count + '): ' + ($failures -join ', ') + '; backup retained on runner') }
Remove-Item -LiteralPath $StateDir -Recurse -Force
Write-Host 'E2E target files and Native Messaging registration restored and verified.'
