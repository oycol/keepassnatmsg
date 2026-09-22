using System;
using System.IO;
using System.Linq;
using KeePassNatMsg.NativeMessaging;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace KeePassNatMsg.Tests
{
    [TestFixture]
    public class ChromeIntegrationTests
    {
        private ChromeIntegrationService _service;

        [SetUp]
        public void SetUp()
        {
            _service = new ChromeIntegrationService();
        }

        [Test]
        public void Constants_AreCorrectForChromeAndKeePassXcBrowser()
        {
            Assert.AreEqual("org.keepassxc.keepassxc_browser", ChromeIntegrationService.NativeHostName);
            Assert.AreEqual(@"Software\Google\Chrome\NativeMessagingHosts\org.keepassxc.keepassxc_browser", ChromeIntegrationService.RegistrySubKey);

            // Verify both Chrome and Edge registry keys are registered
            Assert.AreEqual(2, ChromeIntegrationService.SupportedRegistryKeys.Length);
            Assert.Contains(@"Software\Google\Chrome\NativeMessagingHosts\org.keepassxc.keepassxc_browser", ChromeIntegrationService.SupportedRegistryKeys);
            Assert.Contains(@"Software\Microsoft\Edge\NativeMessagingHosts\org.keepassxc.keepassxc_browser", ChromeIntegrationService.SupportedRegistryKeys);
        }

        [Test]
        public void GenerateManifestContent_ProducesValidJson_WithCorrectAttributes()
        {
            var fakeProxyPath = @"C:\Users\TestUser\AppData\Local\KeePassNatMsg\keepassnatmsg-proxy.exe";
            var json = _service.GenerateManifestContent(fakeProxyPath);

            Assert.IsNotNull(json);
            var parsed = JObject.Parse(json);

            Assert.AreEqual(ChromeIntegrationService.NativeHostName, (string)parsed["name"]);
            Assert.AreEqual("stdio", (string)parsed["type"]);
            Assert.AreEqual(fakeProxyPath, (string)parsed["path"]);

            var origins = parsed["allowed_origins"] as JArray;
            Assert.IsNotNull(origins);
            Assert.AreEqual(ChromeIntegrationService.AllowedExtensionOrigins.Length, origins.Count);
            Assert.Contains("chrome-extension://pdffhmdngciaglkoonimfcmckehcpafo/", origins.Select(o => o.ToString()).ToArray());
            Assert.Contains("chrome-extension://oboonakemofpalcgghocfoadofidjkkk/", origins.Select(o => o.ToString()).ToArray());
        }

        [Test]
        public void ExecutableValidation_DetectsValidMzHeader()
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                // File less than 1KB should fail
                File.WriteAllBytes(tempFile, new byte[] { 0x4D, 0x5A });
                Assert.IsFalse(ChromeIntegrationService.IsValidExecutable(tempFile));

                // 2KB file with valid MZ header should pass
                var validDummy = new byte[2048];
                validDummy[0] = (byte)'M';
                validDummy[1] = (byte)'Z';
                File.WriteAllBytes(tempFile, validDummy);
                Assert.IsTrue(ChromeIntegrationService.IsValidExecutable(tempFile));
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }
        [Test]
        public void GenerateManifestContent_PathWithBackslashes_SurvivesJsonRoundTrip()
        {
            // Fix #13: hand-rolled JSON used Replace(@"\", @"\\") which could break UNC paths.
            // Newtonsoft handles escaping correctly in all cases.
            var pathWithSlashes = @"C:\Users\Test User\AppData\Local\KeePassNatMsg\keepassnatmsg-proxy.exe";
            var json = _service.GenerateManifestContent(pathWithSlashes);
            var parsed = JObject.Parse(json);   // must not throw
            Assert.AreEqual(pathWithSlashes, (string)parsed["path"]);
        }

        [Test]
        public void GenerateManifestContent_UncPath_SurvivesJsonRoundTrip()
        {
            var uncPath = @"\\server\share\keepassnatmsg-proxy.exe";
            var json = _service.GenerateManifestContent(uncPath);
            var parsed = JObject.Parse(json);
            Assert.AreEqual(uncPath, (string)parsed["path"]);
        }
    }
}
