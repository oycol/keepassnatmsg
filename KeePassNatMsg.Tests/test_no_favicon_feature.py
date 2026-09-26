"""Regression guard for complete removal of the optional favicon downloader.
Run: python3 -m unittest discover -s KeePassNatMsg.Tests -p test_no_favicon_feature.py -v
This checks repository integration; Windows CI validates the compiled plugin.
"""
from pathlib import Path
import unittest

ROOT = Path(__file__).resolve().parents[1]


class NoFaviconFeature(unittest.TestCase):
    def test_no_downloader_sources_or_menu_and_options_integration(self):
        self.assertFalse((ROOT / 'KeePassNatMsg/Favicon').exists())
        self.assertFalse((ROOT / 'KeePassNatMsg.Tests/FaviconTests.cs').exists())
        self.assertFalse((ROOT / 'KeePassNatMsg.Tests/test_favicon_manager_races.py').exists())
        self.assertFalse((ROOT / 'KeePassNatMsg/Resources/favicon_download_16.png').exists())
        paths = [
            'KeePassNatMsg/KeePassNatMsgExt.cs',
            'KeePassNatMsg/ConfigOpt.cs',
            'KeePassNatMsg/Options/OptionsForm.cs',
            'KeePassNatMsg/Options/OptionsForm.Designer.cs',
            'KeePassNatMsg/KeePassNatMsg.csproj',
            'KeePassNatMsg.Tests/KeePassNatMsg.Tests.csproj',
            'KeePassNatMsg/Properties/Resources.resx',
            'KeePassNatMsg/Properties/Resources.Designer.cs',
            'scripts/verify-options-gui.ps1',
        ]
        for path in paths:
            with self.subTest(path=path):
                self.assertNotIn('favicon', (ROOT / path).read_text(encoding='utf-8-sig').lower())


if __name__ == '__main__':
    unittest.main()
