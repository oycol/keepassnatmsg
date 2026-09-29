# Windows UI Automation + Win32 approval for the disposable KeePass E2E database only.
param(
    [Parameter(Mandatory=$true)][ValidateSet('association','access')][string]$Phase,
    [Parameter(Mandatory=$true)][int]$KeePassPid,
    [Parameter(Mandatory=$true)][string]$DatabasePath,
    [Parameter(Mandatory=$true)][string]$ExpectedHost,
    [Parameter(Mandatory=$true)][string]$ExpectedTitle,
    [Parameter(Mandatory=$true)][string]$AssociationName
)
$ErrorActionPreference = 'Stop'
try {
    Add-Type -AssemblyName UIAutomationClient
    Add-Type -AssemblyName UIAutomationTypes
    Add-Type -AssemblyName System.Windows.Forms

    # Define minimal Win32 messaging helpers for reliable control manipulation
    Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
public static class Win32Native {
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, string lParam);
    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern bool SetWindowText(IntPtr hWnd, string text);
    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")]
    public static extern IntPtr SetFocus(IntPtr hWnd);
}
'@

    $db = [IO.Path]::GetFullPath($DatabasePath)
    if ($db -notmatch '(?i)[\\/]keepass-cidr-e2e-[a-f0-9]{16,}\.kdbx$' -or -not [IO.File]::Exists($db) -or ([DateTime]::UtcNow - [IO.File]::GetCreationTimeUtc($db)).TotalHours -ge 1) { throw 'Unsafe fixture path' }
    $proc = Get-CimInstance Win32_Process -Filter "ProcessId=$KeePassPid"
    if (-not $proc -or [IO.Path]::GetFileName($proc.ExecutablePath) -ne 'KeePass.exe' -or -not $proc.CommandLine.Contains($db)) { throw 'KeePass process is not bound to fixture' }
    $root = [System.Windows.Automation.AutomationElement]::RootElement
    $scope = [System.Windows.Automation.TreeScope]::Children
    $condition = [System.Windows.Automation.Condition]::TrueCondition
    $title = if ($Phase -eq 'association') { 'KeePassNatMsg: Confirm New Key Association' } else { 'KeePassNatMsg: Confirm Access' }
    $deadline = [DateTime]::UtcNow.AddSeconds(35)
    $window = $null
    while ([DateTime]::UtcNow -lt $deadline) {
        $matches = @($root.FindAll($scope, $condition) | Where-Object { $_.Current.Name -eq $title -and $_.Current.ProcessId -eq $KeePassPid })
        if ($matches.Count -gt 1) { throw 'Ambiguous approval windows' }
        if ($matches.Count -eq 1) { $window = $matches[0]; break }
        Start-Sleep -Milliseconds 200
    }
    if (-not $window) {
        $seen = @($root.FindAll($scope, $condition) | Where-Object { $_.Current.ProcessId -eq $KeePassPid } | ForEach-Object { $_.Current.Name }) | Select-Object -First 8
        [Console]::Error.WriteLine("approval-window-not-found title=$title pid=$KeePassPid windowsOfPid=$($seen -join '|')")
        exit 1
    }

    $formHwnd = [IntPtr]$window.Current.NativeWindowHandle
    [Win32Native]::SetForegroundWindow($formHwnd) | Out-Null

    $controlsDeadline = [DateTime]::UtcNow.AddSeconds(20)
    $treeScope = [System.Windows.Automation.TreeScope]::Descendants

    if ($Phase -eq 'association') {
        $field = $null
        $saveBtn = $null
        while ([DateTime]::UtcNow -lt $controlsDeadline) {
            $all = @($window.FindAll($treeScope, $condition))
            $fingerprints = @($all | Where-Object { $_.Current.ControlType -eq [System.Windows.Automation.ControlType]::Text -and $_.Current.Name -match '^([0-9A-F]{2}:){7}[0-9A-F]{2}$' })

            # The textbox in WinForms often has empty Name and no special AutomationId.
            # Match by:
            # 1. AutomationId 'KeyName' or Name 'KeyName'
            # 2. Or Any enabled element that is NOT a button, NOT a label, NOT a form itself, with a valid NativeWindowHandle
            $candidates = @($all | Where-Object {
                $_.Current.AutomationId -eq 'KeyName' -or
                $_.Current.Name -eq 'KeyName' -or
                ($_.Current.ClassName -and $_.Current.ClassName -like '*Edit*')
            })
            if ($candidates.Count -eq 0) {
                # Fallback: find any child pane with valid HWND that is not Save/Cancel
                $candidates = @($all | Where-Object {
                    $_.Current.NativeWindowHandle -ne 0 -and
                    $_.Current.NativeWindowHandle -ne $formHwnd.ToInt32() -and
                    $_.Current.Name -notin @('Save', '&Save', 'Cancel', '&Cancel') -and
                    $_.Current.ControlType.ProgrammaticName -in @('ControlType.Edit', 'ControlType.Pane', 'ControlType.Custom') -and
                    $_.Current.IsEnabled
                })
            }
            if ($candidates.Count -ge 1) {
                $field = $candidates[0]
            }

            # Find Save button
            $btnCandidates = @($all | Where-Object {
                ($_.Current.AutomationId -eq 'Save' -or $_.Current.Name -in @('Save', '&Save', 'SaveButton')) -and $_.Current.IsEnabled
            })
            if ($btnCandidates.Count -ge 1) {
                $saveBtn = $btnCandidates[0]
            }

            if ($field -and $saveBtn -and $fingerprints.Count -ge 1) {
                break
            }
            Start-Sleep -Milliseconds 250
        }

        if (-not $field) {
            $inventory = @($all | ForEach-Object { "$($_.Current.ControlType.ProgrammaticName):id=$($_.Current.AutomationId):name=$($_.Current.Name):class=$($_.Current.ClassName):enabled=$($_.Current.IsEnabled)" }) | Select-Object -First 20
            [Console]::Error.WriteLine("control-inventory-field-failed list=$($inventory -join ' | ')")
            throw 'KeyName editor not found within deadline'
        }
        if (-not $saveBtn) {
            $inventory = @($all | ForEach-Object { "$($_.Current.ControlType.ProgrammaticName):id=$($_.Current.AutomationId):name=$($_.Current.Name):class=$($_.Current.ClassName):enabled=$($_.Current.IsEnabled)" }) | Select-Object -First 20
            [Console]::Error.WriteLine("control-inventory-save-failed list=$($inventory -join ' | ')")
            throw 'Save button not found within deadline'
        }

        # Set value: try ValuePattern, fallback to Win32 WM_SETTEXT + SetWindowText
        $vPattern = $null
        $fieldHwnd = [IntPtr]$field.Current.NativeWindowHandle
        if ($field.TryGetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern, [ref]$vPattern)) {
            $vPattern.SetValue($AssociationName)
        } elseif ($fieldHwnd -ne [IntPtr]::Zero) {
            [Win32Native]::SetFocus($fieldHwnd) | Out-Null
            [Win32Native]::SetWindowText($fieldHwnd, $AssociationName) | Out-Null
            [Win32Native]::SendMessage($fieldHwnd, 0x000C, [IntPtr]::Zero, $AssociationName) | Out-Null
        } else {
            throw 'KeyName editor has no pattern and no window handle'
        }

        Start-Sleep -Milliseconds 150

        # Click Save: try InvokePattern, fallback to Win32 BM_CLICK (0x00F5)
        $invokePattern = $null
        $saveHwnd = [IntPtr]$saveBtn.Current.NativeWindowHandle
        if ($saveBtn.TryGetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern, [ref]$invokePattern)) {
            $invokePattern.Invoke()
        } elseif ($saveHwnd -ne [IntPtr]::Zero) {
            [Win32Native]::SendMessage($saveHwnd, 0x00F5, [IntPtr]::Zero, [IntPtr]::Zero) | Out-Null
        } else {
            # As last resort, press ENTER on the form (AcceptButton is Save)
            [System.Windows.Forms.SendKeys]::SendWait('{ENTER}')
        }
    } else {
        # access phase: verify the expected host and single-item prompt; the
        # extension response and page fill verify the synthetic entry identity.
        if ([string]::IsNullOrWhiteSpace($ExpectedHost)) {
            throw 'ExpectedHost must be specified for access phase'
        }
        if ([string]::IsNullOrWhiteSpace($ExpectedTitle)) {
            throw 'ExpectedTitle must be specified for access phase'
        }

        $allowBtn = $null
        $hostVerified = $false

        while ([DateTime]::UtcNow -lt $controlsDeadline) {
            $all = @($window.FindAll($treeScope, $condition))

            # 1. Exact expected host label verification (singular "above item." for exactly 1 item)
            $hostLabels = @($all | Where-Object {
                $_.Current.ControlType -eq [System.Windows.Automation.ControlType]::Text -and
                $_.Current.Name -and
                $_.Current.Name -like "$ExpectedHost has requested access to passwords for the above item.*"
            })
            $hostVerified = ($hostLabels.Count -eq 1)

            # 2. Allow only a single item for the exact expected host.
            # The response identity is checked later in the extension worker.
            $btnCandidates = @($all | Where-Object {
                ($_.Current.AutomationId -eq 'AllowButton' -or $_.Current.Name -in @('Allow', '&Allow', 'AllowButton')) -and $_.Current.IsEnabled
            })
            $allowBtn = if ($btnCandidates.Count -eq 1) { $btnCandidates[0] } else { $null }
            if ($hostVerified -and $allowBtn) { break }
            Start-Sleep -Milliseconds 250
        }

        if (-not $hostVerified) {
            throw "Expected host label for '$ExpectedHost' (single item) not verified within deadline"
        }
        # The WinForms ListBox does not expose items via UIA on this runner.
        # The host label proves one item; the extension response verifies identity.
        # Never use cross-process LB_GETTEXT with a local buffer.
        if (-not $allowBtn) {
            $inventory = @($all | ForEach-Object { "$($_.Current.ControlType.ProgrammaticName):id=$($_.Current.AutomationId):name=$($_.Current.Name):class=$($_.Current.ClassName):enabled=$($_.Current.IsEnabled)" }) | Select-Object -First 20
            [Console]::Error.WriteLine("control-inventory-allow-failed list=$($inventory -join ' | ')")
            throw 'AllowButton not found within deadline'
        }

        $invokePattern = $null
        $allowHwnd = [IntPtr]$allowBtn.Current.NativeWindowHandle
        if ($allowBtn.TryGetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern, [ref]$invokePattern)) {
            $invokePattern.Invoke()
        } elseif ($allowHwnd -ne [IntPtr]::Zero) {
            [Win32Native]::SendMessage($allowHwnd, 0x00F5, [IntPtr]::Zero, [IntPtr]::Zero) | Out-Null
        } else {
            throw 'Allow button lacks InvokePattern and valid window handle'
        }
    }

    Write-Output "approval=$Phase"
} catch {
    [Console]::Error.WriteLine("approval-failed phase=$Phase step=$Phase reason=$($_.Exception.Message)")
    exit 1
}
