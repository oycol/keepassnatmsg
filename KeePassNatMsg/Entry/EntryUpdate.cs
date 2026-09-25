using KeePass.Plugins;
using KeePass.UI;
using KeePassLib;
using KeePassLib.Collections;
using KeePassLib.Security;
using KeePassLib.Utility;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace KeePassNatMsg.Entry
{
    public sealed class EntryUpdate
    {
        private IPluginHost _host;
        private KeePassNatMsgExt _ext;

        public EntryUpdate()
        {
            _host = KeePassNatMsgExt.HostInstance;
            _ext = KeePassNatMsgExt.ExtInstance;
        }

        public bool UpdateEntry(string uuid, string username, string password, string formHost)
        {
            Uri requestUri;
            if (string.IsNullOrEmpty(uuid) || string.IsNullOrEmpty(formHost) ||
                !Uri.TryCreate(formHost, UriKind.Absolute, out requestUri) ||
                !UrlMatchingHelper.DefaultAllowedSchemes.Contains(requestUri.Scheme.ToLowerInvariant()))
                return false;

            PwEntry entry = null;
            PwUuid id;
            try { id = new PwUuid(MemUtil.HexStringToByteArray(uuid)); }
            catch (Exception) { return false; }
            PwDatabase db = null;
            bool ambiguous = false;
            var configOpt = new ConfigOpt(_host.CustomConfig);
            if (configOpt.AllowSearchDatabase == (ulong)AllowSearchDatabase.SearchInAllOpenedDatabases)
            {
                foreach (PwDocument doc in _host.MainWindow.DocumentManager.Documents)
                {
                    if (doc.Database != null && doc.Database.IsOpen && doc.Database.RootGroup != null)
                    {
                        var found = doc.Database.RootGroup.FindEntry(id, true);
                        if (found != null)
                        {
                            if (entry != null && !object.ReferenceEquals(entry, found)) ambiguous = true;
                            entry = found;
                            db = doc.Database;
                        }
                    }
                }
            }
            else if (configOpt.AllowSearchDatabase == (ulong)AllowSearchDatabase.RestrictSearchInSpecificDatabase)
            {
                db = _ext.GetSearchDatabase();
                if (db == null || !db.IsOpen || db.RootGroup == null) return false;
                entry = db.RootGroup.FindEntry(id, true);
            }
            else
            {
                db = _host.Database;
                if (db == null || !db.IsOpen || db.RootGroup == null) return false;
                entry = db.RootGroup.FindEntry(id, true);
            }

            if (entry == null || ambiguous)
            {
                return false;
            }

            if (entry.Expires && entry.ExpiryTime <= DateTime.UtcNow && configOpt.HideExpired) return false;
            if (!EntrySearch.GetEntryUrls(entry, configOpt.SearchUrls).Any(url =>
                UrlMatchingHelper.MatchesUrl(url, requestUri.Host, requestUri.Scheme,
                    configOpt.MatchSchemes, configOpt.SpecificMatchingOnly))) return false;
            var entryConfig = _ext.GetEntryConfig(entry);
            if (entryConfig != null && (entryConfig.Deny.Contains(requestUri.Authority) ||
                entryConfig.Deny.Contains(requestUri.Host))) return false;

            string[] up = _ext.GetUserPass(new PwEntryDatabase(entry, db));
            var u = up[0];
            var p = up[1];

            if (u != username || p != password)
            {
                bool allowUpdate = configOpt.AlwaysAllowUpdates;

                if (!allowUpdate)
                {
                    _host.MainWindow.Activate();

                    DialogResult result;
                    if (_host.MainWindow.IsTrayed())
                    {
                        result = MessageBox.Show(
                            String.Format("Do you want to update the information in {0} - {1}?", formHost, u),
                            "Update Entry", MessageBoxButtons.YesNo,
                            MessageBoxIcon.None, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    }
                    else
                    {
                        result = MessageBox.Show(
                            _host.MainWindow,
                            String.Format("Do you want to update the information in {0} - {1}?", formHost, u),
                            "Update Entry", MessageBoxButtons.YesNo,
                            MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                    }


                    if (result == DialogResult.Yes)
                    {
                        allowUpdate = true;
                    }
                }

                if (allowUpdate)
                {
                    PwObjectList<PwEntry> m_vHistory = entry.History.CloneDeep();
                    entry.History = m_vHistory;
                    entry.CreateBackup(null);

                    entry.Strings.Set(PwDefs.UserNameField, new ProtectedString(false, username));
                    entry.Strings.Set(PwDefs.PasswordField, new ProtectedString(true, password));
                    entry.Touch(true, false);
                    _ext.UpdateUI(entry.ParentGroup);

                    AutoSaveIfRequired(db);

                    return true;
                }
            }

            return false;
        }

        private void AutoSaveIfRequired(PwDatabase db)
        {
            if (!KeePass.Program.Config.Application.AutoSaveAfterEntryEdit) return;
            _host.MainWindow.Invoke(new MethodInvoker(() =>
            { //different thread access UI elements
                KeePassNatMsgExt.HostInstance.MainWindow.SaveDatabase(db, null);
            }));
        }

        public bool CreateEntry(string username, string password, string url, string submithost, string realm, string groupUuid)
        {
            // Validate url before doing anything else.
            Uri uri;
            if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out uri))
                return false;

            // Store only the origin+path prefix (drop query/fragment) so the saved URL
            // is still useful as a login URL without leaking search parameters.
            // We keep the path up to the last '/' (trimming only the file part if any).
            string baseUrl = url;
            var lastSlash = url.LastIndexOf('/');
            // lastSlash > (scheme + "://").Length  →  there is a real path segment
            var schemeEnd = url.IndexOf("://", StringComparison.Ordinal);
            var pathStart = schemeEnd >= 0 ? schemeEnd + 3 : 0;
            if (lastSlash > pathStart)
            {
                baseUrl = url.Substring(0, lastSlash + 1);
            }

            var connectionDb = _ext.GetConnectionDatabase();
            if (connectionDb == null || !connectionDb.IsOpen || connectionDb.RootGroup == null) return false;

            PwEntry entry = new PwEntry(true, true);
            entry.Strings.Set(PwDefs.TitleField, new KeePassLib.Security.ProtectedString(false, uri.Host));
            entry.Strings.Set(PwDefs.UserNameField, new KeePassLib.Security.ProtectedString(false, username));
            entry.Strings.Set(PwDefs.PasswordField, new KeePassLib.Security.ProtectedString(true, password));
            entry.Strings.Set(PwDefs.UrlField, new KeePassLib.Security.ProtectedString(false, baseUrl));

            if ((submithost != null && uri.Host != submithost) || realm != null)
            {
                var config = new EntryConfig();
                if (submithost != null)
                    config.Allow.Add(submithost);
                if (realm != null)
                    config.Realm = realm;

                _ext.SetEntryConfig(entry, config);
            }

            PwGroup group = null;

            if (!string.IsNullOrEmpty(groupUuid))
            {
                var db = connectionDb;
                if (db.RootGroup != null)
                {
                    var uuid = new PwUuid(KeePassLib.Utility.MemUtil.HexStringToByteArray(groupUuid));
                    group = db.RootGroup.FindGroup(uuid, true);
                }
            }

            if (group == null)
            {
                // A UUID that does not belong to the configured connection database
                // must not silently redirect a create operation to the default group.
                if (!string.IsNullOrEmpty(groupUuid)) return false;
                group = _ext.GetPasswordsGroup();
            }
            if (group == null) return false;

            group.AddEntry(entry, true);
            _ext.UpdateUI(group);

            AutoSaveIfRequired(connectionDb);

            return true;
        }
    }
}
