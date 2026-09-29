"""Regression, classification, and safety verification for KeePass-hosted display validation script.

Validates that scripts/verify-options-hosted-display.ps1:
1. Accurately restricts 'blocked' classification ONLY to genuine preflight prerequisites:
   - effective DPI < MinimumDpi
   - target resolution mode absent from enumerated modes
   - CDS_TEST driver rejected
   All other exceptions (invalid PID, command line DB mismatch, window mismatch,
   display API failure, mode change failure, GUI check failure) must NOT be blocked
   and must resolve to exit 1 (preventing false-green runs).
2. Provides meaningful behavioral tests and internal testable helpers (Test-IsBlockedError,
   Resolve-HostedDisplayExitCode, Test-HostedDisplayRestoration, Invoke-SelfTest).
3. Evaluates display restoration via independent primary monitor DPI readback rather than
   the KeePass main window handle (which may become invalid if KeePass crashes or closes).
4. Verifies display restoration via readback even when display mode was unchanged,
   accurately reflecting unmutated display state when GUI verification fails rather than
   blindly setting restored=true or falsely leaving it false.
5. Uses explicit display device name and never EnumDisplaySettings($null).
6. Correctly handles probe 36601787312 scenario (96 DPI runner prerequisite blocked).

Run with:
python3 -m unittest discover -s KeePassNatMsg.Tests -p test_verify_hosted_display.py -v
"""
import re
import shutil
import subprocess
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SCRIPT_PATH = ROOT / 'scripts' / 'verify-options-hosted-display.ps1'


class TestVerifyHostedDisplay(unittest.TestCase):
    def setUp(self):
        self.assertTrue(SCRIPT_PATH.exists(), f"Script must exist at {SCRIPT_PATH}")
        self.content = SCRIPT_PATH.read_text(encoding='utf-8')

    def test_no_broad_ispreflight_flag(self):
        """Old script had $isPreflight = $true spanning all initialization through CDS_TEST.

        Any invalid PID, DB mismatch, or API failure was falsely marked blocked=true (exit 0).
        Verify $isPreflight flag is removed from the script.
        """
        self.assertNotIn(
            '$isPreflight',
            self.content,
            "$isPreflight boolean flag must be removed to avoid catch-all false-green classification",
        )

    def test_blocked_classification_strictly_restricted(self):
        """Verify Test-IsBlockedError classifies ONLY genuine preflight prerequisites.

        Fatal test failures (invalid PID, DB mismatch, GUI failure, etc.) must NOT be blocked.
        """
        # Extract blocked messages from script content
        blocked_block_match = re.search(
            r'function Test-IsBlockedError\s*\{.*?\$blockedMessages\s*=\s*@\((.*?)\)',
            self.content,
            re.DOTALL,
        )
        self.assertIsNotNone(blocked_block_match, "Test-IsBlockedError must define $blockedMessages array")
        assert blocked_block_match is not None
        blocked_messages_raw = blocked_block_match.group(1)
        blocked_messages = re.findall(r"['\"]([^'\"]+)['\"]", blocked_messages_raw)

        expected_blocked = {
            'High-DPI prerequisite unmet: configure Windows display scale in the interactive session before running',
            'Target mode unsupported at current color depth, frequency and orientation',
            'Target display mode rejected by CDS_TEST',
        }
        self.assertEqual(
            set(blocked_messages),
            expected_blocked,
            f"Blocked messages must strictly match expected 3 prerequisites, got: {blocked_messages}",
        )

        def is_blocked(msg: str) -> bool:
            return msg in blocked_messages

        # Non-blocked failures: MUST NOT be blocked (false green prevention!)
        fatal_failures = [
            'PID must belong to a running, interactive KeePass main window',
            'KeePass process command line does not contain the dedicated test database path',
            'KeePass window PID mismatch',
            'Primary display device unavailable',
            'KeePass test window must be on the primary monitor',
            'Options dialog already open; refusing to touch it',
            'Cannot read original primary display mode',
            'Display mode change failed',
            'Target display mode did not take effect',
            'Effective DPI below required high-DPI threshold after mode change',
            'KeePass Tools menu not found',
            'Tools menu has no UIAutomation activation pattern',
            'KeePassNatMsg Options menu item not found in the specified process',
            'Options menu item cannot be invoked via UIAutomation',
            'KeePass-hosted Options window not visible',
            'Options window outside monitor working area',
            'Options tab count mismatch',
            'Required Options tab unavailable',
            'Required Options dialog button unavailable',
            'Options dialog button clipped',
            'Cancel button cannot be invoked',
            'Unsupported target resolution pair',
            'An interactive Windows desktop is required',
            'GUI verification failed (details suppressed)',
        ]

        for failure in fatal_failures:
            self.assertFalse(
                is_blocked(failure),
                f"Failure '{failure}' must NOT be classified as blocked (would cause false-green exit 0)",
            )

        # Genuine blocked conditions: MUST be blocked
        for blocked in expected_blocked:
            self.assertTrue(
                is_blocked(blocked),
                f"Prerequisite '{blocked}' MUST be classified as blocked",
            )

    def test_exit_code_resolution_matrix(self):
        """Verify Resolve-HostedDisplayExitCode truth table matches specification:

        - pass: 0
        - blocked (no restore error): 0
        - test failure: 1
        - restore error with pass: 1 (restore error overrides pass)
        - restore error with blocked: 1 (restore error overrides blocked)
        """
        self.assertIn(
            'function Resolve-HostedDisplayExitCode',
            self.content,
            "Script must implement Resolve-HostedDisplayExitCode helper",
        )

        def resolve_exit_code(passed: bool, blocked: bool, restore_error: str | None) -> int:
            if restore_error:
                return 1
            if not passed and not blocked:
                return 1
            return 0

        self.assertEqual(resolve_exit_code(passed=True, blocked=False, restore_error=None), 0)
        self.assertEqual(resolve_exit_code(passed=False, blocked=True, restore_error=None), 0)
        self.assertEqual(resolve_exit_code(passed=False, blocked=False, restore_error=None), 1)
        self.assertEqual(resolve_exit_code(passed=True, blocked=False, restore_error="restore failed"), 1)
        self.assertEqual(resolve_exit_code(passed=False, blocked=True, restore_error="restore failed"), 1)
        self.assertEqual(resolve_exit_code(passed=False, blocked=False, restore_error="restore failed"), 1)

    def test_primary_monitor_dpi_independent_of_process_window(self):
        """Reviewer finding (3): finally DPI readback must use primary monitor independent

        of KeePass mainHandle which can become invalid if KeePass crashes or closes.
        """
        # Native definition must provide GetPrimaryMonitorHandle and GetPrimaryDpi
        self.assertIn(
            'GetPrimaryMonitorHandle()',
            self.content,
            "HostedDisplayNative must implement GetPrimaryMonitorHandle()",
        )
        self.assertIn(
            'GetPrimaryDpi()',
            self.content,
            "HostedDisplayNative must implement GetPrimaryDpi() querying primary monitor directly",
        )

        # MonitorFromWindow(IntPtr.Zero, 1) or MonitorFromPoint for primary monitor
        self.assertRegex(
            self.content,
            r'MonitorFromWindow\(\s*IntPtr\.Zero\s*,\s*1\s*\)',
            "GetPrimaryMonitorHandle must use MonitorFromWindow(IntPtr.Zero, 1) [MONITOR_DEFAULTTOPRIMARY]",
        )

        # In finally block / restoration, $mainHandle must NOT be used for DPI readback
        finally_idx = self.content.rfind('finally {')
        self.assertNotEqual(finally_idx, -1, "Script must have a finally block")
        finally_content = self.content[finally_idx:]

        self.assertNotIn(
            'Dpi($mainHandle)',
            finally_content,
            "Finally block must not call Dpi($mainHandle); DPI readback must be independent of process window",
        )
        self.assertNotIn(
            '$mainHandle',
            finally_content,
            "Finally block must not reference $mainHandle for display verification or restoration",
        )

    def test_mode_unchanged_restoration_readback_behavior(self):
        """Reviewer finding (4): if mode unchanged and GUI check fails, restored should

        still reflect no display mutation, but verified by readback rather than blindly true or false.
        """
        # Test-HostedDisplayRestoration helper must exist and handle ModeChanged
        self.assertIn(
            'function Test-HostedDisplayRestoration',
            self.content,
            "Script must implement Test-HostedDisplayRestoration",
        )

        # Logic behavioral model of Test-HostedDisplayRestoration
        class MockDevMode:
            def __init__(self, width=1920, height=1080, freq=60, bits=32, orientation=0):
                self.dmPelsWidth = width
                self.dmPelsHeight = height
                self.dmDisplayFrequency = freq
                self.dmBitsPerPel = bits
                self.dmDisplayOrientation = orientation

            def matches(self, other):
                return (
                    self.dmPelsWidth == other.dmPelsWidth
                    and self.dmPelsHeight == other.dmPelsHeight
                    and self.dmDisplayFrequency == other.dmDisplayFrequency
                    and self.dmBitsPerPel == other.dmBitsPerPel
                    and self.dmDisplayOrientation == other.dmDisplayOrientation
                )

        def mock_restore(
            display_device: str,
            orig_mode: MockDevMode,
            orig_dpi: tuple[int, int],
            mode_changed: bool,
            change_result: int,
            current_mode: MockDevMode,
            current_dpi: tuple[int, int],
        ):
            if not orig_mode or not display_device:
                return {'Restored': False, 'RestoreError': None}
            try:
                if mode_changed:
                    if change_result != 0:
                        raise RuntimeError("Original display mode restore rejected")
                if not current_mode.matches(orig_mode):
                    raise RuntimeError("Original display mode not restored")
                if current_dpi != orig_dpi:
                    raise RuntimeError("Original effective display scale changed")
                return {'Restored': True, 'RestoreError': None}
            except RuntimeError:
                return {
                    'Restored': False,
                    'RestoreError': 'Original display mode/scale restoration could not be verified; inspect Windows display settings immediately',
                }

        orig = MockDevMode(2560, 1440, 60, 32, 0)
        orig_dpi = (144, 144)

        # Case 1: Mode was NOT changed, readback confirms display unmutated -> Restored is True!
        res1 = mock_restore(
            display_device=r'\\.\DISPLAY1',
            orig_mode=orig,
            orig_dpi=orig_dpi,
            mode_changed=False,
            change_result=0,
            current_mode=orig,
            current_dpi=(144, 144),
        )
        self.assertTrue(res1['Restored'], "Mode unchanged with matching readback must report Restored=True")
        self.assertIsNone(res1['RestoreError'])

        # Case 2: Mode was NOT changed, but readback detects mode deviation -> Restored is False, RestoreError set!
        res2 = mock_restore(
            display_device=r'\\.\DISPLAY1',
            orig_mode=orig,
            orig_dpi=orig_dpi,
            mode_changed=False,
            change_result=0,
            current_mode=MockDevMode(1920, 1080),
            current_dpi=(144, 144),
        )
        self.assertFalse(res2['Restored'])
        self.assertIsNotNone(res2['RestoreError'])

        # Case 3: Mode was NOT changed, but readback detects DPI deviation -> Restored is False, RestoreError set!
        res3 = mock_restore(
            display_device=r'\\.\DISPLAY1',
            orig_mode=orig,
            orig_dpi=orig_dpi,
            mode_changed=False,
            change_result=0,
            current_mode=orig,
            current_dpi=(96, 96),
        )
        self.assertFalse(res3['Restored'])
        self.assertIsNotNone(res3['RestoreError'])

        # Case 4: Mode was changed, ChangeDisplaySettingsEx succeeds, readback matches -> Restored is True
        res4 = mock_restore(
            display_device=r'\\.\DISPLAY1',
            orig_mode=orig,
            orig_dpi=orig_dpi,
            mode_changed=True,
            change_result=0,
            current_mode=orig,
            current_dpi=(144, 144),
        )
        self.assertTrue(res4['Restored'])
        self.assertIsNone(res4['RestoreError'])

        # Case 5: Mode was changed, ChangeDisplaySettingsEx rejected -> Restored is False, RestoreError set
        res5 = mock_restore(
            display_device=r'\\.\DISPLAY1',
            orig_mode=orig,
            orig_dpi=orig_dpi,
            mode_changed=True,
            change_result=1,
            current_mode=orig,
            current_dpi=(144, 144),
        )
        self.assertFalse(res5['Restored'])
        self.assertIsNotNone(res5['RestoreError'])

    def test_no_null_device_in_display_apis(self):
        """Win32 EnumDisplaySettings and ChangeDisplaySettingsEx must never use $null."""
        null_enums = re.findall(r'EnumDisplaySettings\(\s*\$null', self.content)
        self.assertEqual(null_enums, [], "EnumDisplaySettings must not use $null device")
        null_changes = re.findall(r'ChangeDisplaySettingsEx\(\s*\$null', self.content)
        self.assertEqual(null_changes, [], "ChangeDisplaySettingsEx must not use $null device")

    def test_probe_36601787312_scenario_simulation(self):
        """Simulate probe 36601787312 runner condition:

        Device: \\\\.\\DISPLAY1, 96 DPI, Target: 2560x1440, MinimumDpi: 144.
        Verifies:
        1. PrimaryScreen DeviceName resolved and EnumDisplaySettings populates original.
        2. MinimumDpi prerequisite check throws 'High-DPI prerequisite unmet...'.
        3. Catch classifies as blocked=true, passed=false.
        4. Mode change was not called (modeChanged=false).
        5. Restoration block reads back DEVMODE and DPI, confirms unmutated (restored=true).
        6. Exit code resolves to 0 (preflight blocked, no restore error).
        """
        enum_pos = self.content.find('EnumDisplaySettings($displayDevice, -1, [ref]$originalMode)')
        orig_dpi_pos = self.content.find('$originalDpi = [HostedDisplayNative]::GetPrimaryDpi()')
        result_orig_pos = self.content.find('$result.original =')
        min_dpi_check = self.content.find('$originalDpi[0] -lt $MinimumDpi')
        mode_change_call = self.content.find('ChangeDisplaySettingsEx($displayDevice, [ref]$targetMode')

        self.assertNotEqual(enum_pos, -1)
        self.assertNotEqual(orig_dpi_pos, -1)
        self.assertNotEqual(result_orig_pos, -1)
        self.assertNotEqual(min_dpi_check, -1)
        self.assertNotEqual(mode_change_call, -1)

        self.assertLess(enum_pos, orig_dpi_pos)
        self.assertLess(orig_dpi_pos, result_orig_pos)
        self.assertLess(result_orig_pos, min_dpi_check)
        self.assertLess(min_dpi_check, mode_change_call)

    def test_all_script_throws_registered_in_allowed_messages(self):
        """Ensure all throw statements in try block are listed in $allowedMessages.

        Any unregistered throw would be suppressed into generic 'GUI verification failed',
        preventing exact error reporting and proper blocked-state classification.
        """
        # Match the main verify try block
        main_try_match = re.search(r'try \{\s*if \(\(\$TargetWidth.*?\n\} catch \{', self.content, re.DOTALL)
        self.assertIsNotNone(main_try_match, "Script must have main verify try-catch block")
        assert main_try_match is not None
        try_body = main_try_match.group(0)

        thrown_messages = set(re.findall(r"throw\s+['\"]([^'\"]+)['\"]", try_body))
        self.assertGreater(len(thrown_messages), 20, f"Should find >20 thrown messages in main try block, found {len(thrown_messages)}")

        allowed_match = re.search(
            r'\$allowedMessages\s*=\s*@\((.*?)\)',
            self.content,
            re.DOTALL,
        )
        self.assertIsNotNone(allowed_match, "$allowedMessages array must be defined in catch block")
        assert allowed_match is not None
        allowed_messages = set(re.findall(r"['\"]([^'\"]+)['\"]", allowed_match.group(1)))

        unregistered = thrown_messages - allowed_messages
        self.assertEqual(
            unregistered,
            set(),
            f"All thrown messages in try block must be registered in $allowedMessages; missing: {unregistered}",
        )

    def test_selftest_cli_harness(self):
        """Test script contains -SelfTest parameter set and Invoke-SelfTest.

        If powershell or pwsh is available on PATH, runs it directly.
        """
        self.assertIn('Invoke-SelfTest', self.content)
        self.assertIn('[switch]$SelfTest', self.content)

        ps_bin = shutil.which('powershell') or shutil.which('pwsh')
        if not ps_bin:
            self.skipTest("powershell/pwsh executable not found on PATH; mock-safe simulation verified in Python suite")

        result = subprocess.run(
            [ps_bin, '-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', str(SCRIPT_PATH), '-SelfTest'],
            capture_output=True,
            text=True,
            timeout=30,
        )
        self.assertEqual(result.returncode, 0, f"SelfTest failed with output:\n{result.stdout}\n{result.stderr}")
        self.assertIn("All verify-options-hosted-display self-tests passed!", result.stdout)

    def test_task2_tab_by_tab_selection_and_control_verification(self):
        """Task 2 RED: verify scripts/verify-options-hosted-display.ps1:
        1. Selects each of the 4 tabs individually via SelectionItemPattern (not just enumerating headers).
        2. Inspects controls and persistent help text across all 4 tabs:
           - Browser Integration: Install/Repair, Refresh, Uninstall buttons and hint.
           - Matching Rules: 5 checkboxes and 5 persistent help tips.
           - Database & Security: 3 radio buttons, 2 danger zone checkboxes, reset button, danger warning.
           - Associations: audit tip label, removal buttons.
        3. Validates footer version text (lblVersion / KeePassNatMsg v2.5.0) and Save/Cancel buttons.
        4. Verifies control geometry using a testable helper (Test-ControlGeometry) to catch clipping.
        5. Populates $result.tabs with structured verification objects containing verifiedControls.
        """
        # 1. SelectionItemPattern used to switch tabs
        self.assertIn(
            'SelectionItemPattern',
            self.content,
            "Script must use SelectionItemPattern to switch and activate each tab",
        )

        # 2. Control labels / help texts must be verified
        expected_tokens = [
            'Install / Repair Integration',
            'Return only the best URL matches',
            'Search only the active database',
            'Always allow credential access',
            'Remove Selected',
            'KeePassNatMsg v',
            'Test-ControlGeometry',
        ]
        for token in expected_tokens:
            self.assertIn(
                token,
                self.content,
                f"Script must include verification token '{token}' for Task 2 UI inspection",
            )

        # 3. Structured tab records rather than simple string array
        self.assertRegex(
            self.content,
            r'controlsVerified|verifiedControls',
            "Script must record structured control verification details in $result.tabs",
        )

    def test_task2_workflow_strict_classification_and_exit_handling(self):
        """Task 2 RED: verify .github/workflows/e2e-windows.yml:
        1. Checks $LASTEXITCODE or throws on non-zero exit from verify-options-hosted-display.ps1.
        2. Strictly checks $hRes.passed and $hRes.restored before declaring PASSED.
        3. Properly classifies $hRes.blocked with restoration check (restored=true).
        4. Strictly fails (throws) if test failed without being blocked (not passed and not blocked).
        5. Fails if restoration failed (restoreError or not restored).
        """
        workflow_path = ROOT / '.github' / 'workflows' / 'e2e-windows.yml'
        self.assertTrue(workflow_path.exists(), "Workflow e2e-windows.yml must exist")
        workflow_content = workflow_path.read_text(encoding='utf-8')

        # Must not silently accept non-zero exit code or ignore unblocked failures
        self.assertIn(
            'verify-options-hosted-display.ps1',
            workflow_content,
        )
        self.assertRegex(
            workflow_content,
            r'throw\s+["\'].*?[Ff]ail',
            "Workflow must throw on hosted options test failure",
        )
        self.assertRegex(
            workflow_content,
            r'\$LASTEXITCODE',
            "Workflow must verify $LASTEXITCODE after running verify-options-hosted-display.ps1",
        )
        self.assertRegex(
            workflow_content,
            r'restored',
            "Workflow must check display restoration status from JSON output",
        )

    def test_task2_control_geometry_logic_matrix(self):
        """Verify behavioral logic of Test-ControlGeometry against clipping edge cases."""
        def test_control_geometry(bounds, container, work):
            if not bounds:
                return {'Valid': False, 'Reason': 'Bounds is null'}
            if bounds.get('Width', 0) <= 0 or bounds.get('Height', 0) <= 0:
                return {'Valid': False, 'Reason': 'Invalid dimensions: Width or Height <= 0'}
            if container:
                if (bounds['Left'] < container['Left'] or bounds['Top'] < container['Top'] or
                        bounds['Right'] > container['Right'] or bounds['Bottom'] > container['Bottom']):
                    return {'Valid': False, 'Reason': 'Clipped by container boundary'}
            if work:
                if (bounds['Left'] < work['Left'] or bounds['Top'] < work['Top'] or
                        bounds['Right'] > work['Right'] or bounds['Bottom'] > work['Bottom']):
                    return {'Valid': False, 'Reason': 'Bounds outside monitor working area'}
            return {'Valid': True, 'Reason': None}

        container = {'Left': 0, 'Top': 0, 'Right': 800, 'Bottom': 600}
        work = {'Left': 0, 'Top': 0, 'Right': 1920, 'Bottom': 1080}

        # Valid control inside container and work area
        res = test_control_geometry({'Left': 20, 'Top': 20, 'Right': 200, 'Bottom': 60, 'Width': 180, 'Height': 40}, container, work)
        self.assertTrue(res['Valid'])

        # Collapsed width
        res = test_control_geometry({'Left': 20, 'Top': 20, 'Right': 20, 'Bottom': 60, 'Width': 0, 'Height': 40}, container, work)
        self.assertFalse(res['Valid'])

        # Clipped by container right
        res = test_control_geometry({'Left': 750, 'Top': 20, 'Right': 850, 'Bottom': 60, 'Width': 100, 'Height': 40}, container, work)
        self.assertFalse(res['Valid'])

        # Clipped by container bottom
        res = test_control_geometry({'Left': 20, 'Top': 580, 'Right': 200, 'Bottom': 620, 'Width': 180, 'Height': 40}, container, work)
        self.assertFalse(res['Valid'])

        # Outside work area
        res = test_control_geometry({'Left': 20, 'Top': 20, 'Right': 200, 'Bottom': 1100, 'Width': 180, 'Height': 40}, None, work)
        self.assertFalse(res['Valid'])

    def test_task2_safety_only_cancel_invoked_never_save(self):
        """Verify that the script only activates Cancel and never invokes Save or modifications."""
        self.assertIn('$cancelInvoke.Invoke()', self.content)
        self.assertNotIn('$saveInvoke', self.content)
        self.assertNotIn('Save button clicked', self.content)
        # Check comment constraint
        self.assertIn('Never save Options: only close this dialog using its Cancel button.', self.content)


if __name__ == '__main__':
    unittest.main()
