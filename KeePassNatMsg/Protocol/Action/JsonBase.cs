using Newtonsoft.Json.Linq;
using System;

namespace KeePassNatMsg.Protocol.Action
{
    public class JsonBase : JObject
    {
        public JsonBase() : base()
        {
        }

        public JsonBase(JObject obj) : base(obj)
        {
        }

        public void AddBytes(string key, byte[] data)
        {
            Add(key, Convert.ToBase64String(data));
        }

        public string GetString(string key)
        {
            var value = this[key] as JValue;
            return value == null ? null : value.Value as string;
        }

        /// <summary>
        /// Decodes a base64-encoded field.  Returns an empty byte array (never null) when the
        /// field is absent, null, or empty so callers never receive a null reference.
        /// Throws <see cref="FormatException"/> only if the field is present but not valid base64.
        /// </summary>
        public byte[] GetBytes(string key)
        {
            var s = GetString(key);
            if (string.IsNullOrEmpty(s))
                return Array.Empty<byte>();
            return Convert.FromBase64String(s);
        }
    }
}
