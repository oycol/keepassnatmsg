using KeePassLib;
using KeePassNatMsg.NativeMessaging;
using KeePassNatMsg.Utils;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace KeePassNatMsg.Options
{
    public partial class OptionsForm : Form
    {
        readonly ConfigOpt _config;
        private readonly ChromeIntegrationService _chromeService = new ChromeIntegrationService();
        private bool _initialAlwaysAllowAccess;
        private bool _initialAlwaysAllowUpdates;

        private string AssemblyVersion
        {
            get
            {
                try
                {
                    var v = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                    if (v != null)
                    {
                        if (v.Revision <= 0)
                        {
                            return string.Format("{0}.{1}.{2}", v.Major, v.Minor, Math.Max(0, v.Build));
                        }
                        return string.Format("{0}.{1}.{2}.{3}", v.Major, v.Minor, Math.Max(0, v.Build), v.Revision);
                    }
                }
                catch { }

                return "2.4.0";
            }
        }

        public OptionsForm(ConfigOpt config)
        {
            _config = config;
            InitializeComponent();
            lblVersion.Text = string.Format("KeePassNatMsg v{0}", AssemblyVersion);
        }

        private void OptionsForm_Load(object sender, EventArgs e)
        {
            credMatchingCheckbox.Checked = _config.SpecificMatchingOnly;
            unlockDatabaseCheckbox.Checked = _config.UnlockDatabaseRequest;
            credAllowAccessCheckbox.Checked = _config.AlwaysAllowAccess;
            credAllowUpdatesCheckbox.Checked = _config.AlwaysAllowUpdates;
            if (_config.AllowSearchDatabase == (ulong)AllowSearchDatabase.SearchInOnlySelectedDatabase)
                credOnlySearchInSelectedDatabaseRadioButton.Checked = true;
            else if (_config.AllowSearchDatabase == (ulong)AllowSearchDatabase.SearchInAllOpenedDatabases)
                credSearchInAllOpenedDatabasesRadioButton.Checked = true;
            else
                credRestrictSearchInSpecificDatabaseRadioButton.Checked = true;

            comboBoxSearchDatabases.Enabled = credRestrictSearchInSpecificDatabaseRadioButton.Checked;
            hideExpiredCheckbox.Checked = _config.HideExpired;
            matchSchemesCheckbox.Checked = _config.MatchSchemes;
            chkSearchUrls.Checked = _config.SearchUrls;
            _initialAlwaysAllowAccess = credAllowAccessCheckbox.Checked;
            _initialAlwaysAllowUpdates = credAllowUpdatesCheckbox.Checked;

            InitDatabasesDropdown();

            try
            {
                picFormLogo.Image = KeePassNatMsg.Properties.Resources.icon_16;
            }
            catch { }

            foreach (DatabaseItem item in comboBoxSearchDatabases.Items)
            {
                if (item.DbHash == _config.SearchDatabaseHash)
                {
                    comboBoxSearchDatabases.SelectedItem = item;
                }
            }
            foreach (DatabaseItem item in comboBoxDatabases.Items)
            {
                if (item.DbHash == _config.ConnectionDatabaseHash)
                {
                    comboBoxDatabases.SelectedItem = item;
                }
            }

            // Bind tooltip help for both the tip icons and checkboxes
            toolTip.AutoPopDelay = 10000;
            toolTip.InitialDelay = 350;
            toolTip.ReshowDelay = 150;
            toolTip.ShowAlways = true;

            BindTip(credMatchingCheckbox, tipMatching, "Filters out broader domain entries when a more specific path or subdomain matches.");
            BindTip(unlockDatabaseCheckbox, tipUnlock, "Prompts KeePass to request master password unlock if queried while locked.");
            BindTip(hideExpiredCheckbox, tipExpired, "Do not return credentials that have reached their configured expiration date.");
            BindTip(matchSchemesCheckbox, tipSchemes, "Separates HTTP and HTTPS logins. Recommended to prevent leakage to cleartext sites.");
            BindTip(chkSearchUrls, tipSearchUrls, "Also checks custom string attributes (URL1, URL2, KP2A_URL_1) for alternative login URLs.");
            BindTip(credAllowAccessCheckbox, tipAllowAccess, "Bypasses user confirmation when a browser extension queries stored credentials.");
            BindTip(credAllowUpdatesCheckbox, tipAllowUpdates, "Bypasses user confirmation when a browser extension creates or updates stored credentials.");

            // Favicon options initialization
            chkFaviconPrefixUrls.Checked = _config.FaviconPrefixUrls;
            chkFaviconUseTitle.Checked = _config.FaviconUseTitle;
            chkFaviconUpdateModified.Checked = _config.FaviconUpdateModified;

            cmbFaviconMaxIconSize.Items.Clear();
            cmbFaviconMaxIconSize.Items.Add("16x16");
            cmbFaviconMaxIconSize.Items.Add("32x32");
            cmbFaviconMaxIconSize.Items.Add("48x48");
            cmbFaviconMaxIconSize.Items.Add("64x64");
            cmbFaviconMaxIconSize.Items.Add("128x128 (Recommended)");

            int currentSize = _config.FaviconMaxIconSize;
            if (currentSize <= 16) cmbFaviconMaxIconSize.SelectedIndex = 0;
            else if (currentSize <= 32) cmbFaviconMaxIconSize.SelectedIndex = 1;
            else if (currentSize <= 48) cmbFaviconMaxIconSize.SelectedIndex = 2;
            else if (currentSize <= 64) cmbFaviconMaxIconSize.SelectedIndex = 3;
            else cmbFaviconMaxIconSize.SelectedIndex = 4;

            cmbFaviconProvider.Items.Clear();
            Favicon.FaviconProvider[] providers = Favicon.FaviconProvider.GetProviders();
            for (int pi = 0; pi < providers.Length; pi++)
            {
                cmbFaviconProvider.Items.Add(providers[pi]);
                if (string.Equals(providers[pi].Name, _config.FaviconProvider, StringComparison.OrdinalIgnoreCase))
                {
                    cmbFaviconProvider.SelectedIndex = pi;
                }
            }
            if (cmbFaviconProvider.SelectedIndex < 0) cmbFaviconProvider.SelectedIndex = 0;

            txtFaviconCustomUrl.Text = _config.FaviconCustomUrl ?? string.Empty;
            UpdateFaviconCustomUrlState();
            cmbFaviconProvider.SelectedIndexChanged += delegate { UpdateFaviconCustomUrlState(); };
        }

        private void UpdateFaviconCustomUrlState()
        {
            Favicon.FaviconProvider selected = cmbFaviconProvider.SelectedItem as Favicon.FaviconProvider;
            bool isCustom = selected != null && string.Equals(selected.Name, Favicon.FaviconProvider.ProviderCustom, StringComparison.OrdinalIgnoreCase);
            txtFaviconCustomUrl.Enabled = isCustom;
            if (!isCustom && selected != null)
            {
                txtFaviconCustomUrl.Text = selected.UrlTemplate ?? string.Empty;
            }
        }

        private void BindTip(Control ctrl, PictureBox icon, string tipText)
        {
            if (ctrl != null) toolTip.SetToolTip(ctrl, tipText);
            if (icon != null) toolTip.SetToolTip(icon, tipText);
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            if ((!_initialAlwaysAllowAccess && credAllowAccessCheckbox.Checked) ||
                (!_initialAlwaysAllowUpdates && credAllowUpdatesCheckbox.Checked))
            {
                var confirm = MessageBox.Show(
                    this,
                    "You are enabling an option that bypasses KeePass confirmation prompts. This can expose or overwrite credentials without an additional approval dialog.\n\nContinue?",
                    "Confirm Unsafe Setting",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);
                if (confirm != DialogResult.Yes) return;
            }

            _config.SpecificMatchingOnly = credMatchingCheckbox.Checked;
            _config.UnlockDatabaseRequest = unlockDatabaseCheckbox.Checked;
            _config.AlwaysAllowAccess = credAllowAccessCheckbox.Checked;
            _config.AlwaysAllowUpdates = credAllowUpdatesCheckbox.Checked;
            _config.SearchDatabaseHash = (comboBoxSearchDatabases.SelectedItem as DatabaseItem) == null ? null : (comboBoxSearchDatabases.SelectedItem as DatabaseItem).DbHash;
            _config.HideExpired = hideExpiredCheckbox.Checked;
            _config.MatchSchemes = matchSchemesCheckbox.Checked;
            _config.ConnectionDatabaseHash = (comboBoxDatabases.SelectedItem as DatabaseItem) == null ? null : (comboBoxDatabases.SelectedItem as DatabaseItem).DbHash;
            _config.SearchUrls = chkSearchUrls.Checked;

            if (credOnlySearchInSelectedDatabaseRadioButton.Checked)
                _config.AllowSearchDatabase = (ulong)AllowSearchDatabase.SearchInOnlySelectedDatabase;
            else if (credSearchInAllOpenedDatabasesRadioButton.Checked)
                _config.AllowSearchDatabase = (ulong)AllowSearchDatabase.SearchInAllOpenedDatabases;
            else
                _config.AllowSearchDatabase = (ulong)AllowSearchDatabase.RestrictSearchInSpecificDatabase;

            // Save Favicon settings
            _config.FaviconPrefixUrls = chkFaviconPrefixUrls.Checked;
            _config.FaviconUseTitle = chkFaviconUseTitle.Checked;
            _config.FaviconUpdateModified = chkFaviconUpdateModified.Checked;

            int selectedSizeIdx = cmbFaviconMaxIconSize.SelectedIndex;
            int saveSize = 128;
            if (selectedSizeIdx == 0) saveSize = 16;
            else if (selectedSizeIdx == 1) saveSize = 32;
            else if (selectedSizeIdx == 2) saveSize = 48;
            else if (selectedSizeIdx == 3) saveSize = 64;
            else saveSize = 128;
            _config.FaviconMaxIconSize = saveSize;

            Favicon.FaviconProvider selectedProvider = cmbFaviconProvider.SelectedItem as Favicon.FaviconProvider;
            if (selectedProvider != null)
            {
                _config.FaviconProvider = selectedProvider.Name;
            }
            _config.FaviconCustomUrl = txtFaviconCustomUrl.Text != null ? txtFaviconCustomUrl.Text.Trim() : string.Empty;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void removePermissionsButton_Click(object sender, EventArgs e)
        {
            var confirm = MessageBox.Show(
                this,
                "Remove all saved per-entry browser access decisions from the active database?\n\nThis does not delete entries or browser associations.",
                "Reset Entry Permissions",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);
            if (confirm != DialogResult.Yes) return;

            if (KeePass.Program.MainForm.DocumentManager.ActiveDatabase.IsOpen)
            {
                PwDatabase db = KeePass.Program.MainForm.DocumentManager.ActiveDatabase;

                uint counter = 0;
                var entries = db.RootGroup.GetEntries(true);

                if (entries.Count() > 999)
                {
                    MessageBox.Show(
                        String.Format("{0} entries detected!\nSearching and removing permissions could take some while.\n\nWe will inform you when the process has been finished.", entries.Count().ToString()),
                        String.Format("{0} entries detected", entries.Count().ToString()),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }

                foreach (var entry in entries)
                {
                    foreach (var str in entry.CustomData)
                    {
                        if (str.Key.Equals(KeePassNatMsgExt.SettingKey))
                        {
                            entry.History = entry.History.CloneDeep();
                            entry.CreateBackup(null);
                            entry.CustomData.Remove(str.Key);
                            entry.Touch(true);

                            counter++;

                            break;
                        }
                    }
                }

                if (counter > 0)
                {
                    KeePass.Program.MainForm.UpdateUI(false, null, true, db.RootGroup, true, null, true);
                    MessageBox.Show(
                        String.Format("Successfully removed permissions from {0} entr{1}.", counter.ToString(), counter == 1 ? "y" : "ies"),
                        String.Format("Removed permissions from {0} entr{1}", counter.ToString(), counter == 1 ? "y" : "ies"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
                else
                {
                    MessageBox.Show(
                        "The active database does not contain an entry with permissions.",
                        "No entry with permissions found!",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
            }
            else
            {
                MessageBox.Show("The active database is locked!\nPlease unlock the selected database or choose another one which is unlocked.", "Database locked!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private static string StatusText(bool ok)
        {
            return ok ? "Ready" : "Needs repair";
        }

        private static System.Drawing.Color StatusColor(bool ok)
        {
            return ok ? System.Drawing.Color.DarkGreen : System.Drawing.Color.DarkRed;
        }

        private void UpdateIntegrationUi()
        {
            var status = _chromeService.CheckStatus();
            var ready = status.State == ChromeIntegrationState.Ready;
            lblOverallStatus.Text = ready ? "Integration is ready" : status.Message;
            lblOverallStatus.ForeColor = ready ? System.Drawing.Color.DarkGreen : System.Drawing.Color.DarkOrange;

            lblProxyStatus.Text = "Proxy: " + StatusText(status.ProxyOk);
            lblProxyStatus.ForeColor = StatusColor(status.ProxyOk);
            lblManifestStatus.Text = "Manifest: " + StatusText(status.ManifestOk);
            lblManifestStatus.ForeColor = StatusColor(status.ManifestOk);
            lblChromeStatus.Text = "Google Chrome: " + StatusText(status.ChromeRegistryOk);
            lblChromeStatus.ForeColor = StatusColor(status.ChromeRegistryOk);
            lblEdgeStatus.Text = "Microsoft Edge: " + StatusText(status.EdgeRegistryOk);
            lblEdgeStatus.ForeColor = StatusColor(status.EdgeRegistryOk);
        }

        private void btnInstallIntegration_Click(object sender, EventArgs e)
        {
            string error;
            if (_chromeService.InstallOrRepair(out error))
            {
                UpdateIntegrationUi();
                MessageBox.Show(this, "Browser integration for Chrome and Edge was installed successfully.\n\nRestart the browsers, then open KeePassXC-Browser and click Connect.", "Integration Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                UpdateIntegrationUi();
                MessageBox.Show(this, "Failed to install browser integration:\n\n" + error, "Installation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnUninstallIntegration_Click(object sender, EventArgs e)
        {
            var confirm = MessageBox.Show(this, "Remove the Native Messaging registration for Chrome and Edge, including the deployed proxy?", "Confirm Uninstall", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (confirm == DialogResult.Yes)
            {
                string error;
                if (_chromeService.Uninstall(out error))
                {
                    UpdateIntegrationUi();
                    MessageBox.Show(this, "Chrome and Edge integration has been removed.", "Uninstall Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    UpdateIntegrationUi();
                    MessageBox.Show(this, "Failed to uninstall: " + error, "Uninstall Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnRefreshIntegration_Click(object sender, EventArgs e)
        {
            UpdateIntegrationUi();
        }

        private void OptionsForm_Shown(object sender, EventArgs e)
        {
            UpdateIntegrationUi();
        }

        private void InitDatabasesDropdown()
        {
            comboBoxSearchDatabases.DisplayMember = "Id";
            comboBoxSearchDatabases.ValueMember = "DbHash";
            comboBoxDatabases.DisplayMember = "Id";
            comboBoxDatabases.ValueMember = "DbHash";

            foreach (var item in KeePass.Program.MainForm.DocumentManager.Documents)
            {
                if (!item.Database.IsOpen)
                    continue;

                var dbIdentifier = item.Database.Name;
                if (string.IsNullOrEmpty(dbIdentifier))
                {
                    dbIdentifier = item.Database.IOConnectionInfo.Path;
                }

                comboBoxSearchDatabases.Items.Add(new DatabaseItem { Id = dbIdentifier, DbHash = KeePassNatMsgExt.ExtInstance.GetDbHash(item.Database) });
                comboBoxDatabases.Items.Add(new DatabaseItem { Id = dbIdentifier, DbHash = KeePassNatMsgExt.ExtInstance.GetDbHash(item.Database) });
            }
        }

        private void LoadDatabaseKeys()
        {
            LoadDatabaseKeys(KeePass.Program.MainForm.DocumentManager.ActiveDatabase);
        }

        private void LoadDatabaseKeys(PwDatabase db)
        {
            if (db.IsOpen)
            {
                var keys = new List<DatabaseKeyItem>();
                var dbKey = KeePassNatMsgExt.GetDbKey(_config.UseKeePassXcSettings);

                foreach (var cd in db.CustomData)
                {
                    if (cd.Key.StartsWith(dbKey))
                    {
                        var keyName = cd.Key.Substring(dbKey.Length);
                        keys.Add(new DatabaseKeyItem { Name = keyName, Fingerprint = CreateFingerprint(cd.Value) });
                    }
                }

                dgvKeys.DataSource = keys;
            }
        }

        private static string CreateFingerprint(string value)
        {
            if (string.IsNullOrEmpty(value)) return "(empty)";
            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(value));
                return string.Join(":", hash.Take(8).Select(x => x.ToString("X2")));
            }
        }

        private void tabControl_Selected(object sender, TabControlEventArgs e)
        {
            if (e.TabPage == tabAssociations)
            {
                LoadDatabaseKeys();
            }
        }

        private void btnRemoveSelectedKeys_Click(object sender, EventArgs e)
        {
            var db = KeePass.Program.MainForm.DocumentManager.ActiveDatabase;

            if (db.IsOpen)
            {
                var dbKey = KeePassNatMsgExt.GetDbKey(_config.UseKeePassXcSettings);

                var items = dgvKeys.SelectedRows
                    .OfType<DataGridViewRow>()
                    .Select(x => dbKey + ((x.DataBoundItem as DatabaseKeyItem) == null ? string.Empty : (x.DataBoundItem as DatabaseKeyItem).Name));

                var deleteKeys = db.CustomData
                    .Where(x => items.Contains(x.Key))
                    .Select(x => x.Key).ToList();

                RemoveKeys(deleteKeys, db);
            }
            else
            {
                MessageBox.Show(this, "The active database is locked. Unlock it before removing browser associations.", "Database Locked", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnRemoveAllKeys_Click(object sender, EventArgs e)
        {
            var db = KeePass.Program.MainForm.DocumentManager.ActiveDatabase;

            if (db.IsOpen)
            {
                var dbKey = KeePassNatMsgExt.GetDbKey(_config.UseKeePassXcSettings);

                var deleteKeys = db.CustomData
                    .Where(x => x.Key.StartsWith(dbKey))
                    .Select(x => x.Key).ToList();

                RemoveKeys(deleteKeys, db);
            }
            else
            {
                MessageBox.Show("The active database is locked!\nPlease unlock the selected database or choose another one which is unlocked.", "Database locked!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RemoveKeys(List<string> keys, PwDatabase db)
        {
            if (keys.Count > 0)
            {
                foreach (var key in keys)
                {
                    db.CustomData.Remove(key);
                }

                LoadDatabaseKeys(db);

                KeePass.Program.MainForm.UpdateUI(false, null, true, db.RootGroup, true, null, true);
                MessageBox.Show(
                    string.Format("Successfully removed {0} encryption-key{1} from KeePassNatMsg Settings.", keys.Count, keys.Count == 1 ? "" : "s"),
                    string.Format("Removed {0} key{1} from database", keys.Count, keys.Count == 1 ? "" : "s"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            else
            {
                MessageBox.Show(
                    "No shared encryption-keys found in KeePassNatMsg Settings.", "No keys found",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
        }

        private void rbSearchDatabase_CheckedChanged(object sender, EventArgs e)
        {
            comboBoxSearchDatabases.Enabled = credRestrictSearchInSpecificDatabaseRadioButton.Checked;
        }
    }
}
