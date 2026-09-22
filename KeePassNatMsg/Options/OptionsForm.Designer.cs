namespace KeePassNatMsg.Options
{
    partial class OptionsForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabIntegration = new System.Windows.Forms.TabPage();
            this.grpIntegration = new System.Windows.Forms.GroupBox();
            this.lblOverallStatus = new System.Windows.Forms.Label();
            this.lblProxyStatus = new System.Windows.Forms.Label();
            this.lblManifestStatus = new System.Windows.Forms.Label();
            this.lblChromeStatus = new System.Windows.Forms.Label();
            this.lblEdgeStatus = new System.Windows.Forms.Label();
            this.lblIntegrationHint = new System.Windows.Forms.Label();
            this.btnInstallIntegration = new System.Windows.Forms.Button();
            this.btnRefreshIntegration = new System.Windows.Forms.Button();
            this.btnUninstallIntegration = new System.Windows.Forms.Button();
            this.tabPreferences = new System.Windows.Forms.TabPage();
            this.grpMatching = new System.Windows.Forms.GroupBox();
            this.credNotifyCheckbox = new System.Windows.Forms.CheckBox();
            this.credMatchingCheckbox = new System.Windows.Forms.CheckBox();
            this.unlockDatabaseCheckbox = new System.Windows.Forms.CheckBox();
            this.hideExpiredCheckbox = new System.Windows.Forms.CheckBox();
            this.matchSchemesCheckbox = new System.Windows.Forms.CheckBox();
            this.SortByTitleRadioButton = new System.Windows.Forms.RadioButton();
            this.SortByUsernameRadioButton = new System.Windows.Forms.RadioButton();
            this.lblSort = new System.Windows.Forms.Label();
            this.grpDatabase = new System.Windows.Forms.GroupBox();
            this.credOnlySearchInSelectedDatabaseRadioButton = new System.Windows.Forms.RadioButton();
            this.credSearchInAllOpenedDatabasesRadioButton = new System.Windows.Forms.RadioButton();
            this.credRestrictSearchInSpecificDatabaseRadioButton = new System.Windows.Forms.RadioButton();
            this.comboBoxSearchDatabases = new System.Windows.Forms.ComboBox();
            this.labelConnDb = new System.Windows.Forms.Label();
            this.comboBoxDatabases = new System.Windows.Forms.ComboBox();
            this.grpNewLogins = new System.Windows.Forms.GroupBox();
            this.lblDefaultGroup = new System.Windows.Forms.Label();
            this.txtDefaultGroup = new System.Windows.Forms.TextBox();
            this.chkDefaultGroupAlwaysAllow = new System.Windows.Forms.CheckBox();
            this.chkSearchUrls = new System.Windows.Forms.CheckBox();
            this.grpDangerZone = new System.Windows.Forms.GroupBox();
            this.lblDangerWarning = new System.Windows.Forms.Label();
            this.credAllowAccessCheckbox = new System.Windows.Forms.CheckBox();
            this.credAllowUpdatesCheckbox = new System.Windows.Forms.CheckBox();
            this.removePermissionsButton = new System.Windows.Forms.Button();
            this.tabAssociations = new System.Windows.Forms.TabPage();
            this.lblKeysHint = new System.Windows.Forms.Label();
            this.dgvKeys = new System.Windows.Forms.DataGridView();
            this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colFingerprint = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnRemoveSelectedKeys = new System.Windows.Forms.Button();
            this.btnRemoveAllKeys = new System.Windows.Forms.Button();
            this.lblVersion = new System.Windows.Forms.Label();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.tabControl.SuspendLayout();
            this.tabIntegration.SuspendLayout();
            this.grpIntegration.SuspendLayout();
            this.tabPreferences.SuspendLayout();
            this.grpMatching.SuspendLayout();
            this.grpDatabase.SuspendLayout();
            this.grpNewLogins.SuspendLayout();
            this.grpDangerZone.SuspendLayout();
            this.tabAssociations.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvKeys)).BeginInit();
            this.SuspendLayout();
            //
            // tabControl
            //
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.tabIntegration);
            this.tabControl.Controls.Add(this.tabPreferences);
            this.tabControl.Controls.Add(this.tabAssociations);
            this.tabControl.Location = new System.Drawing.Point(12, 12);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(696, 564);
            this.tabControl.TabIndex = 0;
            this.tabControl.Selected += new System.Windows.Forms.TabControlEventHandler(this.tabControl_Selected);
            //
            // tabIntegration
            //
            this.tabIntegration.Controls.Add(this.grpIntegration);
            this.tabIntegration.Location = new System.Drawing.Point(4, 24);
            this.tabIntegration.Name = "tabIntegration";
            this.tabIntegration.Padding = new System.Windows.Forms.Padding(14);
            this.tabIntegration.Size = new System.Drawing.Size(688, 536);
            this.tabIntegration.TabIndex = 0;
            this.tabIntegration.Text = "Browser Integration";
            this.tabIntegration.UseVisualStyleBackColor = true;
            //
            // grpIntegration
            //
            this.grpIntegration.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.grpIntegration.Controls.Add(this.lblOverallStatus);
            this.grpIntegration.Controls.Add(this.lblProxyStatus);
            this.grpIntegration.Controls.Add(this.lblManifestStatus);
            this.grpIntegration.Controls.Add(this.lblChromeStatus);
            this.grpIntegration.Controls.Add(this.lblEdgeStatus);
            this.grpIntegration.Controls.Add(this.lblIntegrationHint);
            this.grpIntegration.Controls.Add(this.btnInstallIntegration);
            this.grpIntegration.Controls.Add(this.btnRefreshIntegration);
            this.grpIntegration.Controls.Add(this.btnUninstallIntegration);
            this.grpIntegration.Location = new System.Drawing.Point(14, 14);
            this.grpIntegration.Name = "grpIntegration";
            this.grpIntegration.Size = new System.Drawing.Size(660, 286);
            this.grpIntegration.TabIndex = 0;
            this.grpIntegration.TabStop = false;
            this.grpIntegration.Text = "Chrome and Edge Native Messaging";
            //
            // lblOverallStatus
            //
            this.lblOverallStatus.AutoSize = true;
            this.lblOverallStatus.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.lblOverallStatus.Location = new System.Drawing.Point(18, 30);
            this.lblOverallStatus.Name = "lblOverallStatus";
            this.lblOverallStatus.Size = new System.Drawing.Size(127, 19);
            this.lblOverallStatus.TabIndex = 0;
            this.lblOverallStatus.Text = "Checking status...";
            //
            // status labels
            //
            this.lblProxyStatus.AutoSize = true;
            this.lblProxyStatus.Location = new System.Drawing.Point(20, 68);
            this.lblProxyStatus.Name = "lblProxyStatus";
            this.lblProxyStatus.Size = new System.Drawing.Size(79, 15);
            this.lblProxyStatus.Text = "Proxy: ...";
            this.lblManifestStatus.AutoSize = true;
            this.lblManifestStatus.Location = new System.Drawing.Point(20, 94);
            this.lblManifestStatus.Name = "lblManifestStatus";
            this.lblManifestStatus.Size = new System.Drawing.Size(91, 15);
            this.lblManifestStatus.Text = "Manifest: ...";
            this.lblChromeStatus.AutoSize = true;
            this.lblChromeStatus.Location = new System.Drawing.Point(20, 120);
            this.lblChromeStatus.Name = "lblChromeStatus";
            this.lblChromeStatus.Size = new System.Drawing.Size(91, 15);
            this.lblChromeStatus.Text = "Chrome: ...";
            this.lblEdgeStatus.AutoSize = true;
            this.lblEdgeStatus.Location = new System.Drawing.Point(20, 146);
            this.lblEdgeStatus.Name = "lblEdgeStatus";
            this.lblEdgeStatus.Size = new System.Drawing.Size(73, 15);
            this.lblEdgeStatus.Text = "Edge: ...";
            //
            // lblIntegrationHint
            //
            this.lblIntegrationHint.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.lblIntegrationHint.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblIntegrationHint.Location = new System.Drawing.Point(20, 177);
            this.lblIntegrationHint.Name = "lblIntegrationHint";
            this.lblIntegrationHint.Size = new System.Drawing.Size(620, 36);
            this.lblIntegrationHint.TabIndex = 5;
            this.lblIntegrationHint.Text = "Install / Repair deploys the bundled proxy and registers the Native Messaging host for the current Windows user. Restart Chrome and Edge after changes.";
            //
            // integration buttons
            //
            this.btnInstallIntegration.Location = new System.Drawing.Point(20, 228);
            this.btnInstallIntegration.Name = "btnInstallIntegration";
            this.btnInstallIntegration.Size = new System.Drawing.Size(190, 32);
            this.btnInstallIntegration.TabIndex = 6;
            this.btnInstallIntegration.Text = "Install / Repair";
            this.btnInstallIntegration.UseVisualStyleBackColor = true;
            this.btnInstallIntegration.Click += new System.EventHandler(this.btnInstallIntegration_Click);
            this.btnRefreshIntegration.Location = new System.Drawing.Point(220, 228);
            this.btnRefreshIntegration.Name = "btnRefreshIntegration";
            this.btnRefreshIntegration.Size = new System.Drawing.Size(105, 32);
            this.btnRefreshIntegration.TabIndex = 7;
            this.btnRefreshIntegration.Text = "Refresh";
            this.btnRefreshIntegration.UseVisualStyleBackColor = true;
            this.btnRefreshIntegration.Click += new System.EventHandler(this.btnRefreshIntegration_Click);
            this.btnUninstallIntegration.Location = new System.Drawing.Point(335, 228);
            this.btnUninstallIntegration.Name = "btnUninstallIntegration";
            this.btnUninstallIntegration.Size = new System.Drawing.Size(105, 32);
            this.btnUninstallIntegration.TabIndex = 8;
            this.btnUninstallIntegration.Text = "Uninstall";
            this.btnUninstallIntegration.UseVisualStyleBackColor = true;
            this.btnUninstallIntegration.Click += new System.EventHandler(this.btnUninstallIntegration_Click);
            //
            // tabPreferences  (AutoScroll so all groups are reachable on any DPI)
            //
            this.tabPreferences.AutoScroll = true;
            this.tabPreferences.Controls.Add(this.grpMatching);
            this.tabPreferences.Controls.Add(this.grpDatabase);
            this.tabPreferences.Controls.Add(this.grpNewLogins);
            this.tabPreferences.Controls.Add(this.grpDangerZone);
            this.tabPreferences.Location = new System.Drawing.Point(4, 24);
            this.tabPreferences.Name = "tabPreferences";
            this.tabPreferences.Padding = new System.Windows.Forms.Padding(14);
            this.tabPreferences.Size = new System.Drawing.Size(688, 536);
            this.tabPreferences.TabIndex = 1;
            this.tabPreferences.Text = "Preferences";
            this.tabPreferences.UseVisualStyleBackColor = true;
            //
            // grpMatching  Y=14, H=200
            //
            this.grpMatching.Controls.Add(this.credNotifyCheckbox);
            this.grpMatching.Controls.Add(this.credMatchingCheckbox);
            this.grpMatching.Controls.Add(this.unlockDatabaseCheckbox);
            this.grpMatching.Controls.Add(this.hideExpiredCheckbox);
            this.grpMatching.Controls.Add(this.matchSchemesCheckbox);
            this.grpMatching.Controls.Add(this.lblSort);
            this.grpMatching.Controls.Add(this.SortByTitleRadioButton);
            this.grpMatching.Controls.Add(this.SortByUsernameRadioButton);
            this.grpMatching.Location = new System.Drawing.Point(14, 14);
            this.grpMatching.Name = "grpMatching";
            this.grpMatching.Size = new System.Drawing.Size(638, 200);
            this.grpMatching.TabIndex = 0;
            this.grpMatching.TabStop = false;
            this.grpMatching.Text = "Credential Matching";
            this.credNotifyCheckbox.AutoSize = true;
            this.credNotifyCheckbox.Location = new System.Drawing.Point(18, 28);
            this.credNotifyCheckbox.Name = "credNotifyCheckbox";
            this.credNotifyCheckbox.Size = new System.Drawing.Size(254, 19);
            this.credNotifyCheckbox.Text = "Notify when credentials are requested";
            this.credNotifyCheckbox.UseVisualStyleBackColor = true;
            this.credMatchingCheckbox.AutoSize = true;
            this.credMatchingCheckbox.Location = new System.Drawing.Point(18, 56);
            this.credMatchingCheckbox.Name = "credMatchingCheckbox";
            this.credMatchingCheckbox.Size = new System.Drawing.Size(206, 19);
            this.credMatchingCheckbox.Text = "Return only the best URL matches";
            this.credMatchingCheckbox.UseVisualStyleBackColor = true;
            this.unlockDatabaseCheckbox.AutoSize = true;
            this.unlockDatabaseCheckbox.Location = new System.Drawing.Point(18, 84);
            this.unlockDatabaseCheckbox.Name = "unlockDatabaseCheckbox";
            this.unlockDatabaseCheckbox.Size = new System.Drawing.Size(222, 19);
            this.unlockDatabaseCheckbox.Text = "Request database unlock when needed";
            this.unlockDatabaseCheckbox.UseVisualStyleBackColor = true;
            this.hideExpiredCheckbox.AutoSize = true;
            this.hideExpiredCheckbox.Location = new System.Drawing.Point(18, 112);
            this.hideExpiredCheckbox.Name = "hideExpiredCheckbox";
            this.hideExpiredCheckbox.Size = new System.Drawing.Size(166, 19);
            this.hideExpiredCheckbox.Text = "Exclude expired entries";
            this.hideExpiredCheckbox.UseVisualStyleBackColor = true;
            this.matchSchemesCheckbox.AutoSize = true;
            this.matchSchemesCheckbox.Location = new System.Drawing.Point(18, 140);
            this.matchSchemesCheckbox.Name = "matchSchemesCheckbox";
            this.matchSchemesCheckbox.Size = new System.Drawing.Size(206, 19);
            this.matchSchemesCheckbox.Text = "Require matching URL scheme";
            this.matchSchemesCheckbox.UseVisualStyleBackColor = true;
            this.lblSort.AutoSize = true;
            this.lblSort.Location = new System.Drawing.Point(18, 170);
            this.lblSort.Name = "lblSort";
            this.lblSort.Size = new System.Drawing.Size(78, 15);
            this.lblSort.Text = "Sort results:";
            this.SortByTitleRadioButton.AutoSize = true;
            this.SortByTitleRadioButton.Location = new System.Drawing.Point(110, 168);
            this.SortByTitleRadioButton.Name = "SortByTitleRadioButton";
            this.SortByTitleRadioButton.Size = new System.Drawing.Size(61, 19);
            this.SortByTitleRadioButton.Text = "Title";
            this.SortByTitleRadioButton.UseVisualStyleBackColor = true;
            this.SortByUsernameRadioButton.AutoSize = true;
            this.SortByUsernameRadioButton.Location = new System.Drawing.Point(190, 168);
            this.SortByUsernameRadioButton.Name = "SortByUsernameRadioButton";
            this.SortByUsernameRadioButton.Size = new System.Drawing.Size(83, 19);
            this.SortByUsernameRadioButton.Text = "Username";
            this.SortByUsernameRadioButton.UseVisualStyleBackColor = true;
            //
            // grpDatabase  Y=222, H=130
            //
            this.grpDatabase.Controls.Add(this.credOnlySearchInSelectedDatabaseRadioButton);
            this.grpDatabase.Controls.Add(this.credSearchInAllOpenedDatabasesRadioButton);
            this.grpDatabase.Controls.Add(this.credRestrictSearchInSpecificDatabaseRadioButton);
            this.grpDatabase.Controls.Add(this.comboBoxSearchDatabases);
            this.grpDatabase.Controls.Add(this.labelConnDb);
            this.grpDatabase.Controls.Add(this.comboBoxDatabases);
            this.grpDatabase.Location = new System.Drawing.Point(14, 222);
            this.grpDatabase.Name = "grpDatabase";
            this.grpDatabase.Size = new System.Drawing.Size(638, 130);
            this.grpDatabase.TabIndex = 1;
            this.grpDatabase.TabStop = false;
            this.grpDatabase.Text = "Database Scope";
            this.credOnlySearchInSelectedDatabaseRadioButton.AutoSize = true;
            this.credOnlySearchInSelectedDatabaseRadioButton.Location = new System.Drawing.Point(18, 26);
            this.credOnlySearchInSelectedDatabaseRadioButton.Name = "credOnlySearchInSelectedDatabaseRadioButton";
            this.credOnlySearchInSelectedDatabaseRadioButton.Size = new System.Drawing.Size(169, 19);
            this.credOnlySearchInSelectedDatabaseRadioButton.Text = "Use the connection database";
            this.credOnlySearchInSelectedDatabaseRadioButton.UseVisualStyleBackColor = true;
            this.credOnlySearchInSelectedDatabaseRadioButton.CheckedChanged += new System.EventHandler(this.rbSearchDatabase_CheckedChanged);
            this.credSearchInAllOpenedDatabasesRadioButton.AutoSize = true;
            this.credSearchInAllOpenedDatabasesRadioButton.Location = new System.Drawing.Point(205, 26);
            this.credSearchInAllOpenedDatabasesRadioButton.Name = "credSearchInAllOpenedDatabasesRadioButton";
            this.credSearchInAllOpenedDatabasesRadioButton.Size = new System.Drawing.Size(184, 19);
            this.credSearchInAllOpenedDatabasesRadioButton.Text = "Search all open databases";
            this.credSearchInAllOpenedDatabasesRadioButton.UseVisualStyleBackColor = true;
            this.credSearchInAllOpenedDatabasesRadioButton.CheckedChanged += new System.EventHandler(this.rbSearchDatabase_CheckedChanged);
            this.credRestrictSearchInSpecificDatabaseRadioButton.AutoSize = true;
            this.credRestrictSearchInSpecificDatabaseRadioButton.Location = new System.Drawing.Point(18, 55);
            this.credRestrictSearchInSpecificDatabaseRadioButton.Name = "credRestrictSearchInSpecificDatabaseRadioButton";
            this.credRestrictSearchInSpecificDatabaseRadioButton.Size = new System.Drawing.Size(151, 19);
            this.credRestrictSearchInSpecificDatabaseRadioButton.Text = "Search only this database:";
            this.credRestrictSearchInSpecificDatabaseRadioButton.UseVisualStyleBackColor = true;
            this.credRestrictSearchInSpecificDatabaseRadioButton.CheckedChanged += new System.EventHandler(this.rbSearchDatabase_CheckedChanged);
            this.comboBoxSearchDatabases.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSearchDatabases.FormattingEnabled = true;
            this.comboBoxSearchDatabases.Location = new System.Drawing.Point(180, 53);
            this.comboBoxSearchDatabases.Name = "comboBoxSearchDatabases";
            this.comboBoxSearchDatabases.Size = new System.Drawing.Size(430, 23);
            this.labelConnDb.AutoSize = true;
            this.labelConnDb.Location = new System.Drawing.Point(18, 95);
            this.labelConnDb.Name = "labelConnDb";
            this.labelConnDb.Size = new System.Drawing.Size(123, 15);
            this.labelConnDb.Text = "Connection database:";
            this.comboBoxDatabases.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDatabases.FormattingEnabled = true;
            this.comboBoxDatabases.Location = new System.Drawing.Point(180, 91);
            this.comboBoxDatabases.Name = "comboBoxDatabases";
            this.comboBoxDatabases.Size = new System.Drawing.Size(430, 23);
            //
            // grpNewLogins  Y=360, H=108
            //
            this.grpNewLogins.Controls.Add(this.lblDefaultGroup);
            this.grpNewLogins.Controls.Add(this.txtDefaultGroup);
            this.grpNewLogins.Controls.Add(this.chkDefaultGroupAlwaysAllow);
            this.grpNewLogins.Controls.Add(this.chkSearchUrls);
            this.grpNewLogins.Location = new System.Drawing.Point(14, 360);
            this.grpNewLogins.Name = "grpNewLogins";
            this.grpNewLogins.Size = new System.Drawing.Size(638, 108);
            this.grpNewLogins.TabIndex = 2;
            this.grpNewLogins.TabStop = false;
            this.grpNewLogins.Text = "Entries and New Logins";
            this.lblDefaultGroup.AutoSize = true;
            this.lblDefaultGroup.Location = new System.Drawing.Point(18, 29);
            this.lblDefaultGroup.Name = "lblDefaultGroup";
            this.lblDefaultGroup.Size = new System.Drawing.Size(81, 15);
            this.lblDefaultGroup.Text = "Default group:";
            this.txtDefaultGroup.Location = new System.Drawing.Point(115, 25);
            this.txtDefaultGroup.Name = "txtDefaultGroup";
            this.txtDefaultGroup.Size = new System.Drawing.Size(495, 23);
            this.chkDefaultGroupAlwaysAllow.AutoSize = true;
            this.chkDefaultGroupAlwaysAllow.Location = new System.Drawing.Point(18, 56);
            this.chkDefaultGroupAlwaysAllow.Name = "chkDefaultGroupAlwaysAllow";
            this.chkDefaultGroupAlwaysAllow.Size = new System.Drawing.Size(293, 19);
            this.chkDefaultGroupAlwaysAllow.Text = "Allow entries in the default group without prompting";
            this.chkDefaultGroupAlwaysAllow.UseVisualStyleBackColor = true;
            this.chkSearchUrls.AutoSize = true;
            this.chkSearchUrls.Location = new System.Drawing.Point(18, 82);
            this.chkSearchUrls.Name = "chkSearchUrls";
            this.chkSearchUrls.Size = new System.Drawing.Size(244, 19);
            this.chkSearchUrls.Text = "Search additional URL and KP2A_URL fields";
            this.chkSearchUrls.UseVisualStyleBackColor = true;
            //
            // grpDangerZone  Y=476, H=124  — fits within 536px tab height (14 padding + 476 + 124 = 614 → AutoScroll handles overflow gracefully)
            //
            this.grpDangerZone.Controls.Add(this.lblDangerWarning);
            this.grpDangerZone.Controls.Add(this.credAllowAccessCheckbox);
            this.grpDangerZone.Controls.Add(this.credAllowUpdatesCheckbox);
            this.grpDangerZone.Controls.Add(this.removePermissionsButton);
            this.grpDangerZone.ForeColor = System.Drawing.Color.DarkRed;
            this.grpDangerZone.Location = new System.Drawing.Point(14, 476);
            this.grpDangerZone.Name = "grpDangerZone";
            this.grpDangerZone.Size = new System.Drawing.Size(638, 124);
            this.grpDangerZone.TabIndex = 3;
            this.grpDangerZone.TabStop = false;
            this.grpDangerZone.Text = "Danger Zone";
            this.lblDangerWarning.ForeColor = System.Drawing.Color.DarkRed;
            this.lblDangerWarning.Location = new System.Drawing.Point(18, 22);
            this.lblDangerWarning.Name = "lblDangerWarning";
            this.lblDangerWarning.Size = new System.Drawing.Size(600, 30);
            this.lblDangerWarning.Text = "These options bypass confirmation prompts. Enable them only when you understand the credential exposure risk.";
            this.credAllowAccessCheckbox.AutoSize = true;
            this.credAllowAccessCheckbox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.credAllowAccessCheckbox.Location = new System.Drawing.Point(18, 58);
            this.credAllowAccessCheckbox.Name = "credAllowAccessCheckbox";
            this.credAllowAccessCheckbox.Size = new System.Drawing.Size(210, 19);
            this.credAllowAccessCheckbox.Text = "Always allow credential access";
            this.credAllowAccessCheckbox.UseVisualStyleBackColor = true;
            this.credAllowUpdatesCheckbox.AutoSize = true;
            this.credAllowUpdatesCheckbox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.credAllowUpdatesCheckbox.Location = new System.Drawing.Point(250, 58);
            this.credAllowUpdatesCheckbox.Name = "credAllowUpdatesCheckbox";
            this.credAllowUpdatesCheckbox.Size = new System.Drawing.Size(204, 19);
            this.credAllowUpdatesCheckbox.Text = "Always allow credential updates";
            this.credAllowUpdatesCheckbox.UseVisualStyleBackColor = true;
            this.removePermissionsButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.removePermissionsButton.Location = new System.Drawing.Point(18, 86);
            this.removePermissionsButton.Name = "removePermissionsButton";
            this.removePermissionsButton.Size = new System.Drawing.Size(250, 26);
            this.removePermissionsButton.Text = "Reset entry access permissions...";
            this.removePermissionsButton.UseVisualStyleBackColor = true;
            this.removePermissionsButton.Click += new System.EventHandler(this.removePermissionsButton_Click);
            //
            // tabAssociations
            //
            this.tabAssociations.Controls.Add(this.lblKeysHint);
            this.tabAssociations.Controls.Add(this.dgvKeys);
            this.tabAssociations.Controls.Add(this.btnRemoveSelectedKeys);
            this.tabAssociations.Controls.Add(this.btnRemoveAllKeys);
            this.tabAssociations.Location = new System.Drawing.Point(4, 24);
            this.tabAssociations.Name = "tabAssociations";
            this.tabAssociations.Padding = new System.Windows.Forms.Padding(14);
            this.tabAssociations.Size = new System.Drawing.Size(688, 536);
            this.tabAssociations.TabIndex = 2;
            this.tabAssociations.Text = "Associations";
            this.tabAssociations.UseVisualStyleBackColor = true;
            this.lblKeysHint.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKeysHint.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblKeysHint.Location = new System.Drawing.Point(14, 14);
            this.lblKeysHint.Name = "lblKeysHint";
            this.lblKeysHint.Size = new System.Drawing.Size(650, 36);
            this.lblKeysHint.Text = "Associated browser identities for the active database. Only a short fingerprint is shown; secret key material is never displayed.";
            this.dgvKeys.AllowUserToAddRows = false;
            this.dgvKeys.AllowUserToDeleteRows = false;
            this.dgvKeys.AllowUserToResizeRows = false;
            this.dgvKeys.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvKeys.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvKeys.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvKeys.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { this.colName, this.colFingerprint });
            this.dgvKeys.Location = new System.Drawing.Point(14, 58);
            this.dgvKeys.MultiSelect = true;
            this.dgvKeys.Name = "dgvKeys";
            this.dgvKeys.ReadOnly = true;
            this.dgvKeys.RowHeadersVisible = false;
            this.dgvKeys.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvKeys.Size = new System.Drawing.Size(650, 420);
            this.colName.DataPropertyName = "Name";
            this.colName.HeaderText = "Client name";
            this.colName.Name = "colName";
            this.colName.ReadOnly = true;
            this.colFingerprint.DataPropertyName = "Fingerprint";
            this.colFingerprint.HeaderText = "Key fingerprint";
            this.colFingerprint.Name = "colFingerprint";
            this.colFingerprint.ReadOnly = true;
            this.btnRemoveSelectedKeys.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemoveSelectedKeys.Location = new System.Drawing.Point(397, 490);
            this.btnRemoveSelectedKeys.Name = "btnRemoveSelectedKeys";
            this.btnRemoveSelectedKeys.Size = new System.Drawing.Size(130, 30);
            this.btnRemoveSelectedKeys.Text = "Remove Selected";
            this.btnRemoveSelectedKeys.UseVisualStyleBackColor = true;
            this.btnRemoveSelectedKeys.Click += new System.EventHandler(this.btnRemoveSelectedKeys_Click);
            this.btnRemoveAllKeys.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemoveAllKeys.Location = new System.Drawing.Point(537, 490);
            this.btnRemoveAllKeys.Name = "btnRemoveAllKeys";
            this.btnRemoveAllKeys.Size = new System.Drawing.Size(127, 30);
            this.btnRemoveAllKeys.Text = "Remove All";
            this.btnRemoveAllKeys.UseVisualStyleBackColor = true;
            this.btnRemoveAllKeys.Click += new System.EventHandler(this.btnRemoveAllKeys_Click);
            //
            // footer
            //
            this.lblVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblVersion.AutoSize = true;
            this.lblVersion.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblVersion.Location = new System.Drawing.Point(16, 594);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(93, 15);
            this.lblVersion.Text = "KeePassNatMsg";
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Location = new System.Drawing.Point(531, 588);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(85, 30);
            this.okButton.Text = "Save";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(623, 588);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(85, 30);
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            //
            // OptionsForm
            //
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(720, 630);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OptionsForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "KeePassNatMsg Options";
            this.Load += new System.EventHandler(this.OptionsForm_Load);
            this.Shown += new System.EventHandler(this.OptionsForm_Shown);
            this.tabControl.ResumeLayout(false);
            this.tabIntegration.ResumeLayout(false);
            this.grpIntegration.ResumeLayout(false);
            this.grpIntegration.PerformLayout();
            this.tabPreferences.ResumeLayout(false);
            this.grpMatching.ResumeLayout(false);
            this.grpMatching.PerformLayout();
            this.grpDatabase.ResumeLayout(false);
            this.grpDatabase.PerformLayout();
            this.grpNewLogins.ResumeLayout(false);
            this.grpNewLogins.PerformLayout();
            this.grpDangerZone.ResumeLayout(false);
            this.grpDangerZone.PerformLayout();
            this.tabAssociations.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvKeys)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabIntegration;
        private System.Windows.Forms.GroupBox grpIntegration;
        private System.Windows.Forms.Label lblOverallStatus;
        private System.Windows.Forms.Label lblProxyStatus;
        private System.Windows.Forms.Label lblManifestStatus;
        private System.Windows.Forms.Label lblChromeStatus;
        private System.Windows.Forms.Label lblEdgeStatus;
        private System.Windows.Forms.Label lblIntegrationHint;
        private System.Windows.Forms.Button btnInstallIntegration;
        private System.Windows.Forms.Button btnRefreshIntegration;
        private System.Windows.Forms.Button btnUninstallIntegration;
        private System.Windows.Forms.TabPage tabPreferences;
        private System.Windows.Forms.GroupBox grpMatching;
        private System.Windows.Forms.CheckBox credNotifyCheckbox;
        private System.Windows.Forms.CheckBox credMatchingCheckbox;
        private System.Windows.Forms.CheckBox unlockDatabaseCheckbox;
        private System.Windows.Forms.CheckBox hideExpiredCheckbox;
        private System.Windows.Forms.CheckBox matchSchemesCheckbox;
        private System.Windows.Forms.Label lblSort;
        private System.Windows.Forms.RadioButton SortByTitleRadioButton;
        private System.Windows.Forms.RadioButton SortByUsernameRadioButton;
        private System.Windows.Forms.GroupBox grpDatabase;
        private System.Windows.Forms.RadioButton credOnlySearchInSelectedDatabaseRadioButton;
        private System.Windows.Forms.RadioButton credSearchInAllOpenedDatabasesRadioButton;
        private System.Windows.Forms.RadioButton credRestrictSearchInSpecificDatabaseRadioButton;
        private System.Windows.Forms.ComboBox comboBoxSearchDatabases;
        private System.Windows.Forms.Label labelConnDb;
        private System.Windows.Forms.ComboBox comboBoxDatabases;
        private System.Windows.Forms.GroupBox grpNewLogins;
        private System.Windows.Forms.Label lblDefaultGroup;
        private System.Windows.Forms.TextBox txtDefaultGroup;
        private System.Windows.Forms.CheckBox chkDefaultGroupAlwaysAllow;
        private System.Windows.Forms.CheckBox chkSearchUrls;
        private System.Windows.Forms.GroupBox grpDangerZone;
        private System.Windows.Forms.Label lblDangerWarning;
        private System.Windows.Forms.CheckBox credAllowAccessCheckbox;
        private System.Windows.Forms.CheckBox credAllowUpdatesCheckbox;
        private System.Windows.Forms.Button removePermissionsButton;
        private System.Windows.Forms.TabPage tabAssociations;
        private System.Windows.Forms.Label lblKeysHint;
        private System.Windows.Forms.DataGridView dgvKeys;
        private System.Windows.Forms.DataGridViewTextBoxColumn colName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFingerprint;
        private System.Windows.Forms.Button btnRemoveSelectedKeys;
        private System.Windows.Forms.Button btnRemoveAllKeys;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
    }
}
