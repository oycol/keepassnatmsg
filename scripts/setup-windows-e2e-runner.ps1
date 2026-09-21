# KeePassNatMsg Windows Interactive E2E Runner Bootstrap
# Run this on the Windows test machine in an interactive admin desktop PowerShell session.
# Do NOT run from WinRM. Do NOT install the runner as a Windows service.

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$RegistrationToken,

    [string]$RepoUrl = "https://github.com/oycol/keepassnatmsg",
    [string]$RunnerName = "keepass-e2e-192-168-8-90",
    [string]$RunnerDir = "C:\actions-runner-keepassnatmsg",
    [string]$Labels = "windows,keepass-e2e,interactive,chrome",
    [switch]$SkipDependencyInstall
)

$ErrorActionPreference = "Stop"

function Write-Step([string]$Message) {
    Write-Host "`n==> $Message" -ForegroundColor Cyan
}

function Assert-InteractiveSession {
    Write-Step "Checking interactive desktop session"
    if (-not [Environment]::UserInteractive) {
        throw "This script must run in an interactive desktop session, not WinRM/service/session 0."
    }
    $sessionName = $env:SESSIONNAME
    Write-Host "User: $env:USERNAME"
    Write-Host "Session: $sessionName"
    if ($sessionName -eq "Services") {
        throw "Current session appears to be Session 0/Services. Log in via RDP/console and run PowerShell there."
    }
}

function Disable-LockAndSleepForTestSession {
    Write-Step "Disabling sleep/lock/display timeout for E2E session"
    powercfg /change standby-timeout-ac 0 | Out-Null
    powercfg /change monitor-timeout-ac 0 | Out-Null
    powercfg /change hibernate-timeout-ac 0 | Out-Null
    reg add "HKCU\Control Panel\Desktop" /v ScreenSaveActive /t REG_SZ /d 0 /f | Out-Null
    reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Policies\System" /v DisableLockWorkstation /t REG_DWORD /d 1 /f | Out-Null
    Write-Host "Power/session settings updated for current user."
}

function Test-CommandExists([string]$Name) {
    return [bool](Get-Command $Name -ErrorAction SilentlyContinue)
}

function Install-WithWinget([string]$Id, [string]$Name) {
    if (-not (Test-CommandExists winget)) {
        Write-Warning "winget not found. Please install $Name manually if missing."
        return
    }
    Write-Host "Installing/checking $Name via winget ($Id)..."
    winget install --id $Id -e --accept-package-agreements --accept-source-agreements --silent
}

function Ensure-Dependencies {
    if ($SkipDependencyInstall) {
        Write-Warning "Skipping dependency installation by request."
        return
    }

    Write-Step "Checking/installing dependencies"

    if (-not (Test-CommandExists git)) {
        Install-WithWinget -Id "Git.Git" -Name "Git"
    } else { Write-Host "Git: OK" }

    $chrome = @(
        "C:\Program Files\Google\Chrome\Application\chrome.exe",
        "C:\Program Files (x86)\Google\Chrome\Application\chrome.exe"
    ) | Where-Object { Test-Path $_ } | Select-Object -First 1
    if (-not $chrome) {
        Install-WithWinget -Id "Google.Chrome" -Name "Google Chrome"
    } else { Write-Host "Chrome: OK ($chrome)" }

    $keepass = @(
        "C:\Program Files\KeePass Password Safe 2\KeePass.exe",
        "C:\Program Files (x86)\KeePass Password Safe 2\KeePass.exe"
    ) | Where-Object { Test-Path $_ } | Select-Object -First 1
    if (-not $keepass) {
        Install-WithWinget -Id "DominikReichl.KeePass" -Name "KeePass 2.x"
    } else { Write-Host "KeePass: OK ($keepass)" }

    if (-not (Test-CommandExists node)) {
        Install-WithWinget -Id "OpenJS.NodeJS.LTS" -Name "Node.js LTS"
    } else { Write-Host "Node.js: OK" }

    if (-not (Test-CommandExists dotnet)) {
        Install-WithWinget -Id "Microsoft.DotNet.SDK.8" -Name ".NET SDK 8"
    } else { Write-Host ".NET SDK: OK" }

    if (-not (Test-CommandExists msbuild)) {
        Write-Warning "msbuild not found in PATH. Install Visual Studio Build Tools or Developer Command Prompt support if build step fails."
        if (Test-CommandExists winget) {
            Write-Host "Suggested command if needed: winget install --id Microsoft.VisualStudio.2022.BuildTools -e"
        }
    } else { Write-Host "MSBuild: OK" }
}

function Install-GitHubRunner {
    Write-Step "Installing/configuring GitHub Actions runner"

    $runnerVersion = "2.328.0"
    $runnerZip = "actions-runner-win-x64-$runnerVersion.zip"
    $runnerUrl = "https://github.com/actions/runner/releases/download/v$runnerVersion/$runnerZip"
    $runnerSha256 = "C25D80764188316813F73D5D9E08103878EBD682513F6BE2CA1AA16B9898E97D"

    New-Item -ItemType Directory -Force -Path $RunnerDir | Out-Null
    Set-Location $RunnerDir

    if (-not (Test-Path $runnerZip)) {
        Write-Host "Downloading runner $runnerVersion..."
        Invoke-WebRequest -Uri $runnerUrl -OutFile $runnerZip
    }

    $actualHash = (Get-FileHash -Path $runnerZip -Algorithm SHA256).Hash.ToUpper()
    if ($actualHash -ne $runnerSha256) {
        throw "Runner zip SHA256 mismatch. Expected $runnerSha256, got $actualHash"
    }

    if (-not (Test-Path ".\config.cmd")) {
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        [System.IO.Compression.ZipFile]::ExtractToDirectory((Join-Path $PWD $runnerZip), $PWD.Path)
    }

    if (Test-Path ".\.runner") {
        Write-Host "Existing runner config detected; reconfiguring with --replace."
    }

    & .\config.cmd --url $RepoUrl --token $RegistrationToken --name $RunnerName --labels $Labels --work _work --unattended --replace
    if ($LASTEXITCODE -ne 0) {
        throw "config.cmd failed with exit code $LASTEXITCODE"
    }

    Write-Host "Runner configured. Starting interactive runner now..." -ForegroundColor Green
    Write-Host "Keep this PowerShell window open. Do not run svc install." -ForegroundColor Yellow
    & .\run.cmd
}

Assert-InteractiveSession
Disable-LockAndSleepForTestSession
Ensure-Dependencies
Install-GitHubRunner
