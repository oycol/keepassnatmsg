using KeePass.Plugins;
using KeePass.UI;
using KeePass.Util.Spr;
using KeePassNatMsg.Protocol;
using KeePassNatMsg.Protocol.Action;
using KeePassLib;
using KeePassLib.Collections;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using KeePassLib.Utility;
using KeePassLib.Delegates;

namespace KeePassNatMsg.Entry
{
    public sealed class EntrySearch
    {
        private const string TotpKey = "TimeOtp-Secret";
        private const string TotpPlaceholder = "{TIMEOTP}";
        private const string TotpLegacyPlaceholder = "{TOTP}";

        private readonly IPluginHost _host;
        private readonly KeePassNatMsgExt _ext;

        public EntrySearch()
        {
            _host = KeePassNatMsgExt.HostInstance;
            _ext = KeePassNatMsgExt.ExtInstance;
        }

        internal Response GetLoginsHandler(Request req)
        {
            if (!req.TryDecrypt()) return new ErrorResponse(req, ErrorType.CannotDecryptMessage);

            var msg = req.Message;
            var id = msg.GetString("id");
            var url = msg.GetString("url");
            var submitUrl = msg.GetString("submitUrl");

            Uri hostUri;
            Uri submitUri = null;

            if (!string.IsNullOrEmpty(url))
            {
                if (!Uri.TryCreate(url, UriKind.Absolute, out hostUri) ||
                    !UrlMatchingHelper.DefaultAllowedSchemes.Contains(hostUri.Scheme.ToLowerInvariant()))
                {
                    return new ErrorResponse(req, ErrorType.NoUrlProvided);
                }
            }
            else
            {
                return new ErrorResponse(req, ErrorType.NoUrlProvided);
            }

            if (!string.IsNullOrEmpty(submitUrl))
            {
                if (Uri.TryCreate(submitUrl, UriKind.Absolute, out submitUri))
                {
                    if (!UrlMatchingHelper.DefaultAllowedSchemes.Contains(submitUri.Scheme.ToLowerInvariant()) ||
                        string.IsNullOrEmpty(submitUri.Authority))
                    {
                        submitUri = null;
                    }
                }
                else
                {
                    submitUri = null;
                }
            }

            var resp = req.GetResponse();
            resp.Message.Add("id", id);

            var items = FindMatchingEntries(hostUri, null).ToList();
            if (items.Count > 0)
            {
                var configOpt = new ConfigOpt(_host.CustomConfig);

                var filter = new GFunc<PwEntry, bool>((PwEntry e) =>
                {
                    var c = _ext.GetEntryConfig(e);
                    if (c == null) return true;

                    if (!c.Allow.Contains(hostUri.Authority)) return true;
                    if (submitUri != null && !c.Allow.Contains(submitUri.Authority)) return true;

                    return false;
                });

                var needPrompting = items.Where(e => filter(e.entry)).ToList();

                if (needPrompting.Count > 0 && !configOpt.AlwaysAllowAccess)
                {
                    var win = _host.MainWindow;

                    using (var f = new AccessControlForm())
                    {
                        win.Invoke((MethodInvoker)delegate
                        {
                            f.Icon = win.Icon;
                            f.Plugin = _ext;
                            f.StartPosition = win.Visible ? FormStartPosition.CenterParent : FormStartPosition.CenterScreen;
                            f.Entries = needPrompting.Select(e => e.entry).ToList();
                            f.Host = submitUri != null ? submitUri.Authority : hostUri.Authority;
                            f.Load += delegate { f.Activate(); };
                            f.ShowDialog(win);
                            if (f.Remember && (f.Allowed || f.Denied))
                            {
                                foreach (var e in needPrompting)
                                {
                                    var c = _ext.GetEntryConfig(e.entry) ?? new EntryConfig();
                                    var set = f.Allowed ? c.Allow : c.Deny;
                                    set.Add(hostUri.Authority);
                                    if (submitUri != null && !string.Equals(submitUri.Authority, hostUri.Authority, StringComparison.OrdinalIgnoreCase))
                                        set.Add(submitUri.Authority);
                                    _ext.SetEntryConfig(e.entry, c);
                                }
                            }
                            if (!f.Allowed)
                            {
                                items = items.Except(needPrompting).ToList();
                            }
                        });
                    }
                }

                var itemsList = items.ToList();

                if (configOpt.SortResultByUsername)
                {
                    itemsList = itemsList.OrderBy(e => _ext.GetUserPass(e)[0], StringComparer.CurrentCultureIgnoreCase).ToList();
                }
                else
                {
                    itemsList = itemsList.OrderBy(e => e.entry.Strings.ReadSafe(PwDefs.TitleField), StringComparer.CurrentCultureIgnoreCase).ToList();
                }

                var entries = new JArray(itemsList.Select(item =>
                {
                    var up = _ext.GetUserPass(item);
                    JArray fldArr = null;
                    var fields = GetFields(configOpt, item);
                    if (fields != null)
                    {
                        fldArr = new JArray(fields.Select(f => new JObject { { f.Key, f.Value } }));
                    }
                    var jobj = new JObject {
                        { "name", item.entry.Strings.ReadSafe(PwDefs.TitleField) },
                        { "login", up[0] },
                        { "password", up[1] },
                        { "uuid", item.entry.Uuid.ToHexString() },
                        { "stringFields", fldArr },
                    };

                    CheckTotp(item, jobj);

                    return jobj;
                }));

                resp.Message.Add("count", itemsList.Count);
                resp.Message.Add("entries", entries);

                if (itemsList.Count > 0)
                {
                    var names = (from e in itemsList select e.entry.Strings.ReadSafe(PwDefs.TitleField)).Distinct();
                    var n = String.Join("\n    ", names);

                    if (configOpt.ReceiveCredentialNotification)
                        _ext.ShowNotification(String.Format("{0}: {1} is receiving credentials for:\n    {2}", req.GetString("id"), hostUri.Host, n));
                }

                return resp;
            }

            resp.Message.Add("count", 0);
            resp.Message.Add("entries", new JArray());

            return resp;
        }

        internal int CountMatchingEntries(string url)
        {
            if (string.IsNullOrEmpty(url)) return 0;
            Uri uri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri)) return 0;
            if (!UrlMatchingHelper.DefaultAllowedSchemes.Contains(uri.Scheme.ToLowerInvariant())) return 0;

            return FindMatchingEntries(uri, null).Count();
        }

        private void CheckTotp(PwEntryDatabase item, JObject obj)
        {
            var totp = GetTotpFromEntry(item);
            if (!string.IsNullOrEmpty(totp))
            {
                obj.Add("totp", totp);
            }
        }

        internal string GetTotp(string uuid)
        {
            var dbEntry = FindEntry(uuid);

            if (dbEntry == null)
                return null;

            return GetTotpFromEntry(dbEntry);
        }

        private string GetTotpFromEntry(PwEntryDatabase item)
        {
            string totp = null;

            if (HasLegacyTotp(item.entry))
            {
                totp = GenerateTotp(item, TotpLegacyPlaceholder);
            }

            if (string.IsNullOrEmpty(totp) && HasTotp(item.entry))
            {
                totp = GenerateTotp(item, TotpPlaceholder);
            }
            return totp;
        }

        private string GenerateTotp(PwEntryDatabase item, string placeholder)
        {
            var ctx = new SprContext(item.entry, item.database, SprCompileFlags.All, false, false);
            return SprEngine.Compile(placeholder, ctx);
        }

        private static bool HasTotp(PwEntry entry)
        {
            return entry.Strings.Any(x => x.Key.StartsWith(TotpKey));
        }

        private static bool HasLegacyTotp(PwEntry entry)
        {
            return entry.Strings.Any(x =>
            x.Key.Equals("otp", StringComparison.InvariantCultureIgnoreCase) ||
            x.Key.Equals("TOTP Seed", StringComparison.InvariantCultureIgnoreCase));
        }

        private PwEntryDatabase FindEntry(string uuid)
        {
            PwUuid id = new PwUuid(MemUtil.HexStringToByteArray(uuid));

            var configOpt = new ConfigOpt(_host.CustomConfig);

            if (configOpt.AllowSearchDatabase == (ulong)AllowSearchDatabase.SearchInAllOpenedDatabases)
            {
                foreach (var doc in _host.MainWindow.DocumentManager.Documents)
                {
                    if (doc.Database.IsOpen)
                    {
                        var entry = doc.Database.RootGroup.FindEntry(id, true);
                        if (entry != null)
                            return new PwEntryDatabase(entry, doc.Database);
                    }
                }
            }
            else if (configOpt.AllowSearchDatabase == (ulong)AllowSearchDatabase.RestrictSearchInSpecificDatabase)
            {
                var entry = _ext.GetSearchDatabase().RootGroup.FindEntry(id, true);
                if (entry != null)
                    return new PwEntryDatabase(entry, _ext.GetSearchDatabase());
            }
            else
            {
                var entry = _host.Database.RootGroup.FindEntry(id, true);
                if (entry != null)
                    return new PwEntryDatabase(entry, _host.Database);
            }

            return null;
        }

        private static IEnumerable<KeyValuePair<string, string>> GetFields(ConfigOpt configOpt, PwEntryDatabase entryDatabase)
        {
            SprContext ctx = new SprContext(entryDatabase.entry, entryDatabase.database, SprCompileFlags.All, false, false);
            var fields = new List<KeyValuePair<string, string>>();

            var standardFields = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
            {
                PwDefs.TitleField, PwDefs.UserNameField, PwDefs.PasswordField, PwDefs.UrlField, PwDefs.NotesField
            };

            foreach (var sf in entryDatabase.entry.Strings)
            {
                if (standardFields.Contains(sf.Key)) continue;

                var sfValue = entryDatabase.entry.Strings.ReadSafe(sf.Key);
                sfValue = SprEngine.Compile(sfValue, ctx);

                var key = sf.Key.StartsWith("KPH: ", StringComparison.OrdinalIgnoreCase) ? sf.Key.Substring(5) : sf.Key;
                fields.Add(new KeyValuePair<string, string>(key, sfValue));
            }

            if (fields.Count > 0)
            {
                return fields.OrderBy(e2 => e2.Key, StringComparer.OrdinalIgnoreCase).ToList();
            }
            return null;
        }

        private IEnumerable<PwEntryDatabase> FindMatchingEntries(Uri hostUri, string realm)
        {
            var formHost = hostUri.Host;
            var requestScheme = hostUri.Scheme;

            List<PwDatabase> listDatabases = new List<PwDatabase>();
            var configOpt = new ConfigOpt(_host.CustomConfig);
            if (configOpt.AllowSearchDatabase == (ulong)AllowSearchDatabase.SearchInAllOpenedDatabases)
            {
                foreach (PwDocument doc in _host.MainWindow.DocumentManager.Documents)
                {
                    if (doc.Database.IsOpen)
                    {
                        listDatabases.Add(doc.Database);
                    }
                }
            }
            else if (configOpt.AllowSearchDatabase == (ulong)AllowSearchDatabase.RestrictSearchInSpecificDatabase)
            {
                listDatabases.Add(_ext.GetSearchDatabase());
            }
            else
            {
                listDatabases.Add(_host.Database);
            }

            var parms = MakeSearchParameters();
            var searchUrls = configOpt.SearchUrls;
            var matchSchemes = configOpt.MatchSchemes;
            var exactHostOnly = configOpt.SpecificMatchingOnly;
            var formAuthority = hostUri.Authority;
            var candidates = new Dictionary<string, PwEntryDatabase>();

            foreach (PwDatabase db in listDatabases)
            {
                if (db == null || !db.IsOpen || db.RootGroup == null)
                    continue;

                foreach (var searchHost in UrlMatchingHelper.GetSearchHosts(formHost))
                {
                    if (matchSchemes)
                    {
                        parms.SearchString = string.Format("^{0}$|{1}://{0}/?", searchHost, requestScheme);
                    }
                    else
                    {
                        parms.SearchString = string.Format("^{0}$|/{0}/?", searchHost);
                    }

                    var listEntries = new PwObjectList<PwEntry>();
                    db.RootGroup.SearchEntries(parms, listEntries);
                    foreach (var entry in listEntries)
                    {
                        var key = entry.Uuid.ToHexString();
                        if (!candidates.ContainsKey(key))
                        {
                            candidates.Add(key, new PwEntryDatabase(entry, db));
                        }
                    }
                }

                AddRegexAndUrlCandidates(db, candidates, parms.RespectEntrySearchingDisabled, searchUrls);
            }

            var filtered = new List<PwEntryDatabase>();
            foreach (var item in candidates.Values)
            {
                var e = item.entry;
                var c = _ext.GetEntryConfig(e);
                if (c != null)
                {
                    if (c.Deny.Contains(formAuthority) || c.Deny.Contains(formHost))
                        continue;
                    if (!string.IsNullOrEmpty(realm) && !string.Equals(c.Realm, realm, StringComparison.OrdinalIgnoreCase))
                        continue;
                }

                var matched = false;
                foreach (var entryUrl in GetEntryUrls(e, searchUrls))
                {
                    if (UrlMatchingHelper.MatchesUrl(entryUrl, formHost, requestScheme, matchSchemes, exactHostOnly))
                    {
                        matched = true;
                        break;
                    }
                }

                if (matched)
                {
                    filtered.Add(item);
                }
            }

            if (configOpt.HideExpired)
            {
                filtered = filtered.Where(x => !(x.entry.Expires && x.entry.ExpiryTime <= DateTime.UtcNow)).ToList();
            }

            return filtered;
        }

        private void AddRegexAndUrlCandidates(PwDatabase db, Dictionary<string, PwEntryDatabase> candidates, bool bRespectEntrySearchingDisabled, bool includeAdditionalFields)
        {
            var listEntries = db.RootGroup.GetEntries(true).AsEnumerable();
            if (bRespectEntrySearchingDisabled)
            {
                listEntries = listEntries.Where(x => x.GetSearchingEnabled());
            }

            foreach (var entry in listEntries)
            {
                var uuid = entry.Uuid.ToHexString();
                if (candidates.ContainsKey(uuid)) continue;

                var isRegexInPrimary = UrlMatchingHelper.IsRegexUrl(entry.Strings.ReadSafe(PwDefs.UrlField));
                var hasAdditionalUrls = includeAdditionalFields && entry.Strings.Any(x =>
                    UrlMatchingHelper.IsAdditionalUrlField(x.Key) &&
                    UrlMatchingHelper.ParseUrlValues(entry.Strings.ReadSafe(x.Key)).Count > 0);

                if (!isRegexInPrimary && !hasAdditionalUrls)
                {
                    continue;
                }

                candidates.Add(uuid, new PwEntryDatabase(entry, db));
            }
        }

        private static IEnumerable<string> GetEntryUrls(PwEntry entry, bool includeAdditionalFields)
        {
            foreach (var url in UrlMatchingHelper.ParseUrlValues(entry.Strings.ReadSafe(PwDefs.UrlField)))
            {
                yield return url;
            }

            if (!includeAdditionalFields) yield break;

            foreach (var field in entry.Strings.Where(x =>
                !string.Equals(x.Key, PwDefs.UrlField, StringComparison.InvariantCultureIgnoreCase) &&
                UrlMatchingHelper.IsAdditionalUrlField(x.Key)))
            {
                foreach (var url in UrlMatchingHelper.ParseUrlValues(entry.Strings.ReadSafe(field.Key)))
                {
                    yield return url;
                }
            }
        }

        private static SearchParameters MakeSearchParameters()
        {
            // ExcludeExpired is intentionally NOT set here.
            // Expired-entry filtering is applied explicitly in FindMatchingEntries after
            // URL matching, so we retain full control over what "expired" means and avoid
            // double-filtering if KeePass's own behaviour changes between versions.
            return new SearchParameters
            {
                SearchInTitles = false,
                SearchInGroupNames = false,
                SearchInNotes = false,
                SearchInOther = false,
                SearchInPasswords = false,
                SearchInTags = false,
                SearchInUrls = true,
                SearchInUserNames = false,
                SearchInUuids = false,
                ExcludeExpired = false,
                SearchMode = PwSearchMode.Regular,
                ComparisonMode = StringComparison.InvariantCultureIgnoreCase,
            };
        }
    }
}
