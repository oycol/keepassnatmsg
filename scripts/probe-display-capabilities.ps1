<#
.SYNOPSIS
    Read-only feasibility probe for Windows Runner display capabilities.
.DESCRIPTION
    Inspects runner process session interactivity, primary display geometry,
    effective DPI, supported target resolutions (2560x1440 and 3840x2160),
    and video controller details.
    Outputs a controlled JSON artifact without publishing usernames or paths.
    Strictly read-only: does not modify display settings, registry, or databases.
    Compatible with Windows PowerShell 5.1.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$OutputPath = 'e2e-artifacts\display-capabilities.json'
)

$ErrorActionPreference = 'Stop'

# Ensure native Win32 definitions are loaded once
if (-not ('DisplayProbeNative' -as [type])) {
    Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;

public static class DisplayProbeNative {
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

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT {
        public int x;
        public int y;
    }

    [DllImport("user32.dll", CharSet=CharSet.Unicode)]
    public static extern bool EnumDisplaySettings(string device, int modeNum, ref DEVMODE mode);

    [DllImport("user32.dll")]
    public static extern IntPtr MonitorFromPoint(POINT pt, uint dwFlags);

    [DllImport("kernel32.dll")]
    public static extern uint WTSGetActiveConsoleSessionId();

    [DllImport("wtsapi32.dll", SetLastError=true, CharSet=CharSet.Unicode)]
    public static extern bool WTSQuerySessionInformationW(
        IntPtr hServer,
        int sessionId,
        int wtsInfoClass,
        out IntPtr ppBuffer,
        out int pBytesReturned
    );

    [DllImport("wtsapi32.dll")]
    public static extern void WTSFreeMemory(IntPtr pMemory);

    [DllImport("shcore.dll")]
    public static extern int GetDpiForMonitor(IntPtr monitor, int dpiType, out uint x, out uint y);

    [DllImport("user32.dll")]
    public static extern uint GetDpiForSystem();

    public static DEVMODE NewDevMode() {
        DEVMODE m = new DEVMODE();
        m.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));
        return m;
    }

    public static string GetSessionConnectState(int sessionId) {
        try {
            IntPtr buffer;
            int bytes;
            // WTSConnectState = 8
            if (WTSQuerySessionInformationW(IntPtr.Zero, sessionId, 8, out buffer, out bytes)) {
                try {
                    if (bytes >= 4) {
                        int state = Marshal.ReadInt32(buffer);
                        switch (state) {
                            case 0: return "WTSActive";
                            case 1: return "WTSConnected";
                            case 2: return "WTSConnectQuery";
                            case 3: return "WTSShadow";
                            case 4: return "WTSDisconnected";
                            case 5: return "WTSIdle";
                            case 6: return "WTSListen";
                            case 7: return "WTSReset";
                            case 8: return "WTSDown";
                            case 9: return "WTSInit";
                            default: return "Unknown(" + state + ")";
                        }
                    }
                } finally {
                    WTSFreeMemory(buffer);
                }
            }
        } catch { }
        return "Unavailable";
    }

    public static uint[] GetPrimaryMonitorDpi() {
        try {
            POINT pt = new POINT();
            pt.x = 0;
            pt.y = 0;
            IntPtr mon = MonitorFromPoint(pt, 1 /* MONITOR_DEFAULTTOPRIMARY */);
            if (mon != IntPtr.Zero) {
                uint x, y;
                int hr = GetDpiForMonitor(mon, 0 /* MDT_EFFECTIVE_DPI */, out x, out y);
                if (hr == 0 && x > 0 && y > 0) {
                    return new uint[] { x, y };
                }
            }
        } catch { }
        return null;
    }

    public static uint GetSystemDpiSafe() {
        try {
            return GetDpiForSystem();
        } catch {
            return 0;
        }
    }
}
'@
}

# 1. Runner process session info & interactivity (strictly no username or user paths)
$currentProcess = [System.Diagnostics.Process]::GetCurrentProcess()
$runnerSessionId = $currentProcess.SessionId
$activeConsoleId = try { [DisplayProbeNative]::WTSGetActiveConsoleSessionId() } catch { $null }
$connectState = try { [DisplayProbeNative]::GetSessionConnectState($runnerSessionId) } catch { 'Unavailable' }

$formsInteractive = try {
    Add-Type -AssemblyName System.Windows.Forms
    [System.Windows.Forms.SystemInformation]::UserInteractive
} catch {
    $null
}

$terminalServer = try {
    [System.Windows.Forms.SystemInformation]::TerminalServerSession
} catch {
    $null
}

$cleanSessionName = if ($env:SESSIONNAME) {
    $env:SESSIONNAME -replace '[^a-zA-Z0-9_\-#]', ''
} else {
    'None'
}

$sessionInfo = [ordered]@{
    runnerProcessId               = $currentProcess.Id
    runnerSessionId               = $runnerSessionId
    activeConsoleSessionId        = $activeConsoleId
    isCurrentSessionActiveConsole = ($runnerSessionId -eq $activeConsoleId)
    environmentUserInteractive    = [Environment]::UserInteractive
    formsUserInteractive          = $formsInteractive
    isTerminalServerSession       = $terminalServer
    sessionName                   = $cleanSessionName
    wtsConnectState               = $connectState
}

# 2. Current primary display device, resolution, and effective DPI
$primaryScreen = try {
    [System.Windows.Forms.Screen]::PrimaryScreen
} catch {
    $null
}

$currentMode = [DisplayProbeNative]::NewDevMode()
$enumCurrentSuccess = try {
    [DisplayProbeNative]::EnumDisplaySettings($null, -1, [ref]$currentMode)
} catch {
    $false
}

$primaryDisplay = [ordered]@{
    deviceName                 = if ($primaryScreen) { $primaryScreen.DeviceName } elseif ($enumCurrentSuccess) { $currentMode.dmDeviceName } else { $null }
    screenBounds               = if ($primaryScreen) { [ordered]@{ width = $primaryScreen.Bounds.Width; height = $primaryScreen.Bounds.Height } } else { $null }
    workingArea                = if ($primaryScreen) { [ordered]@{ width = $primaryScreen.WorkingArea.Width; height = $primaryScreen.WorkingArea.Height } } else { $null }
    bitsPerPixel               = if ($primaryScreen) { $primaryScreen.BitsPerPixel } elseif ($enumCurrentSuccess) { $currentMode.dmBitsPerPel } else { $null }
    currentMode                = if ($enumCurrentSuccess) {
        [ordered]@{
            width            = $currentMode.dmPelsWidth
            height           = $currentMode.dmPelsHeight
            bitsPerPel       = $currentMode.dmBitsPerPel
            displayFrequency = $currentMode.dmDisplayFrequency
            orientation      = $currentMode.dmDisplayOrientation
            displayFlags     = $currentMode.dmDisplayFlags
        }
    } else {
        $null
    }
    enumCurrentSettingsSuccess = $enumCurrentSuccess
}

$monitorDpi = try { [DisplayProbeNative]::GetPrimaryMonitorDpi() } catch { $null }
$systemDpi = try { [DisplayProbeNative]::GetSystemDpiSafe() } catch { 0 }
$gdiDpi = try {
    Add-Type -AssemblyName System.Drawing
    $g = [System.Drawing.Graphics]::FromHwnd([IntPtr]::Zero)
    try {
        [ordered]@{ dpiX = [uint32]$g.DpiX; dpiY = [uint32]$g.DpiY }
    } finally {
        $g.Dispose()
    }
} catch {
    $null
}

$effectiveDpiX = if ($monitorDpi) { $monitorDpi[0] } elseif ($systemDpi -gt 0) { $systemDpi } elseif ($gdiDpi) { $gdiDpi.dpiX } else { 96 }
$effectiveDpiY = if ($monitorDpi) { $monitorDpi[1] } elseif ($systemDpi -gt 0) { $systemDpi } elseif ($gdiDpi) { $gdiDpi.dpiY } else { 96 }

$dpiInfo = [ordered]@{
    effectiveDpiX    = $effectiveDpiX
    effectiveDpiY    = $effectiveDpiY
    scalePercent     = [int][Math]::Round(($effectiveDpiX / 96.0) * 100)
    monitorDpiSource = if ($monitorDpi) { 'GetDpiForMonitor' } elseif ($systemDpi -gt 0) { 'GetDpiForSystem' } elseif ($gdiDpi) { 'GraphicsFromHwnd' } else { 'DefaultFallback96' }
    systemDpi        = $systemDpi
    gdiDpi           = $gdiDpi
}

# 3. EnumDisplaySettings supported 2560x1440 and 3840x2160 modes & resolutions
$targetModes = @()
$uniqueResTable = @{}
$totalModesEnumerated = 0

for ($i = 0; $i -lt 4096; $i++) {
    $candidate = [DisplayProbeNative]::NewDevMode()
    $ok = try { [DisplayProbeNative]::EnumDisplaySettings($null, $i, [ref]$candidate) } catch { $false }
    if (-not $ok) { break }
    $totalModesEnumerated++

    $w = $candidate.dmPelsWidth
    $h = $candidate.dmPelsHeight
    $uniqueResTable["${w}x${h}"] = $true

    if (($w -eq 2560 -and $h -eq 1440) -or ($w -eq 3840 -and $h -eq 2160)) {
        $targetModes += [ordered]@{
            width            = $w
            height           = $h
            bitsPerPel       = $candidate.dmBitsPerPel
            displayFrequency = $candidate.dmDisplayFrequency
            orientation      = $candidate.dmDisplayOrientation
            displayFlags     = $candidate.dmDisplayFlags
        }
    }
}

$modes2560x1440 = @($targetModes | Where-Object { $_.width -eq 2560 -and $_.height -eq 1440 })
$modes3840x2160 = @($targetModes | Where-Object { $_.width -eq 3840 -and $_.height -eq 2160 })
$sortedResolutions = @($uniqueResTable.Keys | Sort-Object {
    $parts = $_ -split 'x'
    [int]$parts[0] * 100000 + [int]$parts[1]
})

# 4. Win32_VideoController hardware details (no sensitive identifiers)
$videoControllers = @()
$cimControllers = try {
    Get-CimInstance -ClassName Win32_VideoController -ErrorAction Stop
} catch {
    try {
        Get-WmiObject -Class Win32_VideoController -ErrorAction Stop
    } catch {
        $null
    }
}

if ($cimControllers) {
    foreach ($vc in $cimControllers) {
        $ramMB = if ($vc.AdapterRAM) { [Math]::Round([double]$vc.AdapterRAM / 1MB, 2) } else { $null }
        $videoControllers += [ordered]@{
            name                        = [string]$vc.Name
            videoProcessor              = [string]$vc.VideoProcessor
            driverVersion               = [string]$vc.DriverVersion
            adapterRAMBytes             = [int64]$vc.AdapterRAM
            adapterRAMMB                = $ramMB
            videoModeDescription        = [string]$vc.VideoModeDescription
            currentHorizontalResolution = [int]$vc.CurrentHorizontalResolution
            currentVerticalResolution   = [int]$vc.CurrentVerticalResolution
            currentBitsPerPixel         = [int]$vc.CurrentBitsPerPixel
            currentRefreshRate          = [int]$vc.CurrentRefreshRate
            status                      = [string]$vc.Status
        }
    }
}

# 5. Assemble final structured probe result
$probeResult = [ordered]@{
    timestampUtc         = [DateTime]::UtcNow.ToString('o')
    session              = $sessionInfo
    primaryDisplay       = $primaryDisplay
    dpi                  = $dpiInfo
    supported2560x1440   = ($modes2560x1440.Count -gt 0)
    supported3840x2160   = ($modes3840x2160.Count -gt 0)
    modes2560x1440Count  = $modes2560x1440.Count
    modes3840x2160Count  = $modes3840x2160.Count
    modes2560x1440       = $modes2560x1440
    modes3840x2160       = $modes3840x2160
    uniqueResolutions    = $sortedResolutions
    totalModesEnumerated = $totalModesEnumerated
    videoControllers     = $videoControllers
}

# 6. Save JSON artifact safely
$resolvedOutputPath = [System.IO.Path]::GetFullPath($OutputPath)
$parentDir = [System.IO.Path]::GetDirectoryName($resolvedOutputPath)
if ($parentDir -and -not (Test-Path -LiteralPath $parentDir)) {
    [System.IO.Directory]::CreateDirectory($parentDir) | Out-Null
}

$json = $probeResult | ConvertTo-Json -Depth 6
[System.IO.File]::WriteAllText($resolvedOutputPath, $json, [System.Text.Encoding]::UTF8)

# 7. Print sanitized diagnostic summary to stdout (no paths, usernames, or secrets)
$outputPathLeaf = [System.IO.Path]::GetFileName($resolvedOutputPath)

Write-Host '====================================================='
Write-Host 'Windows Runner Display Capability Probe (Read-Only)'
Write-Host '====================================================='
Write-Host "Runner Process ID:           $($sessionInfo.runnerProcessId)"
Write-Host "Runner Session ID:           $($sessionInfo.runnerSessionId)"
Write-Host "Active Console Session ID:   $($sessionInfo.activeConsoleSessionId)"
Write-Host "Is Active Console Session:   $($sessionInfo.isCurrentSessionActiveConsole)"
Write-Host "UserInteractive (Env/Forms): $($sessionInfo.environmentUserInteractive) / $($sessionInfo.formsUserInteractive)"
Write-Host "Terminal Server Session:     $($sessionInfo.isTerminalServerSession)"
Write-Host "Session Name:                $($sessionInfo.sessionName)"
Write-Host "WTS Session State:           $($sessionInfo.wtsConnectState)"
Write-Host '-----------------------------------------------------'
Write-Host "Primary Device:              $($primaryDisplay.deviceName)"
if ($primaryDisplay.screenBounds) {
    Write-Host "Primary Screen Bounds:       $($primaryDisplay.screenBounds.width)x$($primaryDisplay.screenBounds.height)"
}
Write-Host "Effective DPI:               $($dpiInfo.effectiveDpiX)x$($dpiInfo.effectiveDpiY) ($($dpiInfo.scalePercent)% scale, source: $($dpiInfo.monitorDpiSource))"
Write-Host "System DPI:                  $($dpiInfo.systemDpi)"
Write-Host '-----------------------------------------------------'
Write-Host "Supports 2560x1440:          $($probeResult.supported2560x1440) ($($probeResult.modes2560x1440Count) modes found)"
Write-Host "Supports 3840x2160:          $($probeResult.supported3840x2160) ($($probeResult.modes3840x2160Count) modes found)"
Write-Host "Total Modes Enumerated:      $($probeResult.totalModesEnumerated)"
Write-Host "Available Resolutions:       $($sortedResolutions -join ', ')"
Write-Host '-----------------------------------------------------'
Write-Host "Video Controllers Found:     $($videoControllers.Count)"
foreach ($vc in $videoControllers) {
    Write-Host "  Name:       $($vc.name)"
    Write-Host "  Processor:  $($vc.videoProcessor)"
    Write-Host "  Driver:     $($vc.driverVersion)"
    Write-Host "  VRAM (MB):  $($vc.adapterRAMMB)"
    Write-Host "  Resolution: $($vc.currentHorizontalResolution)x$($vc.currentVerticalResolution) @ $($vc.currentRefreshRate)Hz ($($vc.currentBitsPerPixel) bpp)"
}
Write-Host '====================================================='
Write-Host "JSON artifact written to:    $outputPathLeaf"
Write-Host 'No credentials, usernames, user paths, or sensitive keys were accessed or exported.'
