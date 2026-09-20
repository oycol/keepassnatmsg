using KeePassNatMsg.Protocol.Crypto;

namespace KeePassNatMsg
{
    /// <summary>
    /// Minimal stub for KeePassNatMsgExt to satisfy compile-time references
    /// in Request.cs (TryDecrypt → KeePassNatMsgExt.CryptoHelper) and
    /// Response.cs (GetEncryptedResponse → CryptoHelper.EncryptMessage,
    /// CreateMessage → ExtInstance.GetDbHashForMessage / GetVersion).
    ///
    /// The real KeePassNatMsgExt derives from KeePass.Plugins.Plugin and has
    /// heavy dependencies on KeePass.exe. This stub provides the static
    /// members that the protocol layer references, so the standalone source
    /// files compile without KeePass. These members are never invoked by the
    /// unit tests — only the non-KeePass code paths are exercised.
    /// </summary>
    internal sealed class KeePassNatMsgExt
    {
        internal static Helper CryptoHelper;
        internal static KeePassNatMsgExt ExtInstance;

        public static string GetVersion()
        {
            return "2.7.0";
        }

        internal string GetDbHashForMessage()
        {
            return "";
        }
    }
}
