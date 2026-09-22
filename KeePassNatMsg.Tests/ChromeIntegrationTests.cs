using System;
using System.IO;
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
            Assert.AreEqual("pdffhmdngciaglkoonimfcmckehcpafo", ChromeIntegrationService.ChromeExtensionId);
            Assert.AreEqual("chrome-extension://pdffhmdngciaglkoonimfcmckehcpafo/", ChromeIntegrationService.ChromeExtensionOrigin);
            Assert.AreEqual(@"Software\Google\Chrome\NativeMessagingHosts\org.keepassxc.keepassxc_browser", ChromeIntegrationService.RegistrySubKey);
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
            Assert.AreEqual(ChromeIntegrationService.ChromeExtensionOrigin, (string)origins[0]);
        }

        [Test]
        public void ProxyExpectedSha256_IsDefinedAndHex64()
        {
            Assert.IsNotNull(ChromeIntegrationService.ExpectedProxySha256);
            Assert.AreEqual(64, ChromeIntegrationService.ExpectedProxySha256.Length);
            // Verify all characters are hex
            foreach (var c in ChromeIntegrationService.ExpectedProxySha256)
            {
                Assert.IsTrue((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f'));
            }
        }
    }
}
