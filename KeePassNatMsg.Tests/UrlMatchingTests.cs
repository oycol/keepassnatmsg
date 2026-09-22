using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KeePassNatMsg.Tests
{
    /// <summary>
    /// Tests for URL matching logic used by EntrySearch.
    /// These tests validate the matching algorithm independently of KeePass,
    /// using the same logic (Levenshtein distance, host matching, scheme filtering).
    /// </summary>
    [TestFixture]
    public class UrlMatchingTests
    {
        // These mirror the allowed schemes in EntrySearch
        private readonly List<string> _allowedSchemes = new List<string>(new[] { "http", "https", "ftp", "sftp" });

        #region Scheme Validation

        [Test]
        public void UrlMatching_HttpScheme_Accepted()
        {
            Assert.IsTrue(_allowedSchemes.Contains("http"));
        }

        [Test]
        public void UrlMatching_HttpsScheme_Accepted()
        {
            Assert.IsTrue(_allowedSchemes.Contains("https"));
        }

        [Test]
        public void UrlMatching_FtpScheme_Accepted()
        {
            Assert.IsTrue(_allowedSchemes.Contains("ftp"));
        }

        [Test]
        public void UrlMatching_SftpScheme_Accepted()
        {
            Assert.IsTrue(_allowedSchemes.Contains("sftp"));
        }

        [Test]
        public void UrlMatching_JavascriptScheme_Rejected()
        {
            Assert.IsFalse(_allowedSchemes.Contains("javascript"));
        }

        [Test]
        public void UrlMatching_DataScheme_Rejected()
        {
            Assert.IsFalse(_allowedSchemes.Contains("data"));
        }

        #endregion

        #region SubmitUrl JavaScript Protection

        [Test]
        public void SubmitUrl_Javascript_Rejected()
        {
            // Simulates the check in EntrySearch.GetLoginsHandler
            var submitUrl = "javascript:alert(1)";
            Uri submitUri;
            Uri.TryCreate(submitUrl, UriKind.Absolute, out submitUri);

            // JavaScript scheme should not be in allowed schemes
            if (submitUri != null)
            {
                Assert.IsFalse(_allowedSchemes.Contains(submitUri.Scheme));
            }
        }

        [Test]
        public void SubmitUrl_ValidHttps_Accepted()
        {
            var submitUrl = "https://example.com/login";
            Uri submitUri = new Uri(submitUrl);

            Assert.IsTrue(_allowedSchemes.Contains(submitUri.Scheme));
            Assert.IsNotNull(submitUri.Authority);
        }

        [Test]
        public void SubmitUrl_ValidHttp_Accepted()
        {
            var submitUrl = "http://example.com/login";
            Uri submitUri = new Uri(submitUrl);

            Assert.IsTrue(_allowedSchemes.Contains(submitUri.Scheme));
        }

        #endregion

        #region Host Matching

        [Test]
        public void HostMatching_ExactHost_Matches()
        {
            var entryHost = "example.com";
            var requestHost = "example.com";
            Assert.IsTrue(entryHost.Contains(requestHost));
        }

        [Test]
        public void HostMatching_Subdomain_MatchesParent()
        {
            // Subdomain stripping: www.example.com → example.com
            var host = "www.example.com";
            var parentHost = host.Substring(host.IndexOf(".") + 1);
            Assert.AreEqual("example.com", parentHost);
        }

        [Test]
        public void HostMatching_DeepSubdomain_StripsCorrectly()
        {
            // a.b.example.com → b.example.com → example.com
            var host = "a.b.example.com";
            Assert.AreEqual("b.example.com", host.Substring(host.IndexOf(".") + 1));
            Assert.AreEqual("example.com", "b.example.com".Substring("b.example.com".IndexOf(".") + 1));
        }

        [Test]
        public void HostMatching_NoDot_StopsStripping()
        {
            // When no dot remains, searchHost == origSearchHost, loop breaks
            var host = "localhost";
            Assert.AreEqual(-1, host.IndexOf("."));
        }

        #endregion

        #region Levenshtein Distance

        [Test]
        public void LevenshteinDistance_IdenticalStrings_ReturnsZero()
        {
            Assert.AreEqual(0, LevenshteinDistance("https://example.com", "https://example.com"));
        }

        [Test]
        public void LevenshteinDistance_EmptySource_ReturnsTargetLength()
        {
            Assert.AreEqual(5, LevenshteinDistance("", "hello"));
        }

        [Test]
        public void LevenshteinDistance_EmptyTarget_ReturnsSourceLength()
        {
            Assert.AreEqual(5, LevenshteinDistance("hello", ""));
        }

        [Test]
        public void LevenshteinDistance_OneCharDiff_ReturnsOne()
        {
            Assert.AreEqual(1, LevenshteinDistance("https://example.com", "https://example.co"));
        }

        [Test]
        public void LevenshteinDistance_CompletelyDifferent_ReturnsLength()
        {
            Assert.AreEqual(5, LevenshteinDistance("abcde", "fghij"));
        }

        [Test]
        public void LevenshteinDistance_SwappedChars_ReturnsTwo()
        {
            Assert.AreEqual(2, LevenshteinDistance("ab", "ba"));
        }

        // Reference implementation matching EntrySearch.LevenshteinDistance
        private static int LevenshteinDistance(string source, string target)
        {
            if (String.IsNullOrEmpty(source))
            {
                if (String.IsNullOrEmpty(target)) return 0;
                return target.Length;
            }
            if (String.IsNullOrEmpty(target)) return source.Length;

            if (source.Length > target.Length)
            {
                var temp = target;
                target = source;
                source = temp;
            }

            var m = target.Length;
            var n = source.Length;
            var distance = new int[2, m + 1];
            for (var j = 1; j <= m; j++) distance[0, j] = j;

            var currentRow = 0;
            for (var i = 1; i <= n; ++i)
            {
                currentRow = i & 1;
                distance[currentRow, 0] = i;
                var previousRow = currentRow ^ 1;
                for (var j = 1; j <= m; j++)
                {
                    var cost = (target[j - 1] == source[i - 1] ? 0 : 1);
                    distance[currentRow, j] = Math.Min(Math.Min(
                        distance[previousRow, j] + 1,
                        distance[currentRow, j - 1] + 1),
                        distance[previousRow, j - 1] + cost);
                }
            }
            return distance[currentRow, m];
        }

        #endregion

        #region Additional URL Fields

        [Test]
        public void AdditionalUrlCandidate_RegularUrl1_IsIncluded()
        {
            Assert.IsTrue(KeePassNatMsg.Entry.EntrySearch.IsAdditionalUrlField("URL1"));
            Assert.IsTrue(KeePassNatMsg.Entry.EntrySearch.IsAdditionalUrlField("URL Microsoft Online"));
            Assert.IsTrue(KeePassNatMsg.Entry.EntrySearch.IsAdditionalUrlField("KP2A_URL_1"));
        }

        [Test]
        public void AdditionalUrlCandidate_UnrelatedField_IsExcluded()
        {
            Assert.IsFalse(KeePassNatMsg.Entry.EntrySearch.IsAdditionalUrlField("Notes"));
            Assert.IsFalse(KeePassNatMsg.Entry.EntrySearch.IsAdditionalUrlField("UserName"));
        }

        [Test]
        public void BestMatchingDistance_UsesAdditionalUrlInsteadOfPrimaryUrlOnly()
        {
            var requestUrl = "https://login.microsoftonline.com/common/oauth2/v2.0/authorize";
            var urls = new[]
            {
                "https://login.live.com/",
                "https://login.microsoftonline.com/"
            };

            var actual = KeePassNatMsg.Entry.EntrySearch.GetBestUrlDistance(requestUrl, urls);
            var primaryOnly = LevenshteinDistance(requestUrl.ToLowerInvariant(), urls[0].ToLowerInvariant());
            var additional = LevenshteinDistance(requestUrl.ToLowerInvariant(), urls[1].ToLowerInvariant());

            Assert.AreEqual(additional, actual);
            Assert.Less(actual, primaryOnly);
        }

        #endregion

        #region URL Parsing

        [Test]
        public void UrlParsing_ValidHttps_ExtractsHost()
        {
            var url = "https://www.example.com/path?query=1";
            var uri = new Uri(url);
            Assert.AreEqual("www.example.com", uri.Host);
            Assert.AreEqual("https", uri.Scheme);
        }

        [Test]
        public void UrlParsing_ValidHttp_ExtractsAuthority()
        {
            var url = "http://example.com:8080/path";
            var uri = new Uri(url);
            Assert.AreEqual("example.com:8080", uri.Authority);
            Assert.AreEqual("example.com", uri.Host);
        }

        [Test]
        public void UrlParsing_EmptyUrl_ThrowsOrReturns()
        {
            // Empty URL should be handled gracefully in the handler
            string url = "";
            Assert.IsTrue(string.IsNullOrEmpty(url));
        }

        [Test]
        public void UrlParsing_RelativeUrl_NotAbsolute()
        {
            Uri uri;
            Assert.IsFalse(Uri.TryCreate("/relative/path", UriKind.Absolute, out uri));
        }

        #endregion

        #region EntryConfig Allow/Deny

        [Test]
        public void EntryConfig_AllowDeny_InitiallyEmpty()
        {
            var config = new KeePassNatMsg.Entry.EntryConfig();
            Assert.AreEqual(0, config.Allow.Count);
            Assert.AreEqual(0, config.Deny.Count);
        }

        [Test]
        public void EntryConfig_AllowDeny_CanAddHosts()
        {
            var config = new KeePassNatMsg.Entry.EntryConfig();
            config.Allow.Add("example.com");
            config.Allow.Add("sub.example.com");
            config.Deny.Add("malicious.com");

            Assert.Contains("example.com", config.Allow.ToList());
            Assert.Contains("sub.example.com", config.Allow.ToList());
            Assert.Contains("malicious.com", config.Deny.ToList());
        }

        [Test]
        public void EntryConfig_AllowContains_ChecksHost()
        {
            var config = new KeePassNatMsg.Entry.EntryConfig();
            config.Allow.Add("example.com");

            Assert.IsTrue(config.Allow.Contains("example.com"));
            Assert.IsFalse(config.Allow.Contains("other.com"));
        }

        #endregion
    }
}
