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
            Assert.AreEqual(ChromeIntegrationService.ChromeExtensionOrigin, (string)origins[0]);
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
    }
}
