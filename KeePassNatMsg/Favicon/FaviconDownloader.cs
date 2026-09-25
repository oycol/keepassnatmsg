using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace KeePassNatMsg.Favicon
{
    public sealed class FaviconDownloader : IDisposable
    {
        private const string DefaultUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";
        private const int RequestTimeoutMs = 5000;

        private readonly CookieContainer _cookies = new CookieContainer();
        private IWebProxy _proxy;
        private readonly object _reqLock = new object();
        private HttpWebRequest _activeRequest;
        private volatile bool _isAborted;

        public bool IsAborted { get { return _isAborted; } }

        public void Abort()
        {
            _isAborted = true;
            lock (_reqLock)
            {
                if (_activeRequest != null)
                {
                    try { _activeRequest.Abort(); } catch { }
                }
            }
        }

        private static readonly Regex DataSchemaRegex = new Regex(@"data:(?<mediatype>.*?)(;(?<base64>.+?))?,(?<data>.+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex HttpSchemaRegex = new Regex(@"^http(s)?://", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex HeadTagRegex = new Regex(@"<head\b[^>]*>(?<content>.*?)</head>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex BaseTagRegex = new Regex(@"<base\b[^>]*?href\s*=\s*(?:""(?<url>[^""]*)""|'(?<url>[^']*)'|(?<url>[^\s>]+))", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex LinkTagRegex = new Regex(@"<link\b[^>]*>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex RelAttrRegex = new Regex(@"\brel\s*=\s*(?:""(?<rel>[^""]*)""|'(?<rel>[^']*)'|(?<rel>[^\s>]+))", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex HrefAttrRegex = new Regex(@"\bhref\s*=\s*(?:""(?<href>[^""]*)""|'(?<href>[^']*)'|(?<href>[^\s>]+))", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

        static FaviconDownloader()
        {
            try
            {
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            }
            catch { }
        }

        public FaviconDownloader(IWebProxy proxy = null)
        {
            _proxy = proxy ?? WebRequest.DefaultWebProxy;
        }

        public byte[] DownloadFaviconDirect(string rawUrl, bool autoPrefix, int maxIconSize)
        {
            if (string.IsNullOrEmpty(rawUrl))
                throw new FaviconDownloaderException(FaviconErrorStatus.NotFound);

            string targetUrl = rawUrl.Trim();
            if (!HttpSchemaRegex.IsMatch(targetUrl))
            {
                if (autoPrefix)
                {
                    targetUrl = "https://" + targetUrl;
                }
                else
                {
                    targetUrl = "http://" + targetUrl;
                }
            }

            int attempts = 0;
            string currentUrl = targetUrl;
            bool triedHttpFallback = false;

        retry_entry:
            attempts++;
            if (attempts > 2)
                throw new FaviconDownloaderException(FaviconErrorStatus.NotFound);

            Uri baseUri;
            if (!Uri.TryCreate(currentUrl, UriKind.Absolute, out baseUri))
                throw new FaviconDownloaderException(FaviconErrorStatus.NotFound);

            // SSRF guard: block private/loopback/link-local targets for the page fetch
            if (IsPrivateAddress(baseUri.Host))
                throw new FaviconDownloaderException(FaviconErrorStatus.NotFound);

            try
            {
                string html;
                Uri responseUri;
                bool pageLoaded = TryFetchPage(baseUri, out html, out responseUri);

                List<string> candidates = new List<string>();

                if (pageLoaded && !string.IsNullOrEmpty(html))
                {
                    string baseUrl;
                    List<string> hrefs = ExtractFaviconHrefsFromHtml(html, out baseUrl);

                    // Resolve the page-provided <base href> (if any) for relative link resolution.
                    Uri baseForResolve = responseUri ?? baseUri;
                    if (!string.IsNullOrEmpty(baseUrl))
                    {
                        Uri baseUrlParsed;
                        if (Uri.TryCreate(baseUrl, UriKind.Absolute, out baseUrlParsed))
                        {
                            baseForResolve = baseUrlParsed;
                        }
                        else if (Uri.TryCreate(responseUri ?? baseUri, baseUrl, out baseUrlParsed))
                        {
                            baseForResolve = baseUrlParsed;
                        }
                    }

                    for (int i = 0; i < hrefs.Count; i++)
                    {
                        string href = hrefs[i];
                        if (string.IsNullOrEmpty(href)) continue;

                        // data: URIs cannot be resolved via Uri.TryCreate; keep the raw URI for inline decoding.
                        if (href.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!candidates.Contains(href))
                            {
                                candidates.Add(href);
                            }
                            continue;
                        }

                        Uri parsed;
                        if (Uri.TryCreate(baseForResolve, href, out parsed))
                        {
                            string abs = parsed.AbsoluteUri;
                            if (!candidates.Contains(abs))
                            {
                                candidates.Add(abs);
                            }
                        }
                    }
                }

                // Direct fallback: standard root /favicon.ico
                Uri rootFavicon;
                if (Uri.TryCreate(new Uri(baseUri.GetLeftPart(UriPartial.Authority)), "/favicon.ico", out rootFavicon))
                {
                    string rootUrl = rootFavicon.AbsoluteUri;
                    if (!candidates.Contains(rootUrl))
                    {
                        candidates.Add(rootUrl);
                    }
                }

                for (int i = 0; i < candidates.Count; i++)
                {
                    string candidate = candidates[i];

                    byte[] rawData;
                    if (candidate.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                    {
                        // Decode data: URI inline instead of performing an HTTP download.
                        if (!TryDecodeDataUri(candidate, out rawData)) continue;
                    }
                    else
                    {
                        Uri candidateUri;
                        if (!Uri.TryCreate(candidate, UriKind.Absolute, out candidateUri)) continue;

                        // SSRF guard: skip candidate URLs resolving to private/loopback addresses
                        if (IsPrivateAddress(candidateUri.Host)) continue;

                        rawData = TryDownloadAsset(candidateUri);
                    }

                    if (rawData != null && rawData.Length > 0)
                    {
                        byte[] resized;
                        if (TryProcessAndResizeImage(rawData, maxIconSize, out resized))
                        {
                            return resized;
                        }
                    }
                }

                // If path had subdirectories, try host root
                if (baseUri.AbsolutePath != "/" && baseUri.AbsolutePath != "")
                {
                    currentUrl = baseUri.GetLeftPart(UriPartial.Authority) + "/";
                    goto retry_entry;
                }
            }
            catch (Exception ex)
            {
                if (ex is FaviconDownloaderException) throw;

                // Fallback from HTTPS to HTTP if autoPrefix was used
                if (autoPrefix && !triedHttpFallback && currentUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    triedHttpFallback = true;
                    currentUrl = "http://" + currentUrl.Substring(8);
                    goto retry_entry;
                }

                throw new FaviconDownloaderException(FaviconErrorStatus.NetworkError, ex);
            }

            if (autoPrefix && !triedHttpFallback && currentUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                triedHttpFallback = true;
                currentUrl = "http://" + currentUrl.Substring(8);
                goto retry_entry;
            }

            throw new FaviconDownloaderException(FaviconErrorStatus.NotFound);
        }

        public byte[] DownloadFaviconFromProvider(string providerTemplate, string rawUrl, int maxIconSize)
        {
            if (string.IsNullOrEmpty(rawUrl) || string.IsNullOrEmpty(providerTemplate))
                throw new FaviconDownloaderException(FaviconErrorStatus.NotFound);

            string host = ExtractHostname(rawUrl);
            if (string.IsNullOrEmpty(host))
                throw new FaviconDownloaderException(FaviconErrorStatus.NotFound);

            string requestUrl = FaviconProvider.BuildProviderUrl(providerTemplate, host, maxIconSize);
            Uri providerUri;
            if (!Uri.TryCreate(requestUrl, UriKind.Absolute, out providerUri))
                throw new FaviconDownloaderException(FaviconErrorStatus.NotFound);

            // SSRF guard: block private/loopback/link-local provider targets
            if (IsPrivateAddress(providerUri.Host))
                throw new FaviconDownloaderException(FaviconErrorStatus.NotFound);

            byte[] data = TryDownloadAsset(providerUri);
            if (data == null || data.Length == 0)
                throw new FaviconDownloaderException(FaviconErrorStatus.NotFound);

            byte[] resized;
            if (TryProcessAndResizeImage(data, maxIconSize, out resized))
            {
                return resized;
            }

            throw new FaviconDownloaderException(FaviconErrorStatus.InvalidData);
        }

        public static string ExtractHostname(string url)
        {
            if (string.IsNullOrEmpty(url)) return string.Empty;
            string target = url.Trim();
            if (!HttpSchemaRegex.IsMatch(target))
            {
                target = "http://" + target;
            }

            Uri uri;
            if (Uri.TryCreate(target, UriKind.Absolute, out uri))
            {
                return uri.Host;
            }
            return string.Empty;
        }

        private static bool IsPrivateAddress(string host)
        {
            if (string.IsNullOrEmpty(host)) return false;
            string h = host.Trim().ToLowerInvariant();

            // IPv6 loopback
            if (h == "::1") return true;
            // IPv6 link-local
            if (h.StartsWith("fe80:", StringComparison.Ordinal)) return true;
            // IPv6 unique-local (fc00::/7)
            if (h.StartsWith("fc", StringComparison.Ordinal) || h.StartsWith("fd", StringComparison.Ordinal)) return true;

            // Strip IPv6 brackets and IPv6 zone
            if (h.StartsWith("[", StringComparison.Ordinal) && h.EndsWith("]", StringComparison.Ordinal))
            {
                h = h.Substring(1, h.Length - 2);
            }
            int zoneIdx = h.LastIndexOf('%');
            if (zoneIdx >= 0) h = h.Substring(0, zoneIdx);

            // Normalize IPv6 loopback variants
            if (h == "0:0:0:0:0:0:0:1") return true;

            // IPv4 literal checks
            // 127.x.x.x
            if (h.StartsWith("127.", StringComparison.Ordinal)) return true;
            // 169.254.x.x (link-local)
            if (h.StartsWith("169.254.", StringComparison.Ordinal)) return true;
            // 10.x.x.x
            if (h.StartsWith("10.", StringComparison.Ordinal)) return true;
            // 192.168.x.x
            if (h.StartsWith("192.168.", StringComparison.Ordinal)) return true;
            // 172.16.x.x - 172.31.x.x
            if (h.StartsWith("172.", StringComparison.Ordinal))
            {
                string rest = h.Substring(4);
                int dot = rest.IndexOf('.');
                if (dot > 0)
                {
                    string second = rest.Substring(0, dot);
                    int secondOctet;
                    if (int.TryParse(second, out secondOctet) && secondOctet >= 16 && secondOctet <= 31)
                    {
                        return true;
                    }
                }
            }

            // localhost
            if (h == "localhost") return true;

            return false;
        }

        private static bool TryDecodeDataUri(string dataUri, out byte[] data)
        {
            data = null;
            if (string.IsNullOrEmpty(dataUri)) return false;
            Match m = DataSchemaRegex.Match(dataUri);
            if (!m.Success) return false;

            string mediaType = m.Groups["mediatype"].Value;
            string base64Flag = m.Groups["base64"].Success ? m.Groups["base64"].Value : string.Empty;
            string encoded = m.Groups["data"].Value;

            if (string.IsNullOrEmpty(encoded)) return false;

            // Only accept image/* media types, or empty (treated as image).
            if (!string.IsNullOrEmpty(mediaType) &&
                !mediaType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (base64Flag != null && base64Flag.ToLowerInvariant() == "base64")
            {
                try
                {
                    // data: URIs may contain URL-encoded characters in the base64 payload.
                    string cleaned = encoded.Replace("%2F", "/").Replace("%2B", "+").Replace("%3D", "=");
                    data = Convert.FromBase64String(cleaned);
                    return data != null && data.Length > 0;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                // URL-encoded raw bytes (e.g. data:image/png,%89PNG...)
                try
                {
                    data = Encoding.UTF8.GetBytes(Uri.UnescapeDataString(encoded));
                    return data != null && data.Length > 0;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static List<string> ExtractFaviconHrefsFromHtml(string html)
        {
            string baseUrl;
            return ExtractFaviconHrefsFromHtml(html, out baseUrl);
        }

        public static List<string> ExtractFaviconHrefsFromHtml(string html, out string baseUrl)
        {
            baseUrl = null;
            List<string> results = new List<string>();
            if (string.IsNullOrEmpty(html)) return results;

            Match headMatch = HeadTagRegex.Match(html);
            string headContent = headMatch.Success ? headMatch.Groups["content"].Value : html;

            // Strip HTML comments and script/style tags to avoid extracting commented-out or script-generated links
            headContent = Regex.Replace(headContent, @"<!--.*?-->", string.Empty, RegexOptions.Singleline);
            headContent = Regex.Replace(headContent, @"<(script|style)\b[^>]*>.*?</\1>", string.Empty, RegexOptions.Singleline | RegexOptions.IgnoreCase);

            // Check base tag
            Match baseMatch = BaseTagRegex.Match(headContent);
            if (baseMatch.Success)
            {
                baseUrl = baseMatch.Groups["url"].Value;
            }

            MatchCollection links = LinkTagRegex.Matches(headContent);
            foreach (Match link in links)
            {
                string tag = link.Value;
                Match relMatch = RelAttrRegex.Match(tag);
                if (relMatch.Success)
                {
                    string rel = relMatch.Groups["rel"].Value.ToLowerInvariant();
                    bool isIcon = rel.Contains("icon") || rel.Contains("apple-touch-icon");
                    if (isIcon)
                    {
                        Match hrefMatch = HrefAttrRegex.Match(tag);
                        if (hrefMatch.Success)
                        {
                            string href = hrefMatch.Groups["href"].Value.Trim();
                            if (!string.IsNullOrEmpty(href) && !results.Contains(href))
                            {
                                results.Add(href);
                            }
                        }
                    }
                }
            }

            return results;
        }

        public static bool TryProcessAndResizeImage(byte[] rawData, int maxIconSize, out byte[] processed)
        {
            processed = null;
            if (rawData == null || rawData.Length == 0) return false;

            try
            {
                using (MemoryStream ms = new MemoryStream(rawData))
                {
                    using (Image original = Image.FromStream(ms))
                    {
                        int targetWidth = original.Width;
                        int targetHeight = original.Height;

                        if (targetWidth <= 0 || targetHeight <= 0) return false;

                        // Downscale if larger than maxIconSize
                        if (targetWidth > maxIconSize || targetHeight > maxIconSize)
                        {
                            if (targetWidth >= targetHeight)
                            {
                                targetHeight = (int)Math.Round((float)targetHeight * maxIconSize / targetWidth);
                                targetWidth = maxIconSize;
                            }
                            else
                            {
                                targetWidth = (int)Math.Round((float)targetWidth * maxIconSize / targetHeight);
                                targetHeight = maxIconSize;
                            }
                        }

                        if (targetWidth < 16) targetWidth = 16;
                        if (targetHeight < 16) targetHeight = 16;

                        using (Bitmap resizedBmp = new Bitmap(targetWidth, targetHeight, PixelFormat.Format32bppArgb))
                        {
                            using (Graphics g = Graphics.FromImage(resizedBmp))
                            {
                                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                g.SmoothingMode = SmoothingMode.HighQuality;
                                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                                g.CompositingQuality = CompositingQuality.HighQuality;
                                g.Clear(Color.Transparent);

                                g.DrawImage(original, new Rectangle(0, 0, targetWidth, targetHeight));
                            }

                            using (MemoryStream outMs = new MemoryStream())
                            {
                                resizedBmp.Save(outMs, ImageFormat.Png);
                                processed = outMs.ToArray();
                                return true;
                            }
                        }
                    }
                }
            }
            catch
            {
                // Fallback: If image cannot be converted by GDI+ directly (e.g. multi-frame ICO or raw bytes),
                // but is a valid ICO file (type 1), return raw data as-is; GDI+ will handle it.
                // CUR files (type 2) are not accepted.
                if (rawData.Length >= 4 && rawData[0] == 0 && rawData[1] == 0 && rawData[2] == 1)
                {
                    processed = rawData;
                    return true;
                }
                return false;
            }
        }

        private bool TryFetchPage(Uri uri, out string html, out Uri responseUri)
        {
            html = null;
            responseUri = uri;
            if (_isAborted) return false;

            HttpWebRequest req = null;
            try
            {
                req = (HttpWebRequest)WebRequest.Create(uri);
                req.Method = "GET";
                req.UserAgent = DefaultUserAgent;
                req.Timeout = RequestTimeoutMs;
                req.ReadWriteTimeout = RequestTimeoutMs;
                req.CookieContainer = _cookies;
                req.Proxy = _proxy;
                req.AllowAutoRedirect = true;
                req.MaximumAutomaticRedirections = 5;

                lock (_reqLock)
                {
                    if (_isAborted) return false;
                    _activeRequest = req;
                }

                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                {
                    responseUri = resp.ResponseUri;
                    using (Stream stream = resp.GetResponseStream())
                    {
                        if (stream == null) return false;
                        using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                        {
                            html = reader.ReadToEnd();
                            return true;
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
            finally
            {
                lock (_reqLock)
                {
                    if (object.ReferenceEquals(_activeRequest, req))
                    {
                        _activeRequest = null;
                    }
                }
            }
        }

        private byte[] TryDownloadAsset(Uri uri)
        {
            if (_isAborted) return null;
            HttpWebRequest req = null;
            try
            {
                req = (HttpWebRequest)WebRequest.Create(uri);
                req.Method = "GET";
                req.UserAgent = DefaultUserAgent;
                req.Timeout = RequestTimeoutMs;
                req.ReadWriteTimeout = RequestTimeoutMs;
                req.CookieContainer = _cookies;
                req.Proxy = _proxy;
                req.AllowAutoRedirect = true;
                req.MaximumAutomaticRedirections = 5;

                lock (_reqLock)
                {
                    if (_isAborted) return null;
                    _activeRequest = req;
                }

                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                {
                    if (resp.StatusCode != HttpStatusCode.OK) return null;

                    using (Stream stream = resp.GetResponseStream())
                    {
                        if (stream == null) return null;
                        using (MemoryStream ms = new MemoryStream())
                        {
                            byte[] buffer = new byte[8192];
                            int read;
                            int total = 0;
                            const int maxDownloadBytes = 5 * 1024 * 1024; // 5MB guard
                            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                if (_isAborted) return null;
                                total += read;
                                if (total > maxDownloadBytes) return null;
                                ms.Write(buffer, 0, read);
                            }
                            return ms.ToArray();
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
            finally
            {
                lock (_reqLock)
                {
                    if (object.ReferenceEquals(_activeRequest, req))
                    {
                        _activeRequest = null;
                    }
                }
            }
        }

        public void Dispose()
        {
            Abort();
        }
    }
}
