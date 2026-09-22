using KeePass.App.Configuration;
using System.ComponentModel;

namespace KeePassNatMsg
{
    public enum AllowSearchDatabase
    {
        [Description("Target database for search")]
        SearchInOnlySelectedDatabase,
        SearchInAllOpenedDatabases,
        RestrictSearchInSpecificDatabase
    }

    public class ConfigOpt
    {
        private readonly AceCustomConfig _config;

        const string ReceiveCredentialNotificationKey = "KeePassHttp_ReceiveCredentialNotification";
        const string SpecificMatchingOnlyKey = "KeePassHttp_SpecificMatchingOnly";
        const string UnlockDatabaseRequestKey = "KeePassHttp_UnlockDatabaseRequest";
        const string AlwaysAllowAccessKey = "KeePassHttp_AlwaysAllowAccess";
        const string AlwaysAllowUpdatesKey = "KeePassHttp_AlwaysAllowUpdates";
        const string AllowSearchDatabaseKey = "KeePassHttp_AllowSearchDatabase";
        const string SearchDatabaseHashKey = "KeePassHttp_SearchDatabaseHash";
        const string HideExpiredKey = "KeePassHttp_HideExpired";
        const string MatchSchemesKey = "KeePassHttp_MatchSchemes";
        const string SortResultByUsernameKey = "KeePassHttp_SortResultByUsername";
        const string ConnectionDatabaseHashKey = "KeePassHttp_ConnectionDatabaseHash";
        const string SearchUrlsKey = "KeePassHttp_SearchUrls";
        const string UseKeePassXcSettingsKey = "KeePassNatMsg_UseKeePassXcSettings";
        const string DefaultGroupKey = "KeePassNatMsg_DefaultGroup";
        const string DefaultGroupAlwaysAllowKey = "KeePassNatMsg_DefaultGroupAlwaysAllow";

        public ConfigOpt(AceCustomConfig config)
        {
            _config = config;
        }

        public bool ReceiveCredentialNotification
        {
            get { return _config.GetBool(ReceiveCredentialNotificationKey, false); }
            set { _config.SetBool(ReceiveCredentialNotificationKey, value); }
        }

        public bool UnlockDatabaseRequest
        {
            get { return _config.GetBool(UnlockDatabaseRequestKey, false); }
            set { _config.SetBool(UnlockDatabaseRequestKey, value); }
        }

        public bool SpecificMatchingOnly
        {
            get { return _config.GetBool(SpecificMatchingOnlyKey, false); }
            set { _config.SetBool(SpecificMatchingOnlyKey, value); }
        }

        public bool AlwaysAllowAccess
        {
            get { return _config.GetBool(AlwaysAllowAccessKey, false); }
            set { _config.SetBool(AlwaysAllowAccessKey, value); }
        }

        public bool AlwaysAllowUpdates
        {
            get { return _config.GetBool(AlwaysAllowUpdatesKey, false); }
            set { _config.SetBool(AlwaysAllowUpdatesKey, value); }
        }

        public ulong AllowSearchDatabase
        {
            get { return _config.GetULong(AllowSearchDatabaseKey, 0); }
            set { _config.SetULong(AllowSearchDatabaseKey, value); }
        }

        public string SearchDatabaseHash
        {
            get { return _config.GetString(SearchDatabaseHashKey, string.Empty); }
            set { _config.SetString(SearchDatabaseHashKey, value); }
        }

        public bool HideExpired
        {
            get { return _config.GetBool(HideExpiredKey, true); }
            set { _config.SetBool(HideExpiredKey, value); }
        }

        public bool MatchSchemes
        {
            get { return _config.GetBool(MatchSchemesKey, false); }
            set { _config.SetBool(MatchSchemesKey, value); }
        }

        public bool SortResultByUsername
        {
            get { return _config.GetBool(SortResultByUsernameKey, true); }
            set { _config.SetBool(SortResultByUsernameKey, value); }
        }

        public string ConnectionDatabaseHash
        {
            get { return _config.GetString(ConnectionDatabaseHashKey, string.Empty); }
            set { _config.SetString(ConnectionDatabaseHashKey, value); }
        }

        public bool SearchUrls
        {
            get { return _config.GetBool(SearchUrlsKey, true); }
            set { _config.SetBool(SearchUrlsKey, value); }
        }

        public bool UseKeePassXcSettings
        {
            get { return _config.GetBool(UseKeePassXcSettingsKey, false); }
            set { _config.SetBool(UseKeePassXcSettingsKey, value); }
        }

        public string DefaultGroup
        {
            get { return _config.GetString(DefaultGroupKey, string.Empty); }
            set { _config.SetString(DefaultGroupKey, value); }
        }

        public bool DefaultGroupAlwaysAllow
        {
            get { return _config.GetBool(DefaultGroupAlwaysAllowKey, false); }
            set { _config.SetBool(DefaultGroupAlwaysAllowKey, value); }
        }
    }
}
