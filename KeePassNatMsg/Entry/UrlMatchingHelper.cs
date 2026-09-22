using System;
using System.Collections.Generic;
using System.Linq;

namespace KeePassNatMsg.Entry
{
    public static class UrlMatchingHelper
    {
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

        public static int GetBestUrlDistance(string requestUrl, IEnumerable<string> candidateUrls)
        {
            var request = (requestUrl ?? string.Empty).ToLowerInvariant();
            var candidates = (candidateUrls ?? Enumerable.Empty<string>())
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => x.ToLowerInvariant())
                .ToList();

            if (candidates.Count == 0) return request.Length;
            return candidates.Min(x => LevenshteinDistance(request, x));
        }

        private static int LevenshteinDistance(string source, string target)
        {
            if (string.IsNullOrEmpty(source)) return string.IsNullOrEmpty(target) ? 0 : target.Length;
            if (string.IsNullOrEmpty(target)) return source.Length;

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
                    var cost = target[j - 1] == source[i - 1] ? 0 : 1;
                    distance[currentRow, j] = Math.Min(
                        Math.Min(distance[previousRow, j] + 1, distance[currentRow, j - 1] + 1),
                        distance[previousRow, j - 1] + cost);
                }
            }

            return distance[currentRow, m];
        }
    }
}
