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
        private const int RequestTimeoutMs = 8000;

        private readonly CookieContainer _cookies = new CookieContainer();
        private IWebProxy _proxy;

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
            if (attempts > 3)
                throw new FaviconDownloaderException(FaviconErrorStatus.NotFound);

            Uri baseUri;
            if (!Uri.TryCreate(currentUrl, UriKind.Absolute, out baseUri))
                throw new FaviconDownloaderException(FaviconErrorStatus.NotFound);

            try
            {
                string html;
                Uri responseUri;
                bool pageLoaded = TryFetchPage(baseUri, out html, out responseUri);

                List<Uri> candidateUrls = new List<Uri>();

                if (pageLoaded && !string.IsNullOrEmpty(html))
                {
                    List<string> hrefs = ExtractFaviconHrefsFromHtml(html);
                    for (int i = 0; i < hrefs.Count; i++)
                    {
                        Uri parsed;
                        if (Uri.TryCreate(responseUri ?? baseUri, hrefs[i], out parsed))
                        {
                            candidateUrls.Add(parsed);
                        }
                    }
                }

                // Direct fallback: standard root /favicon.ico
                Uri rootFavicon;
                if (Uri.TryCreate(new Uri(baseUri.GetLeftPart(UriPartial.Authority)), "/favicon.ico", out rootFavicon))
                {
                    if (!candidateUrls.Contains(rootFavicon))
                    {
                        candidateUrls.Add(rootFavicon);
                    }
                }

                for (int i = 0; i < candidateUrls.Count; i++)
                {
                    byte[] rawData = TryDownloadAsset(candidateUrls[i]);
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

        public static List<string> ExtractFaviconHrefsFromHtml(string html)
        {
            List<string> results = new List<string>();
            if (string.IsNullOrEmpty(html)) return results;

            Match headMatch = HeadTagRegex.Match(html);
            string headContent = headMatch.Success ? headMatch.Groups["content"].Value : html;

            // Check base tag
            string baseUrl = null;
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
                // but is valid ICO header, return rawData if within size.
                if (rawData.Length >= 4 && rawData[0] == 0 && rawData[1] == 0 && (rawData[2] == 1 || rawData[2] == 2))
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

            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(uri);
                req.Method = "GET";
                req.UserAgent = DefaultUserAgent;
                req.Timeout = RequestTimeoutMs;
                req.ReadWriteTimeout = RequestTimeoutMs;
                req.CookieContainer = _cookies;
                req.Proxy = _proxy;
                req.AllowAutoRedirect = true;
                req.MaximumAutomaticRedirections = 5;

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
        }

        private byte[] TryDownloadAsset(Uri uri)
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(uri);
                req.Method = "GET";
                req.UserAgent = DefaultUserAgent;
                req.Timeout = RequestTimeoutMs;
                req.ReadWriteTimeout = RequestTimeoutMs;
                req.CookieContainer = _cookies;
                req.Proxy = _proxy;
                req.AllowAutoRedirect = true;

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
        }

        public void Dispose()
        {
        }
    }
}
