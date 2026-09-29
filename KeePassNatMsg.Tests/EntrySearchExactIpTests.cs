using System;
using System.Collections.Generic;
using System.Linq;
using KeePass.App.Configuration;
using KeePassLib;
using KeePassLib.Keys;
using KeePassLib.Security;
using KeePassLib.Serialization;
using KeePassNatMsg.Entry;
using NUnit.Framework;

namespace KeePassNatMsg.Tests
{
    [TestFixture]
    public class EntrySearchExactIpTests
    {
        private EntrySearch _search;
        private KeePassNatMsgExt _ext;

        [SetUp]
        public void SetUp()
        {
            _ext = new KeePassNatMsgExt();
            _search = new EntrySearch(null, _ext);
        }

        private static PwDatabase CreateTestDatabase()
        {
            var db = new PwDatabase();
            db.New(new IOConnectionInfo(), new CompositeKey());
            return db;
        }

        private static PwEntry CreateEntry(string title, string username, string password, string url, bool expires = false, DateTime? expiryTime = null)
        {
            var entry = new PwEntry(true, true);
            entry.Strings.Set(PwDefs.TitleField, new ProtectedString(false, title ?? string.Empty));
            entry.Strings.Set(PwDefs.UserNameField, new ProtectedString(false, username ?? string.Empty));
            entry.Strings.Set(PwDefs.PasswordField, new ProtectedString(true, password ?? string.Empty));
            if (!string.IsNullOrEmpty(url))
            {
                entry.Strings.Set(PwDefs.UrlField, new ProtectedString(false, url));
            }
            if (expires && expiryTime.HasValue)
            {
                entry.Expires = true;
                entry.ExpiryTime = expiryTime.Value;
            }
            return entry;
        }

        [Test]
        public void ExactIpAndCidrBothMatch_OnlyExactIpEntriesReturned()
        {
            var db = CreateTestDatabase();
            var exactEntry = CreateEntry("Exact Host", "admin_exact", "secret1", "https://10.125.1.8/");
            var cidrEntry = CreateEntry("CIDR Subnet", "admin_cidr", "secret2", "CIDR:10.125.1.0/24");

            db.RootGroup.AddEntry(exactEntry, true);
            db.RootGroup.AddEntry(cidrEntry, true);

            var matches = _search.FindMatchingEntries(new Uri("https://10.125.1.8/"), null, new[] { db }).ToList();
            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual("Exact Host", matches[0].entry.Strings.ReadSafe(PwDefs.TitleField));

            var count = _search.CountMatchingEntries("https://10.125.1.8/", new[] { db });
            Assert.AreEqual(1, count);
        }

        [Test]
        public void MultipleExactAccounts_AllExactAccountsReturned_CidrExcluded()
        {
            var db = CreateTestDatabase();
            var exact1 = CreateEntry("Exact Admin 1", "admin1", "secret1", "https://10.125.1.8/");
            var exact2 = CreateEntry("Exact Admin 2", "admin2", "secret2", "https://10.125.1.8/");
            var cidrEntry = CreateEntry("CIDR Subnet", "cidr_user", "secret3", "CIDR:10.125.1.0/24");

            db.RootGroup.AddEntry(exact1, true);
            db.RootGroup.AddEntry(exact2, true);
            db.RootGroup.AddEntry(cidrEntry, true);

            var matches = _search.FindMatchingEntries(new Uri("https://10.125.1.8/"), null, new[] { db }).ToList();
            Assert.AreEqual(2, matches.Count);
            var titles = matches.Select(delegate(PwEntryDatabase m) { return m.entry.Strings.ReadSafe(PwDefs.TitleField); }).OrderBy(delegate(string t) { return t; }).ToList();
            CollectionAssert.AreEqual(new[] { "Exact Admin 1", "Exact Admin 2" }, titles);

            var count = _search.CountMatchingEntries("https://10.125.1.8/", new[] { db });
            Assert.AreEqual(2, count);
        }

        [Test]
        public void SameEntry_WithExactIpAndCidr_IsKept()
        {
            var db = CreateTestDatabase();
            var mixedEntry = CreateEntry("Mixed Exact And CIDR", "admin_mixed", "secret1", "https://10.125.1.8/, CIDR:10.125.1.0/24");
            var cidrOnlyEntry = CreateEntry("CIDR Only Subnet", "admin_cidr", "secret2", "CIDR:10.125.1.0/24");

            db.RootGroup.AddEntry(mixedEntry, true);
            db.RootGroup.AddEntry(cidrOnlyEntry, true);

            var matches = _search.FindMatchingEntries(new Uri("https://10.125.1.8/"), null, new[] { db }).ToList();
            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual("Mixed Exact And CIDR", matches[0].entry.Strings.ReadSafe(PwDefs.TitleField));

            var count = _search.CountMatchingEntries("https://10.125.1.8/", new[] { db });
            Assert.AreEqual(1, count);
        }

        [Test]
        public void OnlyCidrMatches_CidrEntryReturned()
        {
            var db = CreateTestDatabase();
            var cidrEntry = CreateEntry("CIDR Subnet", "admin_cidr", "secret2", "CIDR:10.125.1.0/24");
            db.RootGroup.AddEntry(cidrEntry, true);

            var matches = _search.FindMatchingEntries(new Uri("https://10.125.1.8/"), null, new[] { db }).ToList();
            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual("CIDR Subnet", matches[0].entry.Strings.ReadSafe(PwDefs.TitleField));

            var count = _search.CountMatchingEntries("https://10.125.1.8/", new[] { db });
            Assert.AreEqual(1, count);
        }

        [Test]
        public void ExpiredExactEntry_DoesNotSuppressCidr()
        {
            var db = CreateTestDatabase();
            var expiredExact = CreateEntry("Expired Exact", "admin_exact", "secret1", "https://10.125.1.8/", true, DateTime.UtcNow.AddMinutes(-5));
            var cidrEntry = CreateEntry("CIDR Subnet", "admin_cidr", "secret2", "CIDR:10.125.1.0/24");

            db.RootGroup.AddEntry(expiredExact, true);
            db.RootGroup.AddEntry(cidrEntry, true);

            var configOpt = new ConfigOpt(new AceCustomConfig());
            configOpt.HideExpired = true;

            var matches = _search.FindMatchingEntries(new Uri("https://10.125.1.8/"), null, new[] { db }, configOpt).ToList();
            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual("CIDR Subnet", matches[0].entry.Strings.ReadSafe(PwDefs.TitleField));

            var count = _search.CountMatchingEntries("https://10.125.1.8/", new[] { db }, configOpt);
            Assert.AreEqual(1, count);
        }

        [Test]
        public void DeniedExactEntry_DoesNotSuppressCidr()
        {
            var db = CreateTestDatabase();
            var deniedExact = CreateEntry("Denied Exact", "admin_exact", "secret1", "https://10.125.1.8/");
            var cidrEntry = CreateEntry("CIDR Subnet", "admin_cidr", "secret2", "CIDR:10.125.1.0/24");

            var config = new EntryConfig();
            config.Deny.Add("10.125.1.8");
            _ext.SetEntryConfig(deniedExact, config);

            db.RootGroup.AddEntry(deniedExact, true);
            db.RootGroup.AddEntry(cidrEntry, true);

            var matches = _search.FindMatchingEntries(new Uri("https://10.125.1.8/"), null, new[] { db }).ToList();
            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual("CIDR Subnet", matches[0].entry.Strings.ReadSafe(PwDefs.TitleField));

            var count = _search.CountMatchingEntries("https://10.125.1.8/", new[] { db });
            Assert.AreEqual(1, count);
        }

        [Test]
        public void AdditionalUrlFields_ExactIpInAdditionalField_WinsOverCidr()
        {
            var db = CreateTestDatabase();
            var exactInAdditional = CreateEntry("Exact In Additional", "admin_add", "secret1", "https://primary.example.com/");
            exactInAdditional.Strings.Set("URL1", new ProtectedString(false, "https://10.125.1.8/"));
            var cidrEntry = CreateEntry("CIDR Subnet", "admin_cidr", "secret2", "CIDR:10.125.1.0/24");

            db.RootGroup.AddEntry(exactInAdditional, true);
            db.RootGroup.AddEntry(cidrEntry, true);

            var configOpt = new ConfigOpt(new AceCustomConfig());
            configOpt.SearchUrls = true;

            var matches = _search.FindMatchingEntries(new Uri("https://10.125.1.8/"), null, new[] { db }, configOpt).ToList();
            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual("Exact In Additional", matches[0].entry.Strings.ReadSafe(PwDefs.TitleField));

            var count = _search.CountMatchingEntries("https://10.125.1.8/", new[] { db }, configOpt);
            Assert.AreEqual(1, count);
        }

        [Test]
        public void NonIpv4Request_ExactPrecedenceDoesNotAffectDomainMatching()
        {
            var db = CreateTestDatabase();
            var entry1 = CreateEntry("Domain Entry 1", "user1", "secret1", "https://bmc.example.com/login");
            var entry2 = CreateEntry("Domain Entry 2", "user2", "secret2", "https://bmc.example.com/other");

            db.RootGroup.AddEntry(entry1, true);
            db.RootGroup.AddEntry(entry2, true);

            var matches = _search.FindMatchingEntries(new Uri("https://bmc.example.com/"), null, new[] { db }).ToList();
            Assert.AreEqual(2, matches.Count);

            var count = _search.CountMatchingEntries("https://bmc.example.com/", new[] { db });
            Assert.AreEqual(2, count);
        }

        [Test]
        public void MultipleDatabases_ExactInOneDb_SuppressesCidrInOtherDb()
        {
            var db1 = CreateTestDatabase();
            var db2 = CreateTestDatabase();

            var exactEntry = CreateEntry("Db1 Exact", "admin_exact", "secret1", "https://10.125.1.8/");
            var cidrEntry = CreateEntry("Db2 CIDR", "admin_cidr", "secret2", "CIDR:10.125.1.0/24");

            db1.RootGroup.AddEntry(exactEntry, true);
            db2.RootGroup.AddEntry(cidrEntry, true);

            var matches = _search.FindMatchingEntries(new Uri("https://10.125.1.8/"), null, new[] { db1, db2 }).ToList();
            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual("Db1 Exact", matches[0].entry.Strings.ReadSafe(PwDefs.TitleField));

            var count = _search.CountMatchingEntries("https://10.125.1.8/", new[] { db1, db2 });
            Assert.AreEqual(1, count);
        }
    }
}
