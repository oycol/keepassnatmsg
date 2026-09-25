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
    }
}
