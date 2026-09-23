using KeePassNatMsg.Protocol;
using KeePassNatMsg.Protocol.Action;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;

namespace KeePassNatMsg.Tests
{
    [TestFixture]
    public class ProtocolTests
    {
        #region Actions Constants

        [Test]
        public void Actions_AllExpectedActionStrings_Present()
        {
            // Verify all actions expected by KeePassXC-Browser 1.10.4 are defined
            Assert.AreEqual("get-databasehash", Actions.GET_DATABASE_HASH);
            Assert.AreEqual("associate", Actions.ASSOCIATE);
            Assert.AreEqual("test-associate", Actions.TEST_ASSOCIATE);
            Assert.AreEqual("get-logins", Actions.GET_LOGINS);
            Assert.AreEqual("get-logins-count", Actions.GET_LOGINS_COUNT);
            Assert.AreEqual("set-login", Actions.SET_LOGIN);
            Assert.AreEqual("generate-password", Actions.GENERATE_PASSWORD);
            Assert.AreEqual("change-public-keys", Actions.CHANGE_PUBLIC_KEYS);
            Assert.AreEqual("lock-database", Actions.LOCK_DATABASE);
            Assert.AreEqual("database-locked", Actions.DATABASE_LOCKED);
            Assert.AreEqual("database-unlocked", Actions.DATABASE_UNLOCKED);
            Assert.AreEqual("get-database-groups", Actions.GET_DATABASE_GROUPS);
            Assert.AreEqual("create-new-group", Actions.CREATE_NEW_GROUP);
            Assert.AreEqual("get-totp", Actions.GET_TOTP);
            Assert.AreEqual("request-autotype", Actions.REQUEST_AUTOTYPE);
        }

        [Test]
        public void Actions_NewActionsExist()
        {
            // Verify new actions added for 1.10.4 compatibility
            Assert.IsNotNull(Actions.GET_LOGINS_COUNT);
            Assert.IsNotNull(Actions.REQUEST_AUTOTYPE);
            Assert.IsNotEmpty(Actions.GET_LOGINS_COUNT);
            Assert.IsNotEmpty(Actions.REQUEST_AUTOTYPE);
        }

        #endregion

        #region JsonBase

        [Test]
        public void JsonBase_AddBytes_GetBytes_RoundTrip()
        {
            var jb = new JsonBase();
            var data = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            jb.AddBytes("test", data);

            var retrieved = jb.GetBytes("test");
            Assert.AreEqual(data, retrieved);
        }

        [Test]
        public void JsonBase_AddBytes_Base64Encoded()
        {
            var jb = new JsonBase();
            var data = new byte[] { 0, 1, 2, 3 };
            jb.AddBytes("key", data);

            // Verify it's stored as base64 string
            var value = jb["key"] as JValue;
            Assert.IsNotNull(value);
            Assert.AreEqual(Convert.ToBase64String(data), value.Value);
        }

        [Test]
        public void JsonBase_GetString_ReturnsValue()
        {
            var jb = new JsonBase();
            jb.Add("name", "testvalue");
            Assert.AreEqual("testvalue", jb.GetString("name"));
        }

        [Test]
        public void JsonBase_GetString_MissingKey_ReturnsNull()
        {
            var jb = new JsonBase();
            Assert.IsNull(jb.GetString("nonexistent"));
        }

        [Test]
        public void JsonBase_ConstructionFromJObject()
        {
            var obj = JObject.Parse("{\"action\":\"test\",\"value\":42}");
            var jb = new JsonBase(obj);
            Assert.AreEqual("test", jb.GetString("action"));
        }

        [Test]
        public void JsonBase_GetBytes_MissingKey_ReturnsEmptyArray()
        {
            // Fix #2: GetBytes must not throw on missing/null key
            var jb = new JsonBase();
            var result = jb.GetBytes("nonexistent");
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Length);
        }

        [Test]
        public void JsonBase_GetBytes_NullValue_ReturnsEmptyArray()
        {
            // Fix #2: GetBytes must not throw when the stored value is null
            var jb = new JsonBase();
            jb.Add("key", (string)null);
            var result = jb.GetBytes("key");
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Length);
        }

        [Test]
        public void JsonBase_GetBytes_EmptyString_ReturnsEmptyArray()
        {
            var jb = new JsonBase();
            jb.Add("key", "");
            var result = jb.GetBytes("key");
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Length);
        }

        #endregion

        #region Errors

        [Test]
        public void ErrorType_AllExpectedValuesExist()
        {
            // Verify all error types are defined
            Assert.IsTrue(Enum.IsDefined(typeof(ErrorType), ErrorType.DatabaseNotOpened));
            Assert.IsTrue(Enum.IsDefined(typeof(ErrorType), ErrorType.CannotDecryptMessage));
            Assert.IsTrue(Enum.IsDefined(typeof(ErrorType), ErrorType.AssociationFailed));
            Assert.IsTrue(Enum.IsDefined(typeof(ErrorType), ErrorType.NoLoginsFound));
            Assert.IsTrue(Enum.IsDefined(typeof(ErrorType), ErrorType.NoUrlProvided));
            Assert.IsTrue(Enum.IsDefined(typeof(ErrorType), ErrorType.NoGroupsFound));
            Assert.IsTrue(Enum.IsDefined(typeof(ErrorType), ErrorType.CannotCreateNewGroup));
            Assert.IsTrue(Enum.IsDefined(typeof(ErrorType), ErrorType.IncorrectAction));
        }

        [Test]
        public void Errors_GetErrorMessage_KnownErrors()
        {
            Assert.AreEqual("Database not opened", Errors.GetErrorMessage(ErrorType.DatabaseNotOpened));
            Assert.AreEqual("Cannot decrypt message", Errors.GetErrorMessage(ErrorType.CannotDecryptMessage));
            Assert.AreEqual("Association failed", Errors.GetErrorMessage(ErrorType.AssociationFailed));
            Assert.AreEqual("No Logins Found", Errors.GetErrorMessage(ErrorType.NoLoginsFound));
            Assert.AreEqual("No URL provided", Errors.GetErrorMessage(ErrorType.NoUrlProvided));
        }

        [Test]
        public void Errors_GetErrorMessage_AllEnumsHaveMessages()
        {
            foreach (ErrorType et in Enum.GetValues(typeof(ErrorType)))
            {
                string msg = Errors.GetErrorMessage(et);
                Assert.IsFalse(string.IsNullOrEmpty(msg), "Error message for {0} should not be empty", et);
            }
        }

        #endregion

        #region Request Parsing

        [Test]
        public void Request_FromString_ParsesAction()
        {
            var json = "{\"action\":\"change-public-keys\",\"publicKey\":\"abc123\",\"nonce\":\"tZvLrBzkQ9GxXq9PvKJj4iAnfPT0VZ3Q\",\"clientID\":\"client123\"}";
            var req = Request.FromString(json);
            Assert.AreEqual("change-public-keys", req.Action);
        }

        [Test]
        public void Request_FromString_ParsesClientId()
        {
            var json = "{\"action\":\"test-associate\",\"clientID\":\"myClientId\",\"nonce\":\"nonce123\",\"message\":\"msg\"}";
            var req = Request.FromString(json);
            Assert.AreEqual("myClientId", req.ClientId);
        }

        [Test]
        public void Request_FromString_ParsesNonce()
        {
            var json = "{\"action\":\"test-associate\",\"clientID\":\"cid\",\"nonce\":\"tZvLrBzkQ9GxXq9PvKJj4iAnfPT0VZ3Q\",\"message\":\"msg\"}";
            var req = Request.FromString(json);
            Assert.AreEqual("tZvLrBzkQ9GxXq9PvKJj4iAnfPT0VZ3Q", req.Nonce);
        }

        [Test]
        public void Request_FromString_ParsesTriggerUnlock()
        {
            var json = "{\"action\":\"get-logins\",\"clientID\":\"cid\",\"nonce\":\"n\",\"message\":\"m\",\"triggerUnlock\":\"true\"}";
            var req = Request.FromString(json);
            Assert.IsTrue(req.TriggerUnlock);
        }

        [Test]
        public void Request_FromString_TriggerUnlock_FalseWhenAbsent()
        {
            var json = "{\"action\":\"get-logins\",\"clientID\":\"cid\",\"nonce\":\"n\",\"message\":\"m\"}";
            var req = Request.FromString(json);
            Assert.IsFalse(req.TriggerUnlock);
        }

        [Test]
        public void Request_FromString_TriggerUnlock_FalseWhenFalse()
        {
            var json = "{\"action\":\"get-logins\",\"clientID\":\"cid\",\"nonce\":\"n\",\"message\":\"m\",\"triggerUnlock\":\"false\"}";
            var req = Request.FromString(json);
            Assert.IsFalse(req.TriggerUnlock);
        }

        #endregion

        #region Response Nonce Increment

        [Test]
        public void Response_NonceIncrement_IncrementsByOne()
        {
            // Test that Helper.GenerateNonce increments the nonce
            // This is what Response uses internally
            var originalNonce = new byte[24];
            originalNonce[0] = 42;

            var incremented = KeePassNatMsg.Protocol.Crypto.Helper.GenerateNonce(originalNonce);

            Assert.AreEqual(43, incremented[0]);
            // Remaining bytes should be same
            for (int i = 1; i < 24; i++)
            {
                Assert.AreEqual(originalNonce[i], incremented[i]);
            }
        }

        [Test]
        public void Response_NonceIncrement_CarryOver()
        {
            // Test carry-over: 0xFF + 1 = 0x00 with carry
            var originalNonce = new byte[24];
            originalNonce[0] = 0xFF;

            var incremented = KeePassNatMsg.Protocol.Crypto.Helper.GenerateNonce(originalNonce);

            Assert.AreEqual(0x00, incremented[0]);
            Assert.AreEqual(0x01, incremented[1]); // carry to next byte
        }

        [Test]
        public void Response_NonceIncrement_AllZeros()
        {
            var originalNonce = new byte[24];

            var incremented = KeePassNatMsg.Protocol.Crypto.Helper.GenerateNonce(originalNonce);

            Assert.AreEqual(1, incremented[0]);
            Assert.AreEqual(0, incremented[1]);
        }

        [Test]
        public void Request_TryDecrypt_ReturnsFalse_WhenMessageFieldMissing()
        {
            // Fix #4: TryDecrypt must return false (not true) when message/nonce fields are absent.
            // Previously: DecryptMessage returned null → TryDecrypt returned true → NPE later.
            var json = "{\"action\":\"get-logins\",\"clientID\":\"cid\",\"nonce\":\"tZvLrBzkQ9GxXq9PvKJj4iAnfPT0VZ3Q\"}";
            var req = Request.FromString(json);
            // No "message" field → GetBytes("message") returns empty array → TryDecrypt = false
            Assert.IsFalse(req.TryDecrypt());
            Assert.IsNull(req.Message);
        }

        [Test]
        public void Request_TryDecrypt_ReturnsFalse_WhenBothFieldsMissing()
        {
            // Fix #4: no message and no nonce → false
            var json = "{\"action\":\"get-logins\",\"clientID\":\"cid\"}";
            var req = Request.FromString(json);
            Assert.IsFalse(req.TryDecrypt());
            Assert.IsNull(req.Message);
        }
        [Test]
        public void PipeThreadState_Close_DoesNotThrowWhenServerIsNull()
        {
            var pts = new KeePassNatMsg.Protocol.Listener.PipeThreadState(null);
            Assert.DoesNotThrow(() => pts.Close());
        }

        #endregion
    }
}
