"""Source-contract regressions for FaviconManager; run with python3 -m unittest.

These checks do not replace Windows KeePass integration tests.
"""
import pathlib
import re
import unittest

SOURCE = pathlib.Path(__file__).resolve().parents[1] / 'KeePassNatMsg' / 'Favicon' / 'FaviconManager.cs'


class FaviconManagerRaceTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.source = SOURCE.read_text(encoding='utf-8')

    def test_cancel_aborts_active_downloaders_without_network_abort_on_caller_thread(self):
        cancel = self.source.split('public void Cancel()', 1)[1].split('private sealed class', 1)[0]
        self.assertIn('current.Cancel();', cancel)
        self.assertIn('ThreadPool.QueueUserWorkItem', cancel)
        self.assertIn('downloader.Abort()', cancel.split('ThreadPool.QueueUserWorkItem', 1)[1])
        self.assertIn('_activeDownloaders.Add(fd)', self.source)
        self.assertIn('_activeDownloaders.Remove(fd)', self.source)

    def test_database_commit_checks_cancellation_inside_database_lock(self):
        commit = self.source.split('byte[] hash = ComputeSha256(iconBytes);', 1)[1].split('Interlocked.Increment(ref progress.Success);', 1)[0]
        self.assertIn('lock (_stateLock)', commit)
        self.assertIn('lock (database)', commit)
        self.assertIn('if (ct.IsCancellationRequested || !database.IsOpen) return;', commit)
        self.assertIn('database.CustomIcons.Add(customIcon)', commit)
        self.assertNotIn('_host.Database.CustomIcons', commit)

    def test_late_worker_never_uses_disposed_logger_or_database(self):
        self.assertRegex(self.source, r'if \(!ct\.IsCancellationRequested\)\s+try \{ logger\.SetProgress\(pct\);')
        self.assertIn('if (ct.IsCancellationRequested) return;\n                                _activeDownloaders.Add(fd);', self.source)

    def test_ui_updates_are_dispatched_on_main_window(self):
        cleanup = self.source.split('MainWindow.BeginInvoke', 1)[1].split('lock (_stateLock)', 1)[0]
        self.assertIn('UIBlockInteraction(false)', cleanup)
        self.assertIn('UpdateUI(', cleanup)
        self.assertNotIn('MainWindow.Invoke(', self.source)


if __name__ == '__main__':
    unittest.main()
