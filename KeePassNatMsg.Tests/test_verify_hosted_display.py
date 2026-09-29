"""Regression and safety verification for KeePass-hosted display validation script.

Validates that scripts/verify-options-hosted-display.ps1:
1. Uses explicit primary screen DeviceName instead of $null for Win32
   EnumDisplaySettings and ChangeDisplaySettingsEx (Runner probe 36601787312
   proved EnumDisplaySettings($null) fails on the dedicated Windows runner).
2. Distinguishes preflight prerequisites (blocked=true, passed=false, exit 0)
   from actual test failure (passed=false, exit 1) and restore failure (exit 1).
3. Safely restores display settings in finally and reads back DEVMODE and DPI.
4. Correctly handles probe 36601787312 scenario (96 DPI / 100% scale blocks without
   claiming DPI passed and without attempting mode switch).

Run with:
python3 -m unittest discover -s KeePassNatMsg.Tests -p test_verify_hosted_display.py -v
"""
import re
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SCRIPT_PATH = ROOT / 'scripts' / 'verify-options-hosted-display.ps1'


class TestVerifyHostedDisplay(unittest.TestCase):
    def setUp(self):
        self.assertTrue(SCRIPT_PATH.exists(), f"Script must exist at {SCRIPT_PATH}")
        self.content = SCRIPT_PATH.read_text(encoding='utf-8')

    def test_no_null_device_in_enum_display_settings(self):
        """EnumDisplaySettings($null, ...) failed on runner (probe 36601787312).

        Must use explicit display device name.
        """
        null_enums = re.findall(r'EnumDisplaySettings\(\s*\$null', self.content)
        self.assertEqual(
            null_enums,
            [],
            "EnumDisplaySettings must not use $null device; use $displayDevice or Screen.PrimaryScreen.DeviceName",
        )

    def test_no_null_device_in_change_display_settings_ex(self):
        """ChangeDisplaySettingsEx($null, ...) must use explicit display device name."""
        null_changes = re.findall(r'ChangeDisplaySettingsEx\(\s*\$null', self.content)
        self.assertEqual(
            null_changes,
            [],
            "ChangeDisplaySettingsEx must not use $null device; use $displayDevice",
        )

    def test_uses_explicit_primary_screen_device_name(self):
        """Script must determine primary screen DeviceName and use it for display calls."""
        self.assertIn(
            'PrimaryScreen',
            self.content,
            "Script must inspect [System.Windows.Forms.Screen]::PrimaryScreen",
        )
        self.assertRegex(
            self.content,
            r'EnumDisplaySettings\(\s*\$displayDevice',
            "EnumDisplaySettings must be called with $displayDevice",
        )
        self.assertRegex(
            self.content,
            r'ChangeDisplaySettingsEx\(\s*\$displayDevice',
            "ChangeDisplaySettingsEx must be called with $displayDevice",
        )

    def test_result_structure_has_blocked_field(self):
        """JSON output must include 'blocked' field to distinguish preflight blocks from passes/failures."""
        self.assertRegex(
            self.content,
            r'\$result\s*=\s*\[ordered\]@\{[^}]*blocked\s*=',
            "Initial $result hashtable must contain 'blocked' field",
        )

    def test_exit_code_distinguishes_blocked_from_test_failure(self):
        """Exit code must be non-zero on test or restore failure, but 0 when preflight is blocked."""
        # Must check restoreError
        self.assertRegex(
            self.content,
            r'if\s*\(\$result\.restoreError\)\s*\{\s*exit 1\s*\}',
            "Must exit 1 if restoreError is present",
        )
        # Must check failure when not blocked
        self.assertRegex(
            self.content,
            r'if\s*\(.*-not\s*\$result\.passed.*-not\s*\$result\.blocked.*\)\s*\{\s*exit 1\s*\}',
            "Must exit 1 if test failed and was not blocked",
        )

    def test_finally_block_restores_and_verifies_with_display_device(self):
        """Finally block must restore mode with $displayDevice and read back DEVMODE and DPI."""
        finally_idx = self.content.rfind('finally {')
        self.assertNotEqual(finally_idx, -1, "Script must have a finally block")
        finally_content = self.content[finally_idx:]

        self.assertIn(
            'ChangeDisplaySettingsEx($displayDevice',
            finally_content,
            "Finally block must restore with $displayDevice",
        )
        self.assertIn(
            'EnumDisplaySettings($displayDevice',
            finally_content,
            "Finally block must read back mode with $displayDevice",
        )

    def test_probe_36601787312_scenario_simulation(self):
        """Simulate probe 36601787312 runner condition:

        Device: \\\\.\\DISPLAY1, 96 DPI, Target: 2560x1440, MinimumDpi: 144.
        In the old script: EnumDisplaySettings($null) returned false, causing original=null.
        In the fixed script: EnumDisplaySettings($displayDevice) succeeds, captures original
        width/height/dpi, but fails the MinimumDpi check, resulting in blocked=true, passed=false,
        and no display change.
        """
        # Static check of logic order:
        # 1. $displayDevice resolved from PrimaryScreen
        # 2. EnumDisplaySettings($displayDevice, -1, ...) populates original
        # 3. MinimumDpi checked -> throws prerequisite unmet -> marked blocked
        # 4. Mode change only occurs AFTER MinimumDpi check
        enum_pos = self.content.find('EnumDisplaySettings($displayDevice, -1, [ref]$originalMode)')
        dpi_pos = self.content.find('$result.original =')
        min_dpi_check = self.content.find('$originalDpi[0] -lt $MinimumDpi')
        mode_change_call = self.content.find('ChangeDisplaySettingsEx($displayDevice, [ref]$targetMode')

        self.assertNotEqual(enum_pos, -1, "EnumDisplaySettings for originalMode must use $displayDevice")
        self.assertNotEqual(dpi_pos, -1, "$result.original must be populated")
        self.assertNotEqual(min_dpi_check, -1, "MinimumDpi must be checked")
        self.assertNotEqual(mode_change_call, -1, "Mode change must use $displayDevice")

        self.assertLess(enum_pos, dpi_pos, "EnumDisplaySettings must precede $result.original assignment")
        self.assertLess(dpi_pos, min_dpi_check, "$result.original assignment must precede MinimumDpi check")
        self.assertLess(min_dpi_check, mode_change_call, "MinimumDpi check must precede display mode change")


if __name__ == '__main__':
    unittest.main()
