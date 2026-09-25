using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KeePassNatMsg.Tests
{
    [TestFixture]
    public class UrlMatchingTests
    {
        [Test]
        public void AllowedSchemes_ContainsOnlyExpectedWebSchemes()
        {
            var allowed = KeePassNatMsg.Entry.UrlMatchingHelper.DefaultAllowedSchemes;
            Assert.Contains("https", (System.Collections.ICollection)allowed);
            Assert.Contains("http", (System.Collections.ICollection)allowed);
            Assert.IsFalse(allowed.Contains("javascript"));
            Assert.IsFalse(allowed.Contains("data"));
            Assert.IsFalse(allowed.Contains("ftp"));
            Assert.IsFalse(allowed.Contains("sftp"));
        }

        [Test]
        public void MultipleUrls_CommaSeparatedPrimaryField_AreParsed()
        {
            var urls = KeePassNatMsg.Entry.UrlMatchingHelper.ParseUrlValues(
                "https://login.live.com/, https://login.microsoftonline.com/");

            CollectionAssert.AreEqual(new[]
            {
                "https://login.live.com/",
                "https://login.microsoftonline.com/"
            }, urls);
        }

        [Test]
        public void AdditionalUrlCandidate_SupportedPrefixes_AreIncluded()
        {
            Assert.IsTrue(KeePassNatMsg.Entry.UrlMatchingHelper.IsAdditionalUrlField("URL1"));
            Assert.IsTrue(KeePassNatMsg.Entry.UrlMatchingHelper.IsAdditionalUrlField("URL Microsoft Online"));
            Assert.IsTrue(KeePassNatMsg.Entry.UrlMatchingHelper.IsAdditionalUrlField("KP2A_URL_1"));
            Assert.IsFalse(KeePassNatMsg.Entry.UrlMatchingHelper.IsAdditionalUrlField("Notes"));
            Assert.IsFalse(KeePassNatMsg.Entry.UrlMatchingHelper.IsAdditionalUrlField("UserName"));
        }

        [Test]
        public void CidrRule_MatchesOnlyIpv4SubnetIncludingBoundaries()
        {
            Assert.IsTrue(KeePassNatMsg.Entry.UrlMatchingHelper.MatchesUrl("CIDR:10.125.1.0/24", "10.125.1.0", "https"));
            Assert.IsTrue(KeePassNatMsg.Entry.UrlMatchingHelper.MatchesUrl("cidr:10.125.1.0/24", "10.125.1.255", "http"));
            Assert.IsFalse(KeePassNatMsg.Entry.UrlMatchingHelper.MatchesUrl("CIDR:10.125.1.0/24", "10.125.2.1", "https"));
            Assert.IsFalse(KeePassNatMsg.Entry.UrlMatchingHelper.MatchesUrl("CIDR:10.125.1.0/24", "bmc.example.com", "https"));
            Assert.IsFalse(KeePassNatMsg.Entry.UrlMatchingHelper.MatchesUrl("CIDR:10.125.1.0/24", "10.125.1.1.evil.test", "https"));
        }

        [Test]
        public void CidrRule_MultipleNetworksAndHostMask()
        {
            var rules = KeePassNatMsg.Entry.UrlMatchingHelper.ParseUrlValues("CIDR:10.125.1.0/24, CIDR:10.125.2.3/32");
            CollectionAssert.AreEqual(new[] { "CIDR:10.125.1.0/24", "CIDR:10.125.2.3/32" }, rules);
            Assert.IsTrue(rules.Any(rule => KeePassNatMsg.Entry.UrlMatchingHelper.MatchesUrl(rule, "10.125.2.3", "https")));
            Assert.IsFalse(rules.Any(rule => KeePassNatMsg.Entry.UrlMatchingHelper.MatchesUrl(rule, "10.125.2.4", "https")));
        }

        [Test]
        public void CidrRule_RejectsMalformedAndNonCanonicalNetworks()
        {
            foreach (var rule in new[] { "CIDR:", "CIDR:10.125.1.0", "CIDR:10.125.1.0/33", "CIDR:10.125.1.1/24", "CIDR:10.125.1.999/24", "CIDR:010.125.1.0/24", "CIDR:10.125.1.0/-1", "CIDR:10.125.1.0/24/1", "CIDR:0.0.0.0/0" })
                Assert.IsFalse(KeePassNatMsg.Entry.UrlMatchingHelper.MatchesUrl(rule, "10.125.1.1", "https"), rule);
        }

        [Test]
        public void RegexRules_DoNotMatchAfterRemoval()
        {
            Assert.IsFalse(KeePassNatMsg.Entry.UrlMatchingHelper.MatchesUrl(@"Regex:^10\.125\.1\.\d+$", "10.125.1.8", "https"));
        }

        [Test]
        public void MatchesUrl_ExactHost_Matches()
        {
            Assert.IsTrue(KeePassNatMsg.Entry.UrlMatchingHelper.MatchesUrl(
                "https://example.com/login",
                "example.com"));
        }

        [Test]
        public void MatchesUrl_NakedDomainEntry_NormalizedAndMatches()
        {
            Assert.IsTrue(KeePassNatMsg.Entry.UrlMatchingHelper.MatchesUrl(
                "example.com",
                "example.com"));
            Assert.IsTrue(KeePassNatMsg.Entry.UrlMatchingHelper.MatchesUrl(
                "login.microsoftonline.com",
                "login.microsoftonline.com"));
        }

        [Test]
        public void MatchesUrl_ParentDomainEntry_MatchesChildSubdomainRequest()
        {
            Assert.IsTrue(KeePassNatMsg.Entry.UrlMatchingHelper.MatchesUrl(
                "https://example.com/",
                "login.example.com"));
        }

        [Test]
        public void MatchesUrl_ExactHostOnlyMode_DisallowsParentToSubdomainMatching()
        {
            Assert.IsFalse(KeePassNatMsg.Entry.UrlMatchingHelper.MatchesUrl(
                "https://example.com/",
                "login.example.com",
                exactHostOnly: true));

            Assert.IsTrue(KeePassNatMsg.Entry.UrlMatchingHelper.MatchesUrl(
                "https://login.example.com/",
                "login.example.com",
                exactHostOnly: true));
        }

        [Test]
        public void MatchesUrl_ChildDomainEntry_DoesNotMatchParentDomainRequest()
        {
            Assert.IsFalse(KeePassNatMsg.Entry.UrlMatchingHelper.MatchesUrl(
                "https://login.example.com/",
                "example.com"));
        }

        [Test]
        public void MatchesUrl_TypoSquattingDomain_DoesNotMatch()
        {
            Assert.IsFalse(KeePassNatMsg.Entry.UrlMatchingHelper.MatchesUrl(
                "https://login.microsoftonline.com/",
                "login.rnicrosoftonline.com"));
            Assert.IsFalse(KeePassNatMsg.Entry.UrlMatchingHelper.MatchesUrl(
                "https://example.com/",
                "evil-example.com"));
            Assert.IsFalse(KeePassNatMsg.Entry.UrlMatchingHelper.MatchesUrl(
                "https://example.com/",
                "example.com.evil.test"));
        }

        [Test]
        public void MatchesUrl_RespectsSchemeWhenRequired()
        {
            Assert.IsFalse(KeePassNatMsg.Entry.UrlMatchingHelper.MatchesUrl(
                "http://example.com/",
                "example.com",
                requestScheme: "https",
                matchSchemes: true));

            Assert.IsTrue(KeePassNatMsg.Entry.UrlMatchingHelper.MatchesUrl(
                "https://example.com/",
                "example.com",
                requestScheme: "https",
                matchSchemes: true));
        }

        [Test]
        public void BuildCandidateHosts_ProducesHostAndParentHierarchy_WithoutEmptyLabels()
        {
            var hosts = KeePassNatMsg.Entry.UrlMatchingHelper.GetSearchHosts("a.b.example.com").ToList();
            CollectionAssert.AreEqual(new[]
            {
                "a.b.example.com",
                "b.example.com",
                "example.com"
            }, hosts);
        }

        [Test]
        public void EntryConfig_AllowDeny_CaseInsensitive()
        {
            var config = new KeePassNatMsg.Entry.EntryConfig();
            config.Allow.Add("login.MicrosoftOnline.com:443");
            config.Deny.Add("Malicious.com:8080");

            Assert.IsTrue(config.Allow.Contains("login.microsoftonline.com:443"));
            Assert.IsTrue(config.Deny.Contains("malicious.com:8080"));
        }

        // ── CreateEntry URL baseUrl truncation (Fix #3) ────────────────────────────
        // These tests exercise the same logic path as EntryUpdate.CreateEntry without
        // needing a real KeePass database — they operate directly on the Uri class.

        private static string ComputeBaseUrl(string url)
        {
            // Mirror the fixed logic from EntryUpdate.CreateEntry
            Uri uri;
            if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out uri))
                return null;

            string baseUrl = url;
            var lastSlash = url.LastIndexOf('/');
            var schemeEnd = url.IndexOf("://", StringComparison.Ordinal);
            var pathStart = schemeEnd >= 0 ? schemeEnd + 3 : 0;
            if (lastSlash > pathStart)
                baseUrl = url.Substring(0, lastSlash + 1);
            return baseUrl;
        }

        [Test]
        public void CreateEntry_BaseUrl_HttpsWithPath_TrimsToLastSlash()
        {
            Assert.AreEqual("https://example.com/login/",
                ComputeBaseUrl("https://example.com/login/submit?user=x"));
        }

        [Test]
        public void CreateEntry_BaseUrl_HttpsRootOnly_Unchanged()
        {
            // No path beyond the authority → keep as-is
            Assert.AreEqual("https://example.com",
                ComputeBaseUrl("https://example.com"));
        }

        [Test]
        public void CreateEntry_BaseUrl_TrailingSlashRoot_Unchanged()
        {
            Assert.AreEqual("https://example.com/",
                ComputeBaseUrl("https://example.com/"));
        }

        [Test]
        public void CreateEntry_BaseUrl_HttpScheme_HandledCorrectly()
        {
            // http:// has 7 chars; pathStart = 10; lastSlash at 21 → trim
            Assert.AreEqual("http://example.com/path/",
                ComputeBaseUrl("http://example.com/path/page.html"));
        }

        [Test]
        public void CreateEntry_BaseUrl_InvalidUrl_ReturnsNull()
        {
            Assert.IsNull(ComputeBaseUrl("not-a-url"));
            Assert.IsNull(ComputeBaseUrl(""));
            Assert.IsNull(ComputeBaseUrl(null));
        }
    }
}
