# Run in the interactive Windows session containing the dedicated KeePass test process.
# Example (after configuring Windows display scale >=150% and starting the dedicated
# test DB KeePass instance):
# powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\verify-options-hosted-display.ps1 -ProcessId $kpProc.Id -TestDatabasePath C:\KeePassNatMsg-E2E\test.kdbx -TargetWidth 2560 -TargetHeight 1440 -MinimumDpi 144 -OutputPath .\e2e-artifacts\options-hosted-2k.json
param(
    [Parameter(Mandatory = $true)][ValidateRange(1, 2147483647)][int]$ProcessId,
    [Parameter(Mandatory = $true)][ValidateSet(2560, 3840)][int]$TargetWidth,
    [Parameter(Mandatory = $true)][ValidateSet(1440, 2160)][int]$TargetHeight,
    [ValidateRange(120, 768)][int]$MinimumDpi = 144,
    [Parameter(Mandatory = $true)][string]$TestDatabasePath,
    [string]$OutputPath = 'options-hosted-display.json'
)

$ErrorActionPreference = 'Stop'
# Do not log element text outside the fixed UI labels below: KeePass contains secrets.
$result = [ordered]@{ passed = $false; processId = $ProcessId; target = "${TargetWidth}x${TargetHeight}"; minimumDpi = $MinimumDpi; original = $null; actual = $null; tabs = @(); buttons = @(); restored = $false; error = $null; restoreError = $null }
$originalMode = $null
$modeChanged = $false
$dialog = $null
try {
    if (($TargetWidth -eq 2560 -and $TargetHeight -ne 1440) -or ($TargetWidth -eq 3840 -and $TargetHeight -ne 2160)) { throw 'Unsupported target resolution pair' }
    if (-not [Environment]::UserInteractive) { throw 'An interactive Windows desktop is required' }
    Add-Type -AssemblyName UIAutomationClient
    Add-Type -AssemblyName UIAutomationTypes
    Add-Type -AssemblyName System.Windows.Forms
    # The DEVMODE layout is required by EnumDisplaySettings/ChangeDisplaySettingsEx.
    Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
public static class HostedDisplayNative {
    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    public struct DEVMODE {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst=32)] public string dmDeviceName;
        public short dmSpecVersion, dmDriverVersion, dmSize, dmDriverExtra;
        public int dmFields, dmPositionX, dmPositionY, dmDisplayOrientation, dmDisplayFixedOutput;
        public short dmColor, dmDuplex, dmYResolution, dmTTOption, dmCollate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst=32)] public string dmFormName;
        public short dmLogPixels;
        public int dmBitsPerPel, dmPelsWidth, dmPelsHeight, dmDisplayFlags, dmDisplayFrequency, dmICMMethod, dmICMIntent, dmMediaType, dmDitherType, dmReserved1, dmReserved2, dmPanningWidth, dmPanningHeight;
    }
    [DllImport("user32.dll", CharSet=CharSet.Unicode)] public static extern bool EnumDisplaySettings(string device, int modeNum, ref DEVMODE mode);
    [DllImport("user32.dll", CharSet=CharSet.Unicode)] public static extern int ChangeDisplaySettingsEx(string device, ref DEVMODE mode, IntPtr hwnd, int flags, IntPtr lparam);
    [DllImport("user32.dll")] public static extern IntPtr MonitorFromWindow(IntPtr hwnd, int flags);
    [DllImport("shcore.dll")] public static extern int GetDpiForMonitor(IntPtr monitor, int dpiType, out uint x, out uint y);
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hwnd);
    public static DEVMODE NewMode() { DEVMODE m = new DEVMODE(); m.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE)); return m; }
    public static uint[] Dpi(IntPtr hwnd) {
        IntPtr monitor = MonitorFromWindow(hwnd, 2);
        if (monitor == IntPtr.Zero) throw new InvalidOperationException("Monitor unavailable");
        uint x, y;
        int code = GetDpiForMonitor(monitor, 0, out x, out y); // MDT_EFFECTIVE_DPI
        if (code != 0 || x == 0 || y == 0) throw new InvalidOperationException("Effective DPI unavailable");
        return new uint[] { x, y };
    }
}
'@
    $process = Get-Process -Id $ProcessId -ErrorAction Stop
    if ($process.ProcessName -ne 'KeePass' -or $process.HasExited -or $process.MainWindowHandle -eq [IntPtr]::Zero) { throw 'PID must belong to a running, interactive KeePass main window' }
    $commandLine = (Get-CimInstance Win32_Process -Filter "ProcessId = $ProcessId" -ErrorAction Stop).CommandLine
    if (-not $commandLine -or -not [System.IO.File]::Exists($TestDatabasePath) -or
        -not $commandLine.Contains([System.IO.Path]::GetFullPath($TestDatabasePath))) { throw 'KeePass process command line does not contain the dedicated test database path' }
    $mainHandle = $process.MainWindowHandle
    $root = [System.Windows.Automation.AutomationElement]::FromHandle($mainHandle)
    if (-not $root -or $root.Current.ProcessId -ne $ProcessId) { throw 'KeePass window PID mismatch' }
    $primary = [System.Windows.Forms.Screen]::PrimaryScreen
    if (-not $primary -or [System.Windows.Forms.Screen]::FromHandle($mainHandle).DeviceName -ne $primary.DeviceName) { throw 'KeePass test window must be on the primary monitor' }
    # Never close a pre-existing Options dialog: it may belong to a real user.
    $desktop = [System.Windows.Automation.AutomationElement]::RootElement
    foreach ($window in $desktop.FindAll([System.Windows.Automation.TreeScope]::Children, [System.Windows.Automation.Condition]::TrueCondition)) {
        if ($window.Current.ProcessId -eq $ProcessId -and $window.Current.Name -eq 'KeePassNatMsg Options') { throw 'Options dialog already open; refusing to touch it' }
    }

    $originalMode = [HostedDisplayNative]::NewMode()
    if (-not [HostedDisplayNative]::EnumDisplaySettings($null, -1, [ref]$originalMode)) { throw 'Cannot read original primary display mode' }
    $originalDpi = [HostedDisplayNative]::Dpi($mainHandle)
    $result.original = [ordered]@{ width = $originalMode.dmPelsWidth; height = $originalMode.dmPelsHeight; dpiX = $originalDpi[0]; dpiY = $originalDpi[1] }
    # Windows has no supported immediate per-monitor user-scale setter. Never edit
    # HKCU DPI registry values: they may require logoff and could affect the user.
    # Instead require the requested effective scale to be configured already.
    if ($originalDpi[0] -lt $MinimumDpi -or $originalDpi[1] -lt $MinimumDpi) { throw 'High-DPI prerequisite unmet: configure Windows display scale in the interactive session before running' }

    $targetMode = $null
    for ($i = 0; $i -lt 4096; $i++) {
        $candidate = [HostedDisplayNative]::NewMode()
        if (-not [HostedDisplayNative]::EnumDisplaySettings($null, $i, [ref]$candidate)) { break }
        if ($candidate.dmPelsWidth -eq $TargetWidth -and $candidate.dmPelsHeight -eq $TargetHeight -and
            $candidate.dmBitsPerPel -eq $originalMode.dmBitsPerPel -and $candidate.dmDisplayFrequency -eq $originalMode.dmDisplayFrequency -and
            $candidate.dmDisplayOrientation -eq $originalMode.dmDisplayOrientation) { $targetMode = $candidate; break }
    }
    if (-not $targetMode) { throw 'Target mode unsupported at current color depth, frequency and orientation' }
    if ([HostedDisplayNative]::ChangeDisplaySettingsEx($null, [ref]$targetMode, [IntPtr]::Zero, 2, [IntPtr]::Zero) -ne 0) { throw 'Target display mode rejected by CDS_TEST' }
    if ($originalMode.dmPelsWidth -ne $TargetWidth -or $originalMode.dmPelsHeight -ne $TargetHeight) {
        # Mark before calling: a driver can apply a mode despite returning an error.
        $modeChanged = $true
        if ([HostedDisplayNative]::ChangeDisplaySettingsEx($null, [ref]$targetMode, [IntPtr]::Zero, 0, [IntPtr]::Zero) -ne 0) { throw 'Display mode change failed' }
    }
    Start-Sleep -Milliseconds 800
    $actualMode = [HostedDisplayNative]::NewMode()
    if (-not [HostedDisplayNative]::EnumDisplaySettings($null, -1, [ref]$actualMode) -or $actualMode.dmPelsWidth -ne $TargetWidth -or $actualMode.dmPelsHeight -ne $TargetHeight) { throw 'Target display mode did not take effect' }
    $actualDpi = [HostedDisplayNative]::Dpi($mainHandle)
    $result.actual = [ordered]@{ width = $actualMode.dmPelsWidth; height = $actualMode.dmPelsHeight; dpiX = $actualDpi[0]; dpiY = $actualDpi[1] }
    if ($actualDpi[0] -lt $MinimumDpi -or $actualDpi[1] -lt $MinimumDpi) { throw 'Effective DPI below required high-DPI threshold after mode change' }

    $tree = [System.Windows.Automation.TreeScope]::Descendants
    $trueCondition = [System.Windows.Automation.Condition]::TrueCondition
    $menuCondition = [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::ControlTypeProperty, [System.Windows.Automation.ControlType]::MenuItem)
    $tools = $null
    foreach ($item in $root.FindAll($tree, $menuCondition)) {
        if ($item.Current.Name -eq 'Tools' -and $item.Current.ProcessId -eq $ProcessId) { $tools = $item; break }
    }
    if (-not $tools) { throw 'KeePass Tools menu not found' }
    [void][HostedDisplayNative]::SetForegroundWindow($mainHandle)
    $expand = $null
    if ($tools.TryGetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern, [ref]$expand)) {
        $expand.Expand()
    } else {
        $invoke = $null
        if (-not $tools.TryGetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern, [ref]$invoke)) { throw 'Tools menu has no UIAutomation activation pattern' }
        $invoke.Invoke()
    }
    $menuItem = $null
    $desktop = [System.Windows.Automation.AutomationElement]::RootElement
    for ($attempt = 0; $attempt -lt 30 -and -not $menuItem; $attempt++) {
        foreach ($item in $desktop.FindAll($tree, $menuCondition)) {
            if ($item.Current.ProcessId -eq $ProcessId -and $item.Current.Name -eq 'KeePassNatMsg Options...') { $menuItem = $item; break }
        }
        if (-not $menuItem) { Start-Sleep -Milliseconds 200 }
    }
    if (-not $menuItem) { throw 'KeePassNatMsg Options menu item not found in the specified process' }
    $invoke = $null
    if (-not $menuItem.TryGetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern, [ref]$invoke)) { throw 'Options menu item cannot be invoked via UIAutomation' }
    $invoke.Invoke()
    for ($attempt = 0; $attempt -lt 50 -and -not $dialog; $attempt++) {
        foreach ($window in $desktop.FindAll([System.Windows.Automation.TreeScope]::Children, $trueCondition)) {
            if ($window.Current.ProcessId -eq $ProcessId -and $window.Current.Name -eq 'KeePassNatMsg Options' -and
                $window.Current.ControlType -eq [System.Windows.Automation.ControlType]::Window) { $dialog = $window; break }
        }
        if (-not $dialog) { Start-Sleep -Milliseconds 200 }
    }
    if (-not $dialog -or $dialog.Current.IsOffscreen) { throw 'KeePass-hosted Options window not visible' }
    $bounds = $dialog.Current.BoundingRectangle
    $work = [System.Windows.Forms.Screen]::FromHandle([IntPtr]$dialog.Current.NativeWindowHandle).WorkingArea
    if ($bounds.Width -le 0 -or $bounds.Height -le 0 -or $bounds.Left -lt $work.Left -or $bounds.Top -lt $work.Top -or
        $bounds.Right -gt $work.Right -or $bounds.Bottom -gt $work.Bottom) { throw 'Options window outside monitor working area' }
    $tabs = @('Browser Integration', 'Matching Rules', 'Database & Security', 'Associations')
    $tabCondition = [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::ControlTypeProperty, [System.Windows.Automation.ControlType]::TabItem)
    $tabItems = $dialog.FindAll($tree, $tabCondition)
    if ($tabItems.Count -ne $tabs.Count) { throw 'Options tab count mismatch' }
    foreach ($name in $tabs) {
        $match = @($tabItems | Where-Object { $_.Current.Name -eq $name -and $_.Current.ProcessId -eq $ProcessId })
        if ($match.Count -ne 1 -or $match[0].Current.IsOffscreen) { throw 'Required Options tab unavailable' }
        $result.tabs += $name
    }
    $buttonCondition = [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::ControlTypeProperty, [System.Windows.Automation.ControlType]::Button)
    foreach ($name in @('Save', 'Cancel')) {
        $matches = @($dialog.FindAll($tree, $buttonCondition) | Where-Object { $_.Current.Name -eq $name -and $_.Current.ProcessId -eq $ProcessId -and -not $_.Current.IsOffscreen })
        if ($matches.Count -ne 1) { throw 'Required Options dialog button unavailable' }
        $b = $matches[0].Current.BoundingRectangle
        if ($b.Width -le 0 -or $b.Height -le 0 -or $b.Left -lt $bounds.Left -or $b.Top -lt $bounds.Top -or $b.Right -gt $bounds.Right -or $b.Bottom -gt $bounds.Bottom -or
            $b.Left -lt $work.Left -or $b.Top -lt $work.Top -or $b.Right -gt $work.Right -or $b.Bottom -gt $work.Bottom) { throw 'Options dialog button clipped' }
        $result.buttons += $name
        if ($name -eq 'Cancel') { $cancel = $matches[0] }
    }
    # Never save Options: only close this dialog using its Cancel button.
    $cancelInvoke = $null
    if (-not $cancel.TryGetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern, [ref]$cancelInvoke)) { throw 'Cancel button cannot be invoked' }
    $cancelInvoke.Invoke()
    $dialog = $null
    $result.passed = $true
} catch {
    # No exception text: COM/UIA messages may contain a window title or user data.
    $result.error = if ($_.Exception.Message -in @('Unsupported target resolution pair', 'An interactive Windows desktop is required', 'Cannot read original primary display mode', 'High-DPI prerequisite unmet: configure Windows display scale in the interactive session before running', 'Target mode unsupported at current color depth, frequency and orientation', 'Target display mode rejected by CDS_TEST', 'Display mode change failed', 'Target display mode did not take effect', 'Effective DPI below required high-DPI threshold after mode change', 'KeePass Tools menu not found', 'KeePassNatMsg Options menu item not found in the specified process', 'KeePass-hosted Options window not visible', 'Options window outside monitor working area', 'Options tab count mismatch', 'Required Options tab unavailable', 'Required Options dialog button unavailable', 'Options dialog button clipped')) { $_.Exception.Message } else { 'GUI verification failed (details suppressed)' }
} finally {
    if ($dialog) {
        try {
            # Escape dismisses a partially opened dialog without saving settings.
            $dialog.SetFocus()
            [System.Windows.Forms.SendKeys]::SendWait('{ESC}')
        } catch { }
    }
    try {
        if ($modeChanged) {
            if ([HostedDisplayNative]::ChangeDisplaySettingsEx($null, [ref]$originalMode, [IntPtr]::Zero, 0, [IntPtr]::Zero) -ne 0) { throw 'Original display mode restore rejected' }
        }
        if ($originalMode) {
            $restoredMode = [HostedDisplayNative]::NewMode()
            if (-not [HostedDisplayNative]::EnumDisplaySettings($null, -1, [ref]$restoredMode) -or
                $restoredMode.dmPelsWidth -ne $originalMode.dmPelsWidth -or $restoredMode.dmPelsHeight -ne $originalMode.dmPelsHeight -or
                $restoredMode.dmDisplayFrequency -ne $originalMode.dmDisplayFrequency -or $restoredMode.dmBitsPerPel -ne $originalMode.dmBitsPerPel -or
                $restoredMode.dmDisplayOrientation -ne $originalMode.dmDisplayOrientation) { throw 'Original display mode not restored' }
            $restoredDpi = [HostedDisplayNative]::Dpi($mainHandle)
            if ($restoredDpi[0] -ne $originalDpi[0] -or $restoredDpi[1] -ne $originalDpi[1]) { throw 'Original effective display scale changed' }
            $result.restored = $true
        }
    } catch {
        $result.restoreError = 'Original display mode/scale restoration could not be verified; inspect Windows display settings immediately'
        $result.passed = $false
    }
    try {
        $outputFullPath = [System.IO.Path]::GetFullPath($OutputPath)
        $directory = [System.IO.Path]::GetDirectoryName($outputFullPath)
        if (-not [System.IO.Directory]::Exists($directory)) { [void][System.IO.Directory]::CreateDirectory($directory) }
        $json = $result | ConvertTo-Json -Depth 5
        [System.IO.File]::WriteAllText($outputFullPath, $json, (New-Object System.Text.UTF8Encoding($false)))
        Write-Output $json
    } catch {
        Write-Error 'Could not write sanitized JSON evidence' -ErrorAction Continue
        $result.passed = $false
    }
}
if ($result.restoreError) { exit 1 }
exit 0
