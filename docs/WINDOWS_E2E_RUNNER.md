# Windows Interactive Self-hosted Runner Setup

This project uses a real Windows desktop machine for KeePass + Chrome + Native Messaging E2E.

## Scope

- Runner target: Windows test machine `192.168.8.90`
- GitHub scope: repository-level runner for `oycol/keepassnatmsg`
- Reason: `oycol` is a personal GitHub account. GitHub account-wide shared runners are not available like organization runners; repo-level runner is the practical scope.

## Important Rules

- Run the runner in an already logged-in interactive desktop session.
- Start it with `run.cmd`.
- Do **not** install it as a Windows service for GUI tests.
- Do **not** use `svc install` / `svc start` for the E2E runner.
- Keep the desktop unlocked while E2E runs.
- Disable sleep/lock/screen timeout for the test session.
- Do not store the Windows password in this repo, workflow, logs, release notes, or skills.

## Labels

Use these runner labels:

```text
windows,keepass-e2e,interactive,chrome
```

The workflow selects:

```yaml
runs-on: [self-hosted, windows, keepass-e2e, interactive, chrome]
```

## Setup Commands

Generate a fresh one-hour registration token from GitHub, then run the setup commands on the Windows test machine in PowerShell.

```powershell
mkdir C:\actions-runner-keepassnatmsg
cd C:\actions-runner-keepassnatmsg
Invoke-WebRequest -Uri https://github.com/actions/runner/releases/download/v2.328.0/actions-runner-win-x64-2.328.0.zip -OutFile actions-runner-win-x64-2.328.0.zip
if ((Get-FileHash -Path actions-runner-win-x64-2.328.0.zip -Algorithm SHA256).Hash.ToUpper() -ne 'C25D80764188316813F73D5D9E08103878EBD682513F6BE2CA1AA16B9898E97D') { throw 'Runner zip SHA256 mismatch' }
Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::ExtractToDirectory((Join-Path $PWD 'actions-runner-win-x64-2.328.0.zip'), $PWD.Path)
.\config.cmd --url https://github.com/oycol/keepassnatmsg --token <REGISTRATION_TOKEN> --name keepass-e2e-192-168-8-90 --labels windows,keepass-e2e,interactive,chrome --work _work --unattended --replace
.\run.cmd
```

## Trigger

After the runner is online, manually run:

```text
Actions → Windows Interactive E2E → Run workflow
```

## Security

The E2E workflow is `workflow_dispatch` only. Do not enable pull-request-triggered execution on this self-hosted runner.
