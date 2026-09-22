
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace KeePassNatMsg.Protocol.Action
{
    public class Request : JsonBase
    {
        private JsonBase _msg;

        public Request(JObject obj) : base(obj)
        {
        }

        public static Request ReadFromStream(System.IO.Stream s)
        {
            var reader = new JsonTextReader(new System.IO.StreamReader(s));
            return new Request((JObject)ReadFrom(reader));
        }

        public static Request FromString(string s)
        {
            var rdr = new JsonTextReader(new System.IO.StringReader(s));
            return new Request((JObject)ReadFrom(rdr));
        }

        public string ClientId
        {
            get
            {
                return GetString("clientID");
            }
        }

        public string Action
        {
            get
            {
                return GetString("action");
            }
        }

        public string Nonce
        {
            get
            {
                return GetString("nonce");
            }
        }

        public byte[] NonceBytes
        {
            get
            {
                return GetBytes("nonce");
            }
        }

        public bool TriggerUnlock
        {
            get
            {
                bool x;
                return bool.TryParse(GetString("triggerUnlock"), out x) && x;
            }
        }

        public JsonBase Message
        {
            get
            {
                return _msg;
            }
        }

        public Response GetResponse()
        {
            return new Response(this);
        }

        public Response GetResponse(bool createMessage)
        {
            return new Response(this, createMessage);
        }

        /// <summary>
        /// Attempts to decrypt the encrypted "message" payload.
        /// Returns false if the clientId is unknown, any field is missing/empty,
        /// or decryption fails for any reason (crypto error, bad nonce, …).
        /// </summary>
        public bool TryDecrypt()
        {
            try
            {
                var msgBytes   = GetBytes("message");
                var nonceBytes = GetBytes("nonce");

                // Empty fields mean the request is malformed — treat as failure.
                if (msgBytes.Length == 0 || nonceBytes.Length == 0)
                    return false;

                _msg = KeePassNatMsgExt.CryptoHelper.DecryptMessage(ClientId, msgBytes, nonceBytes);

                // DecryptMessage returns null when the clientId has no registered key pair.
                return _msg != null;
            }
            catch (Exception)
            {
                _msg = null;
            }
            return false;
        }
    }
}
