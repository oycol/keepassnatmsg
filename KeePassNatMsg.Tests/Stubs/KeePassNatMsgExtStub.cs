using System.IO;
using KeePass.Plugins;
using KeePassLib;
using KeePassNatMsg.Entry;
using KeePassNatMsg.Protocol.Crypto;
using Newtonsoft.Json;

namespace KeePassNatMsg
{
    public sealed class KeePassNatMsgExt
    {
        internal static IPluginHost HostInstance;
        internal static Helper CryptoHelper;
        internal static KeePassNatMsgExt ExtInstance;

        internal static string SettingKey
        {
            get { return "KeePassHttp Settings"; }
        }

        public static string GetVersion()
        {
            return "2.7.0";
        }

        internal string GetDbHashForMessage()
        {
            return "";
        }

        internal EntryConfig GetEntryConfig(PwEntry e)
        {
            if (e != null && e.CustomData.Exists(SettingKey))
            {
                var json = e.CustomData.Get(SettingKey);
                using (var ins = new JsonTextReader(new StringReader(json)))
                {
                    return new JsonSerializer().Deserialize<EntryConfig>(ins);
                }
            }
            return null;
        }

        internal void SetEntryConfig(PwEntry e, EntryConfig c)
        {
            if (e == null) return;
            var writer = new StringWriter();
            new JsonSerializer().Serialize(writer, c);
            e.CustomData.Set(SettingKey, writer.ToString());
        }

        internal string[] GetUserPass(PwEntry entry)
        {
            if (entry == null) return new[] { "", "" };
            return new[]
            {
                entry.Strings.ReadSafe(PwDefs.UserNameField),
                entry.Strings.ReadSafe(PwDefs.PasswordField)
            };
        }

        internal string[] GetUserPass(PwEntryDatabase entryDatabase)
        {
            if (entryDatabase == null || entryDatabase.entry == null) return new[] { "", "" };
            return GetUserPass(entryDatabase.entry);
        }

        internal void ShowNotification(string msg)
        {
        }

        internal PwDatabase GetSearchDatabase()
        {
            return null;
        }
    }
}
