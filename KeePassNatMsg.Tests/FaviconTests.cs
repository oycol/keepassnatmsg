using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using KeePassNatMsg.Favicon;
using NUnit.Framework;

namespace KeePassNatMsg.Tests
{
    [TestFixture]
    public class FaviconTests
    {
        [Test]
        [TestCase("https://example.com/login", "example.com")]
        [TestCase("http://sub.domain.org:8080/path/to/page?q=test#hash", "sub.domain.org")]
        [TestCase("github.com/keepassxreboot", "github.com")]
        [TestCase("192.168.1.1:8443/admin", "192.168.1.1")]
        [TestCase("https://my-service.co.uk", "my-service.co.uk")]
        public void ExtractHostname_VariousUrls_ReturnsExpectedHost(string inputUrl, string expectedHost)
        {
            string host = FaviconDownloader.ExtractHostname(inputUrl);
            Assert.AreEqual(expectedHost, host);
        }

        [Test]
        public void ExtractHostname_NullOrEmpty_ReturnsEmpty()
        {
            Assert.AreEqual(string.Empty, FaviconDownloader.ExtractHostname(null));
            Assert.AreEqual(string.Empty, FaviconDownloader.ExtractHostname(""));
            Assert.AreEqual(string.Empty, FaviconDownloader.ExtractHostname("   "));
        }

        [Test]
        public void ExtractFaviconHrefsFromHtml_ModernTags_ExtractsAllIcons()
        {
            string html = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>Test Page</title>
    <link rel=""shortcut icon"" href=""/favicon.ico"">
    <link rel=""icon"" type=""image/png"" sizes=""32x32"" href=""/assets/favicon-32x32.png"">
    <link rel=""apple-touch-icon"" sizes=""180x180"" href=""https://cdn.example.com/apple-touch-icon.png"">
    <link rel=""stylesheet"" href=""/styles.css"">
</head>
<body>
    <h1>Hello World</h1>
</body>
</html>";

            List<string> hrefs = FaviconDownloader.ExtractFaviconHrefsFromHtml(html);
            Assert.AreEqual(3, hrefs.Count);
            Assert.Contains("/favicon.ico", hrefs);
            Assert.Contains("/assets/favicon-32x32.png", hrefs);
            Assert.Contains("https://cdn.example.com/apple-touch-icon.png", hrefs);
        }

        [Test]
        public void ExtractFaviconHrefsFromHtml_CaseInsensitiveAndSingleQuotes_ExtractsCorrectly()
        {
            string html = @"
<HTML>
<HEAD>
    <LINK REL='ICON' HREF='/images/custom-icon.png' />
    <LINK rel=""APPLE-TOUCH-ICON-PRECOMPOSED"" href='/touch-icon.png' />
</HEAD>
</HTML>";

            List<string> hrefs = FaviconDownloader.ExtractFaviconHrefsFromHtml(html);
            Assert.AreEqual(2, hrefs.Count);
            Assert.Contains("/images/custom-icon.png", hrefs);
            Assert.Contains("/touch-icon.png", hrefs);
        }

        [Test]
        public void FaviconProvider_TemplatePlaceholder_ReplacesCorrectly()
        {
            string template = "https://icons.duckduckgo.com/ip3/{URL:HOST}.ico";
            string built = FaviconProvider.BuildProviderUrl(template, "github.com", 64);
            Assert.AreEqual("https://icons.duckduckgo.com/ip3/github.com.ico", built);

            string googleTemplate = "https://www.google.com/s2/favicons?domain={URL:HOST}&sz={YAFD:ICON_SIZE}";
            string builtGoogle = FaviconProvider.BuildProviderUrl(googleTemplate, "example.com", 128);
            Assert.AreEqual("https://www.google.com/s2/favicons?domain=example.com&sz=128", builtGoogle);
        }

        [Test]
        public void FaviconProvider_GetProviders_HasPresetList()
        {
            FaviconProvider[] providers = FaviconProvider.GetProviders();
            Assert.GreaterOrEqual(providers.Length, 4);

            FaviconProvider direct = FaviconProvider.FindByName(FaviconProvider.ProviderDirect);
            Assert.NotNull(direct);
            Assert.AreEqual(FaviconProvider.ProviderDirect, direct.Name);

            FaviconProvider custom = FaviconProvider.FindByName(FaviconProvider.ProviderCustom);
            Assert.NotNull(custom);
        }

        [Test]
        public void TryProcessAndResizeImage_DownscalesLargeImages()
        {
            byte[] rawImageBytes;
            using (Bitmap bmp = new Bitmap(200, 200))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Blue);
                }
                using (MemoryStream ms = new MemoryStream())
                {
                    bmp.Save(ms, ImageFormat.Png);
                    rawImageBytes = ms.ToArray();
                }
            }

            byte[] resizedBytes;
            bool success = FaviconDownloader.TryProcessAndResizeImage(rawImageBytes, 64, out resizedBytes);
            Assert.IsTrue(success);
            Assert.NotNull(resizedBytes);

            using (MemoryStream outMs = new MemoryStream(resizedBytes))
            {
                using (Image outImg = Image.FromStream(outMs))
                {
                    Assert.AreEqual(64, outImg.Width);
                    Assert.AreEqual(64, outImg.Height);
                }
            }
        }

        [Test]
        public void TryProcessAndResizeImage_InvalidData_ReturnsFalseSafely()
        {
            byte[] corruptData = new byte[] { 0xFF, 0x00, 0x12, 0x34 };
            byte[] result;
            bool success = FaviconDownloader.TryProcessAndResizeImage(corruptData, 128, out result);
            Assert.IsFalse(success);
            Assert.IsNull(result);
        }

        [Test]
        public void FaviconDownloader_Abort_SetsIsAbortedAndTerminatesSafely()
        {
            using (FaviconDownloader fd = new FaviconDownloader())
            {
                Assert.IsFalse(fd.IsAborted);
                fd.Abort();
                Assert.IsTrue(fd.IsAborted);

                // Any subsequent operations after abort should terminate without unhandled crashes
                Assert.Throws<FaviconDownloaderException>(delegate
                {
                    fd.DownloadFaviconDirect("https://127.0.0.1:65534/test", false, 128);
                });
            }
        }

        [Test]
        public void TryProcessAndResizeImage_NonSquareImage_PreservesAspectRatio()
        {
            byte[] rawImageBytes;
            using (Bitmap bmp = new Bitmap(400, 200)) // 2:1 ratio
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Red);
                }
                using (MemoryStream ms = new MemoryStream())
                {
                    bmp.Save(ms, ImageFormat.Png);
                    rawImageBytes = ms.ToArray();
                }
            }

            byte[] resizedBytes;
            bool success = FaviconDownloader.TryProcessAndResizeImage(rawImageBytes, 64, out resizedBytes);
            Assert.IsTrue(success);
            Assert.NotNull(resizedBytes);

            using (MemoryStream outMs = new MemoryStream(resizedBytes))
            {
                using (Image outImg = Image.FromStream(outMs))
                {
                    Assert.AreEqual(64, outImg.Width);
                    Assert.AreEqual(32, outImg.Height); // 2:1 preserved
                }
            }
        }

        [Test]
        public void ExtractFaviconHrefsFromHtml_ComplexTagsAndMixedAttributes_FiltersStrictly()
        {
            string html = @"
<html>
<head>
    <!-- Commented out: <link rel=""icon"" href=""/commented.ico""> -->
    <link rel=""stylesheet"" type=""text/css"" href=""/style.css"">
    <link rel=""dns-prefetch"" href=""//dns.example.com"">
    <link rel=""preload"" as=""font"" href=""/font.woff2"">
    <link rel='SHORTCUT ICON' href='/shortcut.ico'>
    <link rel=icon type=image/svg+xml href=/logo.svg>
    <link rel=""apple-touch-icon-precomposed"" sizes=""120x120"" href=""/apple-pre.png"">
</head>
</html>";

            List<string> hrefs = FaviconDownloader.ExtractFaviconHrefsFromHtml(html);
            Assert.AreEqual(3, hrefs.Count);
            Assert.Contains("/shortcut.ico", hrefs);
            Assert.Contains("/logo.svg", hrefs);
            Assert.Contains("/apple-pre.png", hrefs);
            Assert.IsFalse(hrefs.Contains("/commented.ico"));
            Assert.IsFalse(hrefs.Contains("/style.css"));
            Assert.IsFalse(hrefs.Contains("//dns.example.com"));
        }

        [Test]
        public void FaviconProvider_BuildProviderUrl_EdgeCasePlaceholders()
        {
            // Null or empty template
            Assert.AreEqual(string.Empty, FaviconProvider.BuildProviderUrl(null, "example.com", 32));
            Assert.AreEqual(string.Empty, FaviconProvider.BuildProviderUrl("", "example.com", 32));

            // Multiple occurrences of placeholders
            string multi = "https://srv.test/{URL:HOST}/icon/{YAFD:ICON_SIZE}?domain={url:host}&sz={yafd:icon_size}";
            string result = FaviconProvider.BuildProviderUrl(multi, "myhost.org", 64);
            Assert.AreEqual("https://srv.test/myhost.org/icon/64?domain=myhost.org&sz=64", result);
        }

        [Test]
        public void ExtractHostname_UrlWithCredentialsAndSpecialChars_ExtractsOnlyHost()
        {
            Assert.AreEqual("git.corp.internal", FaviconDownloader.ExtractHostname("https://alice:secret123@git.corp.internal:9443/repo/project?branch=main#readme"));
            Assert.AreEqual("10.0.1.50", FaviconDownloader.ExtractHostname("http://admin:pwd@10.0.1.50:8080/dashboard"));
        }

        [Test]
        public void FaviconDownloader_Abort_CanBeCalledMultipleTimesWithoutException()
        {
            using (FaviconDownloader fd = new FaviconDownloader())
            {
                fd.Abort();
                fd.Abort(); // Idempotent call
                Assert.IsTrue(fd.IsAborted);
            }
        }
    }
}
