using System;
using System.Collections.Generic;

namespace KeePassNatMsg.Entry
{
    public class EntryConfig
    {
        public HashSet<string> Allow = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> Deny = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public string Realm = null;
    }
}
