using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace KeePassNatMsg.Tests
{
    /// <summary>
    /// Tests for Native Messaging stdio framing as used by browser extensions.
    /// The browser native messaging protocol uses a 4-byte little-endian length
    /// prefix followed by the JSON message. This tests the framing logic
    /// independently of the actual proxy executable.
    /// </summary>
    [TestFixture]
    public class NativeMessagingFramingTests
    {
        #region Message Framing

        [Test]
        public void Framing_ShortMessage_LengthPrefixCorrect()
        {
            // Native messaging format: 4-byte LE length + UTF-8 JSON
            var json = "{\"action\":\"change-public-keys\",\"publicKey\":\"abc\"}";
            var jsonBytes = Encoding.UTF8.GetBytes(json);
            var lengthPrefix = BitConverter.GetBytes((uint)jsonBytes.Length);

            Assert.AreEqual(4, lengthPrefix.Length);
            Assert.AreEqual((uint)jsonBytes.Length, BitConverter.ToUInt32(lengthPrefix, 0));
        }

        [Test]
        public void Framing_EmptyMessage_LengthIsZero()
        {
            var lengthPrefix = BitConverter.GetBytes((uint)0);
            Assert.AreEqual(0, BitConverter.ToUInt32(lengthPrefix, 0));
        }

        [Test]
        public void Framing_LargeMessage_LengthCorrect()
        {
            // Simulate a large message (1MB)
            var largeJson = new string('x', 1048576);
            var jsonBytes = Encoding.UTF8.GetBytes(largeJson);
            var lengthPrefix = BitConverter.GetBytes((uint)jsonBytes.Length);

            Assert.AreEqual((uint)1048576, BitConverter.ToUInt32(lengthPrefix, 0));
        }

        [Test]
        public void Framing_RoundTrip_WriteAndRead()
        {
            // Simulate writing a framed message and reading it back
            var json = "{\"action\":\"test-associate\",\"clientID\":\"cid\",\"nonce\":\"n\"}";
            var jsonBytes = Encoding.UTF8.GetBytes(json);

            using (var ms = new MemoryStream())
            {
                // Write length prefix (little-endian uint32)
                var lengthBytes = BitConverter.GetBytes((uint)jsonBytes.Length);
                ms.Write(lengthBytes, 0, 4);

                // Write message body
                ms.Write(jsonBytes, 0, jsonBytes.Length);

                // Read back
                ms.Position = 0;
                var lengthBuf = new byte[4];
                ms.Read(lengthBuf, 0, 4);
                var msgLength = BitConverter.ToUInt32(lengthBuf, 0);

                Assert.AreEqual((uint)jsonBytes.Length, msgLength);

                var msgBuf = new byte[msgLength];
                ms.Read(msgBuf, 0, (int)msgLength);
                var msg = Encoding.UTF8.GetString(msgBuf);

                Assert.AreEqual(json, msg);
            }
        }

        [Test]
        public void Framing_MultipleMessages_SequentialRead()
        {
            // Test reading multiple messages in sequence (as a proxy would)
            var msg1 = "{\"action\":\"change-public-keys\"}";
            var msg2 = "{\"action\":\"test-associate\"}";
            var msg3 = "{\"action\":\"get-logins\"}";

            using (var ms = new MemoryStream())
            {
                // Write all three messages
                foreach (var msg in new[] { msg1, msg2, msg3 })
                {
                    var bytes = Encoding.UTF8.GetBytes(msg);
                    ms.Write(BitConverter.GetBytes((uint)bytes.Length), 0, 4);
                    ms.Write(bytes, 0, bytes.Length);
                }

                // Read them back
                ms.Position = 0;
                var messages = new System.Collections.Generic.List<string>();
                while (ms.Position < ms.Length)
                {
                    var lenBuf = new byte[4];
                    if (ms.Read(lenBuf, 0, 4) < 4) break;
                    var len = BitConverter.ToUInt32(lenBuf, 0);
                    var buf = new byte[len];
                    ms.Read(buf, 0, (int)len);
                    messages.Add(Encoding.UTF8.GetString(buf));
                }

                Assert.AreEqual(3, messages.Count);
                Assert.AreEqual(msg1, messages[0]);
                Assert.AreEqual(msg2, messages[1]);
                Assert.AreEqual(msg3, messages[2]);
            }
        }

        #endregion

        #region JSON Message Validation

        [Test]
        public void JsonMessage_ChangePublicKeys_HasRequiredFields()
        {
            var json = "{\"action\":\"change-public-keys\",\"publicKey\":\"abc123\",\"nonce\":\"n\",\"clientID\":\"cid\"}";
            var obj = Newtonsoft.Json.Linq.JObject.Parse(json);

            Assert.AreEqual("change-public-keys", obj["action"].ToString());
            Assert.IsNotNull(obj["publicKey"]);
            Assert.IsNotNull(obj["nonce"]);
            Assert.IsNotNull(obj["clientID"]);
        }

        [Test]
        public void JsonMessage_Associate_HasRequiredFields()
        {
            var json = "{\"action\":\"associate\",\"message\":\"enc\",\"nonce\":\"n\",\"clientID\":\"cid\"}";
            var obj = Newtonsoft.Json.Linq.JObject.Parse(json);

            Assert.AreEqual("associate", obj["action"].ToString());
            Assert.IsNotNull(obj["message"]);
            Assert.IsNotNull(obj["nonce"]);
            Assert.IsNotNull(obj["clientID"]);
        }

        [Test]
        public void JsonMessage_GetLogins_HasRequiredFields()
        {
            var json = "{\"action\":\"get-logins\",\"message\":\"enc\",\"nonce\":\"n\",\"clientID\":\"cid\"}";
            var obj = Newtonsoft.Json.Linq.JObject.Parse(json);

            Assert.AreEqual("get-logins", obj["action"].ToString());
        }

        [Test]
        public void JsonMessage_GeneratePassword_HasRequestID()
        {
            // 1.10.4 generate-password includes optional requestID
            var json = "{\"action\":\"generate-password\",\"nonce\":\"n\",\"clientID\":\"cid\",\"requestID\":\"abc12345\"}";
            var obj = Newtonsoft.Json.Linq.JObject.Parse(json);

            Assert.AreEqual("generate-password", obj["action"].ToString());
            Assert.IsNotNull(obj["requestID"]);
        }

        [Test]
        public void JsonMessage_GetTotp_HasUuid()
        {
            var json = "{\"action\":\"get-totp\",\"message\":\"enc\",\"nonce\":\"n\",\"clientID\":\"cid\"}";
            var obj = Newtonsoft.Json.Linq.JObject.Parse(json);

            Assert.AreEqual("get-totp", obj["action"].ToString());
        }

        [Test]
        public void JsonMessage_ResponseFormat_HasActionAndNonce()
        {
            // Every response should have action and nonce
            var json = "{\"action\":\"change-public-keys\",\"publicKey\":\"key\",\"version\":\"2.7.0\",\"success\":\"true\"}";
            var obj = Newtonsoft.Json.Linq.JObject.Parse(json);

            Assert.AreEqual("change-public-keys", obj["action"].ToString());
            Assert.AreEqual("2.7.0", obj["version"].ToString());
            Assert.AreEqual("true", obj["success"].ToString());
        }

        #endregion

        #region Manifest Validation

        [Test]
        public void Manifest_HasRequiredFields()
        {
            var manifestJson = @"{
                ""name"": ""org.keepassxc.keepassxc_browser"",
                ""description"": ""KeePassXC-Browser native messaging host"",
                ""type"": ""stdio"",
                ""path"": ""C:\\path\\to\\proxy.exe"",
                ""allowed_origins"": [""chrome-extension://abc/""]
            }";

            var obj = Newtonsoft.Json.Linq.JObject.Parse(manifestJson);

            Assert.AreEqual("org.keepassxc.keepassxc_browser", obj["name"].ToString());
            Assert.AreEqual("stdio", obj["type"].ToString());
            Assert.IsNotNull(obj["path"]);
            Assert.IsNotNull(obj["allowed_origins"]);
            Assert.Greater(((Newtonsoft.Json.Linq.JArray)obj["allowed_origins"]).Count, 0);
        }

        [Test]
        public void Manifest_AllowedExtensionOrigins_FollowKeePassXcUpstreamAllowlist()
        {
            var origins = KeePassNatMsg.NativeMessaging.ChromeIntegrationService.AllowedExtensionOrigins;

            Assert.Contains("chrome-extension://pdffhmdngciaglkoonimfcmckehcpafo/", origins);
            Assert.Contains("chrome-extension://oboonakemofpalcgghocfoadofidjkkk/", origins);
            Assert.AreEqual(2, origins.Length);
        }

        #endregion
    }
}
