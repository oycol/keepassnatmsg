using System;
using System.Collections.Generic;
using System.Linq;

namespace KeePassNatMsg.Entry
{
    public static class UrlMatchingHelper
    {
        public static readonly string[] DefaultAllowedSchemes = new[] { "https", "http" };

        public static bool IsAdditionalUrlField(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName)) return false;
            return fieldName.StartsWith("URL", StringComparison.InvariantCultureIgnoreCase) ||
                   fieldName.StartsWith("KP2A_URL_", StringComparison.InvariantCultureIgnoreCase);
        }

        public static IList<string> ParseUrlValues(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return new List<string>();

            return value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();
        }

        public static IEnumerable<string> GetSearchHosts(string host)
        {
            if (string.IsNullOrWhiteSpace(host)) yield break;

            var current = host.Trim().TrimEnd('.');
            yield return current;

            while (true)
            {
                var dotIndex = current.IndexOf('.');
                if (dotIndex < 0) break;

                current = current.Substring(dotIndex + 1);
                if (string.IsNullOrEmpty(current) || current.IndexOf('.') < 0)
                {
                    // Stop at the registrable top boundary to avoid searching naked TLDs like "com" or "uk"
                    break;
                }

                yield return current;
            }
        }

        public static bool MatchesUrl(string entryUrl, string requestHost, string requestScheme = null, bool matchSchemes = false, bool exactHostOnly = false)
        {
            if (string.IsNullOrWhiteSpace(entryUrl) || string.IsNullOrWhiteSpace(requestHost))
                return false;

            var normalizedUrl = entryUrl.Trim();
            if (!normalizedUrl.Contains("://"))
            {
                normalizedUrl = "https://" + normalizedUrl;
            }

            Uri uri;
            if (!Uri.TryCreate(normalizedUrl, UriKind.Absolute, out uri))
                return false;

            if (!DefaultAllowedSchemes.Contains(uri.Scheme.ToLowerInvariant()))
                return false;

            if (matchSchemes && !string.IsNullOrWhiteSpace(requestScheme))
            {
                if (!string.Equals(uri.Scheme, requestScheme, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            var cleanEntryHost = (uri.Host ?? string.Empty).Trim().TrimEnd('.');
            var cleanRequestHost = requestHost.Trim().TrimEnd('.');

            if (string.Equals(cleanEntryHost, cleanRequestHost, StringComparison.OrdinalIgnoreCase))
                return true;

            if (exactHostOnly)
                return false;

            // Security rule: a parent domain entry (e.g. example.com) may match a subdomain request (e.g. login.example.com).
            // A child subdomain entry MUST NOT match a parent domain request.
            if (cleanRequestHost.EndsWith("." + cleanEntryHost, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }
    }
}
