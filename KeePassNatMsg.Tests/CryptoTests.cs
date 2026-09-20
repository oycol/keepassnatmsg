using KeePassNatMsg.Protocol.Crypto;
using NUnit.Framework;
using System;

namespace KeePassNatMsg.Tests
{
    [TestFixture]
    public class CryptoTests
    {
        #region KeyPair

        [Test]
        public void KeyPair_Generation_PublicKeyIs32Bytes()
        {
            var pair = new KeyPair();
            Assert.AreEqual(32, pair.PublicKey.Length, "Public key must be 32 bytes (TweetNaCl)");
        }

        [Test]
        public void KeyPair_Generation_PrivateKeyIs32Bytes()
        {
            var pair = new KeyPair();
            Assert.AreEqual(TweetNaCl.BoxSecretKeyBytes, pair.PrivateKey.Length, "Private key must be BoxSecretKeyBytes");
        }

        [Test]
        public void KeyPair_Generation_KeysAreDifferent()
        {
            var pair = new KeyPair();
            CollectionAssert.AreNotEqual(pair.PublicKey, pair.PrivateKey);
        }

        [Test]
        public void KeyPair_Generation_ProducesUniqueKeys()
        {
            var pair1 = new KeyPair();
            var pair2 = new KeyPair();
            CollectionAssert.AreNotEqual(pair1.PublicKey, pair2.PublicKey);
            CollectionAssert.AreNotEqual(pair1.PrivateKey, pair2.PrivateKey);
        }

        [Test]
        public void KeyPair_ToBase64_FromBase64_RoundTrip()
        {
            var pair = new KeyPair();
            var base64 = pair.ToBase64();
            var restored = KeyPair.FromBase64(base64);

            CollectionAssert.AreEqual(pair.PrivateKey, restored.PrivateKey);
            CollectionAssert.AreEqual(pair.PublicKey, restored.PublicKey);
        }

        [Test]
        public void KeyPair_ToBase64_NotEmpty()
        {
            var pair = new KeyPair();
            var base64 = pair.ToBase64();
            Assert.IsNotNullOrEmpty(base64);
        }

        #endregion

        #region Helper - Nonce

        [Test]
        public void Helper_GenerateNonce_IncrementsCorrectly()
        {
            var nonce = new byte[24];
            nonce[0] = 100;

            var incremented = Helper.GenerateNonce(nonce);

            Assert.AreEqual(101, incremented[0]);
        }

        [Test]
        public void Helper_GenerateNonce_PreservesLength()
        {
            var nonce = new byte[24];
            var incremented = Helper.GenerateNonce(nonce);
            Assert.AreEqual(24, incremented.Length);
        }

        [Test]
        public void Helper_GenerateNonce_CarryPropagation()
        {
            var nonce = new byte[24];
            nonce[0] = 0xFF;
            nonce[1] = 0x00;

            var incremented = Helper.GenerateNonce(nonce);

            Assert.AreEqual(0x00, incremented[0]);
            Assert.AreEqual(0x01, incremented[1]);
        }

        #endregion

        #region Helper - Key Pair Management

        [Test]
        public void Helper_GenerateKeyPair_Returns32BytePublicKey()
        {
            var helper = new Helper();
            var clientPubKey = new byte[TweetNaCl.BoxPublicKeyBytes];

            var serverPubKey = helper.GenerateKeyPair("client1", clientPubKey);

            Assert.AreEqual(32, serverPubKey.Length);
        }

        [Test]
        public void Helper_ClientPublicKey_ReturnsStoredKey()
        {
            var helper = new Helper();
            var clientPubKey = new byte[TweetNaCl.BoxPublicKeyBytes];
            for (int i = 0; i < clientPubKey.Length; i++)
                clientPubKey[i] = (byte)(i + 1);

            helper.GenerateKeyPair("testClient", clientPubKey);
            var retrieved = helper.ClientPublicKey("testClient");

            CollectionAssert.AreEqual(clientPubKey, retrieved);
        }

        [Test]
        public void Helper_GenerateKeyPair_OverwritesExisting()
        {
            var helper = new Helper();
            var key1 = new byte[TweetNaCl.BoxPublicKeyBytes];
            var key2 = new byte[TweetNaCl.BoxPublicKeyBytes];
            key2[0] = 99;

            helper.GenerateKeyPair("client", key1);
            helper.GenerateKeyPair("client", key2);

            var retrieved = helper.ClientPublicKey("client");
            CollectionAssert.AreEqual(key2, retrieved);
        }

        [Test]
        public void Helper_ClientPublicKey_UnknownClient_ReturnsNull()
        {
            var helper = new Helper();
            Assert.IsNull(helper.ClientPublicKey("nonexistent"));
        }

        #endregion

        #region Helper - Encrypt/Decrypt Round Trip

        [Test]
        public void Helper_EncryptDecrypt_RoundTrip()
        {
            var helper = new Helper();

            // Generate client key pair
            var clientPair = new KeyPair();
            var clientPubKey = clientPair.PublicKey;

            // Generate server key pair (stores client's public key)
            var serverPubKey = helper.GenerateKeyPair("testClient", clientPubKey);

            // Now we have:
            // - helper has server's private key + client's public key
            // - clientPair has client's private key + client's public key
            // We need server's public key for the client side

            // Encrypt a message from "server" to "client"
            // Helper encrypts with clientId's keys: uses client's public key + server's private key
            string message = "{\"action\":\"test\",\"data\":\"hello world\"}";
            var nonce = new byte[TweetNaCl.BoxNonceBytes];
            for (int i = 0; i < nonce.Length; i++)
                nonce[i] = (byte)(i + 1);

            var encrypted = helper.EncryptMessage("testClient", message, nonce);
            Assert.IsNotNull(encrypted);

            // Decrypt on client side: use server's public key + client's private key
            // For TweetNaCl.CryptoBoxOpen: (ciphertext, nonce, theirPubKey, myPrivKey)
            var decrypted = TweetNaCl.CryptoBoxOpen(encrypted, nonce, serverPubKey, clientPair.PrivateKey);
            Assert.IsNotNull(decrypted);

            var decryptedStr = System.Text.Encoding.UTF8.GetString(decrypted);
            Assert.AreEqual(message, decryptedStr);
        }

        [Test]
        public void Helper_EncryptMessage_UnknownClient_ReturnsNull()
        {
            var helper = new Helper();
            var result = helper.EncryptMessage("unknown", "test", new byte[24]);
            Assert.IsNull(result);
        }

        [Test]
        public void Helper_DecryptMessage_UnknownClient_ReturnsNull()
        {
            var helper = new Helper();
            var result = helper.DecryptMessage("unknown", new byte[32], new byte[24]);
            Assert.IsNull(result);
        }

        #endregion

        #region TweetNaCl Constants

        [Test]
        public void TweetNaCl_BoxPublicKeyBytes_Is32()
        {
            Assert.AreEqual(32, TweetNaCl.BoxPublicKeyBytes);
        }

        [Test]
        public void TweetNaCl_BoxSecretKeyBytes_Is32()
        {
            Assert.AreEqual(32, TweetNaCl.BoxSecretKeyBytes);
        }

        [Test]
        public void TweetNaCl_BoxNonceBytes_Is24()
        {
            Assert.AreEqual(24, TweetNaCl.BoxNonceBytes);
        }

        [Test]
        public void TweetNaCl_BoxBeforenmBytes_Is32()
        {
            Assert.AreEqual(32, TweetNaCl.BoxBeforenmBytes);
        }

        #endregion

        #region TweetNaCl Operations

        [Test]
        public void TweetNaCl_CryptoBoxKeypair_GeneratesValidKeys()
        {
            var sk = new byte[TweetNaCl.BoxSecretKeyBytes];
            var pk = TweetNaCl.CryptoBoxKeypair(sk);

            Assert.AreEqual(32, pk.Length);
            Assert.AreEqual(32, sk.Length);

            // Verify the key pair works by doing a box/open round trip
            var nonce = new byte[TweetNaCl.BoxNonceBytes];
            var msg = System.Text.Encoding.UTF8.GetBytes("test message");
            var paddedMsg = new byte[msg.Length + TweetNaCl.BoxZeroBytes];
            Array.Copy(msg, 0, paddedMsg, TweetNaCl.BoxZeroBytes, msg.Length);

            var box = TweetNaCl.CryptoBox(paddedMsg, nonce, pk, sk);
            Assert.IsNotNull(box);

            var opened = TweetNaCl.CryptoBoxOpen(box, nonce, pk, sk);
            Assert.IsNotNull(opened);

            // Extract the message (skip boxzerobytes)
            var result = new byte[msg.Length];
            Array.Copy(opened, TweetNaCl.BoxZeroBytes, result, 0, msg.Length);
            Assert.AreEqual("test message", System.Text.Encoding.UTF8.GetString(result));
        }

        [Test]
        public void TweetNaCl_Increment_BasicIncrement()
        {
            var data = new byte[24];
            data[0] = 5;

            var result = TweetNaCl.Increment(data);

            Assert.AreEqual(6, result[0]);
        }

        [Test]
        public void TweetNaCl_Increment_CarryOver()
        {
            var data = new byte[24];
            data[0] = 255;

            var result = TweetNaCl.Increment(data);

            Assert.AreEqual(0, result[0]);
            Assert.AreEqual(1, result[1]);
        }

        #endregion
    }
}
