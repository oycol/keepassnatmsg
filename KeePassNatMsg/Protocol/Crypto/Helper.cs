
using KeePassNatMsg.Protocol.Action;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Text;

namespace KeePassNatMsg.Protocol.Crypto
{
    public sealed class Helper
    {
        private UTF8Encoding _utf8 = new UTF8Encoding(false);
        private ConcurrentDictionary<string, KeyPair> _clientKeys;

        public Helper()
        {
            _clientKeys = new ConcurrentDictionary<string, KeyPair>();
        }

        public JsonBase DecryptMessage(string clientId, byte[] message, byte[] nonce)
        {
            KeyPair pair;
            if (_clientKeys.TryGetValue(clientId, out pair))
            {
                var data = TweetNaCl.CryptoBoxOpen(message, nonce, pair.PublicKey, pair.PrivateKey);
                return new JsonBase(JObject.Parse(_utf8.GetString(data)));
            }
            return null;
        }

        public byte[] EncryptMessage(string clientId, string msg, byte[] nonce)
        {
            KeyPair pair;
            if (_clientKeys.TryGetValue(clientId, out pair))
            {
                return TweetNaCl.CryptoBox(_utf8.GetBytes(msg), nonce, pair.PublicKey, pair.PrivateKey);
            }
            return null;
        }

        public byte[] GenerateKeyPair(string clientId, byte[] clientPublicKey)
        {
            var pair = new KeyPair();
            _clientKeys[clientId] = new KeyPair(pair.PrivateKey, clientPublicKey);
            return pair.PublicKey;
        }

        public byte[] ClientPublicKey(string clientId)
        {
            KeyPair pair;
            if (_clientKeys.TryGetValue(clientId, out pair))
            {
                return pair.PublicKey;
            }
            return null;
        }

        public static byte[] GenerateNonce(byte[] nonce)
        {
            if (nonce == null)
            {
                var newNonce = new byte[24];
                TweetNaCl.RandomBytes(newNonce);
                return newNonce;
            }
            return TweetNaCl.Increment(nonce);
        }
    }
}
