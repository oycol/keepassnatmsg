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
    if (-not $window) { throw 'Expected approval window not found' }
    $all = @($window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $condition))
    function One($type, $name) {
        $found = @($all | Where-Object { $_.Current.ControlType -eq $type -and ($_.Current.AutomationId -eq $name -or $_.Current.Name -eq $name) })
        if ($found.Count -ne 1 -or -not $found[0].Current.IsEnabled) { throw 'Expected unique enabled control absent' }
        return $found[0]
    }
    if ($Phase -eq 'association') {
        $fingerprints = @($all | Where-Object { $_.Current.ControlType -eq [System.Windows.Automation.ControlType]::Text -and $_.Current.Name -match '^([0-9A-F]{2}:){7}[0-9A-F]{2}$' })
        if ($fingerprints.Count -ne 1 -or $AssociationName -notmatch '^E2E-[A-Za-z0-9-]{8,64}$') { throw 'Association identity validation failed' }
        $field = One ([System.Windows.Automation.ControlType]::Edit) 'KeyName'
        $field.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).SetValue($AssociationName)
        $button = One ([System.Windows.Automation.ControlType]::Button) 'Save'
    } else {
        $labels = @($all | Where-Object { $_.Current.ControlType -eq [System.Windows.Automation.ControlType]::Text -and $_.Current.Name -match 'has requested access to passwords' -and $_.Current.Name.Contains($ExpectedHost) })
        $entries = @($all | Where-Object { $_.Current.ControlType -eq [System.Windows.Automation.ControlType]::ListItem -and $_.Current.Name -eq $ExpectedTitle })
        if ($labels.Count -ne 1 -or $entries.Count -ne 1) { throw 'Access prompt target did not match fixture' }
        $remember = One ([System.Windows.Automation.ControlType]::CheckBox) 'RememberCheck'
        if ($remember.GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern).Current.ToggleState -ne [System.Windows.Automation.ToggleState]::Off) { throw 'Remember decision unexpectedly enabled' }
        $button = One ([System.Windows.Automation.ControlType]::Button) 'AllowButton'
    }
    $button.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
    Write-Output "approval=$Phase"
} catch {
    [Console]::Error.WriteLine('Approval failed closed (window, process, or fixture mismatch)')
    exit 1
}
