using System;
using System.Collections.Generic;
using System.Linq;

namespace KeePassNatMsg.Entry
{
    public static class UrlMatchingHelper
    {
        public static readonly string[] DefaultAllowedSchemes = new[] { "https", "http" };
        private const string CidrPrefix = "CIDR:";

        // Only explicit, canonical IPv4 networks are accepted. No DNS resolution or IPv6 coercion.
        private static bool TryParseIpv4(string value, out uint address)
        {
            address = 0;
            if (string.IsNullOrEmpty(value)) return false;
            string[] parts = value.Split('.');
            if (parts.Length != 4) return false;
            foreach (string part in parts)
            {
                if (part.Length < 1 || part.Length > 3 || (part.Length > 1 && part[0] == '0')) return false;
                int octet = 0;
                foreach (char c in part)
                {
                    if (c < '0' || c > '9') return false;
                    octet = octet * 10 + (c - '0');
                }
                if (octet > 255) return false;
                address = (address << 8) | (uint)octet;
            }
            return true;
        }

        private static bool TryParseCidr(string value, out uint network, out uint mask)
        {
            network = 0;
            mask = 0;
            if (string.IsNullOrWhiteSpace(value) ||
                !value.Trim().StartsWith(CidrPrefix, StringComparison.OrdinalIgnoreCase)) return false;
            string[] parts = value.Trim().Substring(CidrPrefix.Length).Split('/');
            if (parts.Length != 2 || !TryParseIpv4(parts[0], out network)) return false;
            string prefix = parts[1];
            if (prefix.Length < 1 || prefix.Length > 2) return false;
            int bits = 0;
            foreach (char c in prefix)
            {
                if (c < '0' || c > '9') return false;
                bits = bits * 10 + (c - '0');
            }
            // A global /0 rule would return a credential for every IPv4 website.
            if (bits < 1 || bits > 32) return false;
            mask = uint.MaxValue << (32 - bits);
            return (network & mask) == network;
        }

        public static bool IsNetworkRuleCandidate(string value)
        {
            foreach (string rule in ParseUrlValues(value))
            {
                uint network, mask;
                if (TryParseCidr(rule, out network, out mask)) return true;
            }
            return false;
        }

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

            // CIDR is an explicit IPv4 host rule; never resolve DNS or match a substring.
            if (normalizedUrl.StartsWith(CidrPrefix, StringComparison.OrdinalIgnoreCase))
            {
                uint network, mask, host;
                return TryParseCidr(normalizedUrl, out network, out mask) &&
                       TryParseIpv4(requestHost, out host) &&
                       (host & mask) == network;
            }
            // Retired Regex rules must not become ordinary URL matches.
            if (normalizedUrl.StartsWith("Regex:", StringComparison.OrdinalIgnoreCase)) return false;
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
