using KeePass.Plugins;
using KeePassLib;
using KeePassNatMsg.Entry;
using KeePassNatMsg.Protocol.Action;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KeePassNatMsg.Protocol
{
    public sealed class Handlers
    {
        private KeePassNatMsgExt _ext;
        private Dictionary<string, RequestHandler> _handlers;
        private IPluginHost _host;
        private object _unlockLock;

        public delegate Response RequestHandler(Request req);

        public Handlers()
        {
            _ext = KeePassNatMsgExt.ExtInstance;
            _host = KeePassNatMsgExt.HostInstance;
            _unlockLock = new object();
        }

        public void Initialize()
        {
            _handlers = new Dictionary<string, RequestHandler>
            {
                {Actions.GET_DATABASE_HASH, GetDatabaseHash},
                {Actions.TEST_ASSOCIATE, TestAssociate},
                {Actions.ASSOCIATE, Associate},
                {Actions.CHANGE_PUBLIC_KEYS, ChangePublicKeys},
                {Actions.GET_LOGINS, GetLogins},
                {Actions.GET_LOGINS_COUNT, GetLoginsCount},
                {Actions.SET_LOGIN, SetLogin},
                {Actions.GENERATE_PASSWORD, GeneratePassword},
                {Actions.LOCK_DATABASE, LockDatabase},
                {Actions.GET_DATABASE_GROUPS, GetDatabaseGroups},
                {Actions.CREATE_NEW_GROUP, CreateNewGroup},
                {Actions.GET_TOTP, GetTotp},
                {Actions.REQUEST_AUTOTYPE, RequestAutoType},
            };
        }

        public Response ProcessRequest(Request req)
        {
            var handler = GetHandler(req.Action);
            if (handler != null)
            {
                if (handler != ChangePublicKeys && !UnlockDatabase(req.TriggerUnlock))
                {
                    return new ErrorResponse(req, ErrorType.DatabaseNotOpened);
                }

                return handler.Invoke(req);
            }
            return new ErrorResponse(req, ErrorType.IncorrectAction);
        }

        private bool UnlockDatabase(bool triggerUnlock)
        {
            lock (_unlockLock)
            {
                var config = new ConfigOpt(_host.CustomConfig);
                if (!_host.Database.IsOpen && config.UnlockDatabaseRequest && KeePass.UI.GlobalWindowManager.WindowCount == 0 && triggerUnlock)
                {
                    _host.MainWindow.Invoke(new System.Action(() => _host.MainWindow.OpenDatabase(_host.MainWindow.DocumentManager.ActiveDocument.LockedIoc, null, false)));
                }

                return _host.Database.IsOpen;
            }
        }

        private RequestHandler GetHandler(string action)
        {
            return _handlers.ContainsKey(action) ? _handlers[action] : null;
        }

        private Response GetDatabaseHash(Request req)
        {
            if (req.TryDecrypt())
            {
                return req.GetResponse();
            }
            return new ErrorResponse(req, ErrorType.CannotDecryptMessage);
        }

        private Response TestAssociate(Request req)
        {
            if (req.TryDecrypt())
            {
                var db = _ext.GetConnectionDatabase();
                var msg = req.Message;
                var customKey = KeePassNatMsgExt.DbKey + msg.GetString("id");
                if (!db.CustomData.Exists(customKey))
                    return new ErrorResponse(req, ErrorType.AssociationFailed);
                var key = db.CustomData.Get(customKey);
                var reqKey = msg.GetBytes("key");
                var id = msg.GetString("id");
                var dbKey = Convert.FromBase64String(key);
                if (dbKey.SequenceEqual(reqKey) && !string.IsNullOrWhiteSpace(id))
                {
                    var resp = req.GetResponse();
                    resp.Message.Add("id", id);
                    return resp;
                }
                return new ErrorResponse(req, ErrorType.AssociationFailed);
            }
            return new ErrorResponse(req, ErrorType.CannotDecryptMessage);
        }

        private Response Associate(Request req)
        {
            if (req.TryDecrypt())
            {
                var msg = req.Message;
                var keyBytes = msg.GetBytes("key");
                if (keyBytes.SequenceEqual(KeePassNatMsgExt.CryptoHelper.ClientPublicKey(req.ClientId)))
                {
                    var id = _ext.ShowConfirmAssociationDialog(msg.GetString("idKey"));
                    if (string.IsNullOrEmpty(id))
                    {
                        return new ErrorResponse(req, ErrorType.AssociationFailed);
                    }
                    var resp = req.GetResponse();
                    resp.Message.Add("id", id);
                    return resp;
                }
                else
                {
                    _ext.ShowNotification("Association Failed. Public Keys don't match.");
                    return new ErrorResponse(req, ErrorType.AssociationFailed);
                }
            }
            return new ErrorResponse(req, ErrorType.CannotDecryptMessage);
        }

        private Response ChangePublicKeys(Request req)
        {
            var crypto = KeePassNatMsgExt.CryptoHelper;
            var publicKey = req.GetString("publicKey");

            if (string.IsNullOrEmpty(publicKey))
                return new ErrorResponse(req, ErrorType.ClientPublicKeyNotReceived);

            var serverPublicKey = crypto.GenerateKeyPair(req.ClientId, Convert.FromBase64String(publicKey));
            var resp = req.GetResponse(false);
            resp.AddBytes("publicKey", serverPublicKey);
            resp.Add("version", KeePassNatMsgExt.GetVersion());
            resp.Add("success", "true");
            return resp;
        }

        private Response GetLogins(Request req)
        {
            var es = new EntrySearch();
            return es.GetLoginsHandler(req);
        }

        private Response SetLogin(Request req)
        {
            if (req.TryDecrypt())
            {
                var eu = new EntryUpdate();
                var reqMsg = req.Message;
                var url = reqMsg.GetString("url");
                var uuid = reqMsg.GetString("uuid");
                var login = reqMsg.GetString("login");
                var pw = reqMsg.GetString("password");
                var submitUrl = reqMsg.GetString("submitUrl");
                var groupUuid = reqMsg.GetString("groupUuid");
                var group = reqMsg.GetString("group");
                var downloadFavicon = reqMsg.GetString("downloadFavicon");

                bool result;

                if (string.IsNullOrEmpty(uuid))
                {
                    // Create new entry - pass group name if provided
                    result = eu.CreateEntry(login, pw, url, submitUrl, null, groupUuid);
                }
                else
                {
                    result = eu.UpdateEntry(uuid, login, pw, url);
                }

                var resp = req.GetResponse();

                resp.Message.Add("count", JValue.CreateNull());
                resp.Message.Add("entries", JValue.CreateNull());
                resp.Message.Add("error", result ? "success" : "error");
                resp.Message.Add("success", result ? "true" : "false");

                return resp;
            }
            return new ErrorResponse(req, ErrorType.CannotDecryptMessage);
        }

        private Response GeneratePassword(Request req)
        {
            var resp = req.GetResponse();
            var msg = resp.Message;
            // KeePassXC-Browser 1.10.4 expects a "password" field directly in the message
            // (changed from the older "entries" array format)
            var pw = _ext.GeneratePassword();
            if (pw != null)
            {
                msg.Add("password", pw["password"]);
            }
            return resp;
        }

        private Response LockDatabase(Request req)
        {

            _host.MainWindow.Invoke(new System.Action(() => _host.MainWindow.LockAllDocuments()));
            return req.GetResponse();
        }

        private Response GetDatabaseGroups(Request req)
        {
            if (!req.TryDecrypt())
                return new ErrorResponse(req, ErrorType.CannotDecryptMessage);

            var db = _ext.GetConnectionDatabase();

            if (db.RootGroup == null)
            {
                return new ErrorResponse(req, ErrorType.NoGroupsFound);
            }

            var root = new JObject
            {
                { "name", db.RootGroup.Name },
                { "uuid", db.RootGroup.Uuid.ToHexString() },
                { "children", GetGroupChildren(db.RootGroup) }
            };

            var resp = req.GetResponse();

            // KeePassXC-Browser 1.10.4 expects defaultGroup and defaultGroupAlwaysAllow
            var configOpt = new ConfigOpt(_host.CustomConfig);
            var defaultGroup = configOpt.DefaultGroup;
            var defaultGroupAlwaysAllow = configOpt.DefaultGroupAlwaysAllow;

            var groups = new JObject
            {
                { "groups", new JArray { root } }
            };
            groups.Add("defaultGroup", string.IsNullOrEmpty(defaultGroup) ? "" : defaultGroup);
            groups.Add("defaultGroupAlwaysAllow", defaultGroupAlwaysAllow ? "true" : "false");

            resp.Message.Add("groups", groups);

            return resp;
        }

        private JArray GetGroupChildren(PwGroup group)
        {
            var groups = new JArray();

            foreach(var grp in group.GetGroups(false))
            {
                groups.Add(new JObject
                {
                    { "name", grp.Name },
                    { "uuid", grp.Uuid.ToHexString() },
                    { "children", GetGroupChildren(grp) }
                });
            }

            return groups;
        }

        private Response CreateNewGroup(Request req)
        {
            if (!req.TryDecrypt())
                return new ErrorResponse(req, ErrorType.CannotDecryptMessage);

            var groupName = req.Message.GetString("groupName");

            var db = _ext.GetConnectionDatabase();

            var group = db.RootGroup.FindCreateSubTree(groupName, new[] { '/' }, true);

            if (group == null)
                return new ErrorResponse(req, ErrorType.CannotCreateNewGroup);

            var resp = req.GetResponse();

            resp.Message.Add("name", group.Name);
            resp.Message.Add("uuid", group.Uuid.ToHexString());

            return resp;
        }

        private Response GetTotp(Request req)
        {
            if (!req.TryDecrypt())
                return new ErrorResponse(req, ErrorType.CannotDecryptMessage);

            var uuid = req.Message.GetString("uuid");

            var es = new EntrySearch();
            var totp = es.GetTotp(uuid);

            if (string.IsNullOrEmpty(totp))
                return new ErrorResponse(req, ErrorType.NoLoginsFound);

            var resp = req.GetResponse();

            resp.Message.Add("totp", totp);

            return resp;
        }

        private Response GetLoginsCount(Request req)
        {
            if (!req.TryDecrypt())
                return new ErrorResponse(req, ErrorType.CannotDecryptMessage);

            var msg = req.Message;
            var url = msg.GetString("url");

            if (string.IsNullOrEmpty(url))
                return new ErrorResponse(req, ErrorType.NoUrlProvided);

            var es = new EntrySearch();
            var items = es.FindMatchingEntries(url, null);

            var resp = req.GetResponse();
            resp.Message.Add("count", items.Count());

            return resp;
        }

        private Response RequestAutoType(Request req)
        {
            if (!req.TryDecrypt())
                return new ErrorResponse(req, ErrorType.CannotDecryptMessage);

            var search = req.Message.GetString("search");

            if (string.IsNullOrEmpty(search))
                return new ErrorResponse(req, ErrorType.NoUrlProvided);

            // Perform Global Auto-Type with the search term
            _host.MainWindow.Invoke(new System.Action(() =>
            {
                try
                {
                    // Use KeePass's built-in global auto-type with the search query
                    var autoType = _host.MainWindow;
                    // KeePass 2.x supports sending auto-type sequences via command line
                    // or via the MainForm's ExecuteGlobalAutoType method
                    // We use the search term to perform a targeted auto-type
                    KeePass.Util.AutoType.PerformIntoPreviousWindow(
                        _host.MainWindow,
                        _host.Database,
                        search);
                }
                catch (Exception)
                {
                    // Auto-type may fail if the target window is not available
                }
            }));

            var resp = req.GetResponse();
            return resp;
        }
    }
}
