# Windows UI Automation approval for the disposable KeePass E2E database only.
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

    $controlsDeadline = [DateTime]::UtcNow.AddSeconds(20)
    $treeScope = [System.Windows.Automation.TreeScope]::Descendants

    if ($Phase -eq 'association') {
        $field = $null
        $saveBtn = $null
        while ([DateTime]::UtcNow -lt $controlsDeadline) {
            $all = @($window.FindAll($treeScope, $condition))
            $fingerprints = @($all | Where-Object { $_.Current.ControlType -eq [System.Windows.Automation.ControlType]::Text -and $_.Current.Name -match '^([0-9A-F]{2}:){7}[0-9A-F]{2}$' })
            $candidates = @($all | Where-Object {
                $_.Current.AutomationId -eq 'KeyName' -or
                $_.Current.Name -eq 'KeyName' -or
                ($_.Current.ClassName -and $_.Current.ClassName -like '*Edit*')
            })
            if ($candidates.Count -ge 1 -and $candidates[0].Current.IsEnabled) {
                $valPattern = $null
                if ($candidates[0].TryGetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern, [ref]$valPattern)) {
                    $field = $candidates[0]
                }
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

        $vPattern = $null
        if ($field.TryGetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern, [ref]$vPattern)) {
            $vPattern.SetValue($AssociationName)
        } else {
            throw 'KeyName editor lacks ValuePattern'
        }

        Start-Sleep -Milliseconds 100
        $invokePattern = $null
        if ($saveBtn.TryGetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern, [ref]$invokePattern)) {
            $invokePattern.Invoke()
        } else {
            throw 'Save button lacks InvokePattern'
        }
    } else {
        # access phase
        $allowBtn = $null
        while ([DateTime]::UtcNow -lt $controlsDeadline) {
            $all = @($window.FindAll($treeScope, $condition))
            $labels = @($all | Where-Object { $_.Current.Name -and $_.Current.Name -match 'has requested access to passwords' -and $_.Current.Name.Contains($ExpectedHost) })
            $btnCandidates = @($all | Where-Object {
                ($_.Current.AutomationId -eq 'AllowButton' -or $_.Current.Name -in @('Allow', '&Allow', 'AllowButton')) -and $_.Current.IsEnabled
            })
            if ($btnCandidates.Count -ge 1) {
                $allowBtn = $btnCandidates[0]
            }
            if ($allowBtn) {
                break
            }
            Start-Sleep -Milliseconds 250
        }

        if (-not $allowBtn) {
            $inventory = @($all | ForEach-Object { "$($_.Current.ControlType.ProgrammaticName):id=$($_.Current.AutomationId):name=$($_.Current.Name):class=$($_.Current.ClassName):enabled=$($_.Current.IsEnabled)" }) | Select-Object -First 20
            [Console]::Error.WriteLine("control-inventory-allow-failed list=$($inventory -join ' | ')")
            throw 'AllowButton not found within deadline'
        }

        $invokePattern = $null
        if ($allowBtn.TryGetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern, [ref]$invokePattern)) {
            $invokePattern.Invoke()
        } else {
            throw 'Allow button lacks InvokePattern'
        }
    }

    Write-Output "approval=$Phase"
} catch {
    [Console]::Error.WriteLine("approval-failed phase=$Phase step=$Phase reason=$($_.Exception.Message)")
    exit 1
}
