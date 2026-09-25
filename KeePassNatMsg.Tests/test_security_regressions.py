"""Source-level fail-closed regression guards for the KeePass-dependent paths.

Run with python3 -m unittest discover -s KeePassNatMsg.Tests -p test_security_regressions.py -v.
These do not replace NUnit or Windows KeePass integration tests.
"""
import pathlib
import re
import unittest

ROOT = pathlib.Path(__file__).resolve().parents[1] / 'KeePassNatMsg'


def source(path):
    return (ROOT / path).read_text(encoding='utf-8')


class SecurityRegressions(unittest.TestCase):
    def test_update_uuid_is_gated_by_url_matching(self):
        update = source('Entry/EntryUpdate.cs')
        self.assertIn('UrlMatchingHelper.MatchesUrl(', update)
        self.assertIn('GetEntryUrls(', update)
        self.assertIn('Uri.TryCreate(formHost', update)

    def test_explicit_db_selection_has_no_fallback(self):
        ext = source('KeePassNatMsgExt.cs')
        connection = ext.split('public PwDatabase GetConnectionDatabase()', 1)[1].split('public PwDatabase GetSearchDatabase()', 1)[0]
        search = ext.split('public PwDatabase GetSearchDatabase()', 1)[1].split('internal void PromptToMigrate', 1)[0]
        for block in (connection, search):
            self.assertIn('if (document != null)', block)
            self.assertIn('return document.Database;', block)
            self.assertIn('else\n                    return null;', block)
        for path in ('Entry/EntrySearch.cs', 'Entry/EntryUpdate.cs', 'Protocol/Handlers.cs'):
            text = source(path)
            self.assertTrue('db == null' in text or 'searchDb == null' in text, path + ' must handle absent db')

    def test_candidate_key_includes_db_identity(self):
        search = source('Entry/EntrySearch.cs')
        self.assertNotIn('Dictionary<string, PwEntryDatabase>', search)
        self.assertIn('candidates.Add(entry, new PwEntryDatabase(entry, db))', search)

    def test_public_suffix_cannot_authorize_subdomain(self):
        helper = source('Entry/UrlMatchingHelper.cs')
        self.assertRegex(helper, r'IsPublicSuffix\(cleanEntryHost\)|IsPublicSuffix\(current\)')
        self.assertIn('co.uk', helper)
        self.assertIn('github.io', helper)


if __name__ == '__main__':
    unittest.main()
