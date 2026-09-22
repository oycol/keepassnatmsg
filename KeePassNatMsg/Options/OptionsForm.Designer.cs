namespace KeePassNatMsg.Options
{
    partial class OptionsForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.grpChrome = new System.Windows.Forms.GroupBox();
            this.lblChromeStatus = new System.Windows.Forms.Label();
            this.btnInstallChrome = new System.Windows.Forms.Button();
            this.btnUninstallChrome = new System.Windows.Forms.Button();
            this.btnRefreshChrome = new System.Windows.Forms.Button();
            this.grpMatching = new System.Windows.Forms.GroupBox();
            this.credNotifyCheckbox = new System.Windows.Forms.CheckBox();
            this.credMatchingCheckbox = new System.Windows.Forms.CheckBox();
            this.unlockDatabaseCheckbox = new System.Windows.Forms.CheckBox();
            this.hideExpiredCheckbox = new System.Windows.Forms.CheckBox();
            this.matchSchemesCheckbox = new System.Windows.Forms.CheckBox();
            this.chkUseKpxcSettingsGeneral = new System.Windows.Forms.CheckBox();
            this.grpSorting = new System.Windows.Forms.GroupBox();
            this.SortByTitleRadioButton = new System.Windows.Forms.RadioButton();
            this.SortByUsernameRadioButton = new System.Windows.Forms.RadioButton();
            this.lblVersion = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.grpSearchDb = new System.Windows.Forms.GroupBox();
            this.credOnlySearchInSelectedDatabaseRadioButton = new System.Windows.Forms.RadioButton();
            this.credSearchInAllOpenedDatabasesRadioButton = new System.Windows.Forms.RadioButton();
            this.credRestrictSearchInSpecificDatabaseRadioButton = new System.Windows.Forms.RadioButton();
            this.comboBoxSearchDatabases = new System.Windows.Forms.ComboBox();
            this.grpDefaultGroup = new System.Windows.Forms.GroupBox();
            this.lblDefaultGroup = new System.Windows.Forms.Label();
            this.txtDefaultGroup = new System.Windows.Forms.TextBox();
            this.chkDefaultGroupAlwaysAllow = new System.Windows.Forms.CheckBox();
            this.grpFields = new System.Windows.Forms.GroupBox();
            this.grpDangerZone = new System.Windows.Forms.GroupBox();
            this.credAllowAccessCheckbox = new System.Windows.Forms.CheckBox();
            this.credAllowUpdatesCheckbox = new System.Windows.Forms.CheckBox();
            this.removePermissionsButton = new System.Windows.Forms.Button();
            this.labelConnDb = new System.Windows.Forms.Label();
            this.comboBoxDatabases = new System.Windows.Forms.ComboBox();
            this.chkUseKpxcSettingsKey = new System.Windows.Forms.CheckBox();
            this.txtKPXCVerOverride = new System.Windows.Forms.TextBox();
            this.lblKPXCVerOverride = new System.Windows.Forms.Label();
            this.btnMigrateSettings = new System.Windows.Forms.Button();
            this.btnCheckForLegacyConfig = new System.Windows.Forms.Button();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.scKeysMain = new System.Windows.Forms.SplitContainer();
            this.dgvKeys = new System.Windows.Forms.DataGridView();
            this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colKey = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnRemoveAllKeys = new System.Windows.Forms.Button();
            this.btnRemoveSelectedKeys = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.grpChrome.SuspendLayout();
            this.grpMatching.SuspendLayout();
            this.grpSorting.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.grpSearchDb.SuspendLayout();
            this.grpDefaultGroup.SuspendLayout();
            this.grpFields.SuspendLayout();
            this.grpDangerZone.SuspendLayout();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scKeysMain)).BeginInit();
            this.scKeysMain.Panel1.SuspendLayout();
            this.scKeysMain.Panel2.SuspendLayout();
            this.scKeysMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvKeys)).BeginInit();
            this.SuspendLayout();
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(465, 520);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(85, 26);
            this.cancelButton.TabIndex = 2;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Location = new System.Drawing.Point(374, 520);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(85, 26);
            this.okButton.TabIndex = 1;
            this.okButton.Text = "Save";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(538, 498);
            this.tabControl1.TabIndex = 0;
            this.tabControl1.Selected += new System.Windows.Forms.TabControlEventHandler(this.tabControl1_Selected);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.grpChrome);
            this.tabPage1.Controls.Add(this.grpMatching);
            this.tabPage1.Controls.Add(this.grpSorting);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(10);
            this.tabPage1.Size = new System.Drawing.Size(530, 472);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "General";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // grpChrome
            // 
            this.grpChrome.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpChrome.Controls.Add(this.lblChromeStatus);
            this.grpChrome.Controls.Add(this.btnInstallChrome);
            this.grpChrome.Controls.Add(this.btnUninstallChrome);
            this.grpChrome.Controls.Add(this.btnRefreshChrome);
            this.grpChrome.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpChrome.Location = new System.Drawing.Point(10, 10);
            this.grpChrome.Name = "grpChrome";
            this.grpChrome.Size = new System.Drawing.Size(510, 105);
            this.grpChrome.TabIndex = 0;
            this.grpChrome.TabStop = false;
            this.grpChrome.Text = "Browser Integration (Chrome & Edge)";
            // 
            // lblChromeStatus
            // 
            this.lblChromeStatus.AutoSize = true;
            this.lblChromeStatus.Location = new System.Drawing.Point(12, 25);
            this.lblChromeStatus.Name = "lblChromeStatus";
            this.lblChromeStatus.Size = new System.Drawing.Size(180, 15);
            this.lblChromeStatus.TabIndex = 0;
            this.lblChromeStatus.Text = "Status: Checking integration...";
            // 
            // btnInstallChrome
            // 
            this.btnInstallChrome.Location = new System.Drawing.Point(15, 55);
            this.btnInstallChrome.Name = "btnInstallChrome";
            this.btnInstallChrome.Size = new System.Drawing.Size(190, 30);
            this.btnInstallChrome.TabIndex = 1;
            this.btnInstallChrome.Text = "Install / Repair Integration";
            this.btnInstallChrome.UseVisualStyleBackColor = true;
            this.btnInstallChrome.Click += new System.EventHandler(this.btnInstallChrome_Click);
            // 
            // btnUninstallChrome
            // 
            this.btnUninstallChrome.Location = new System.Drawing.Point(215, 55);
            this.btnUninstallChrome.Name = "btnUninstallChrome";
            this.btnUninstallChrome.Size = new System.Drawing.Size(110, 30);
            this.btnUninstallChrome.TabIndex = 2;
            this.btnUninstallChrome.Text = "Uninstall";
            this.btnUninstallChrome.UseVisualStyleBackColor = true;
            this.btnUninstallChrome.Click += new System.EventHandler(this.btnUninstallChrome_Click);
            // 
            // btnRefreshChrome
            // 
            this.btnRefreshChrome.Location = new System.Drawing.Point(335, 55);
            this.btnRefreshChrome.Name = "btnRefreshChrome";
            this.btnRefreshChrome.Size = new System.Drawing.Size(95, 30);
            this.btnRefreshChrome.TabIndex = 3;
            this.btnRefreshChrome.Text = "Refresh";
            this.btnRefreshChrome.UseVisualStyleBackColor = true;
            this.btnRefreshChrome.Click += new System.EventHandler(this.btnRefreshChrome_Click);
            // 
            // grpMatching
            // 
            this.grpMatching.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpMatching.Controls.Add(this.credNotifyCheckbox);
            this.grpMatching.Controls.Add(this.credMatchingCheckbox);
            this.grpMatching.Controls.Add(this.unlockDatabaseCheckbox);
            this.grpMatching.Controls.Add(this.hideExpiredCheckbox);
            this.grpMatching.Controls.Add(this.matchSchemesCheckbox);
            this.grpMatching.Controls.Add(this.chkUseKpxcSettingsGeneral);
            this.grpMatching.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpMatching.Location = new System.Drawing.Point(10, 125);
            this.grpMatching.Name = "grpMatching";
            this.grpMatching.Size = new System.Drawing.Size(510, 240);
            this.grpMatching.TabIndex = 1;
            this.grpMatching.TabStop = false;
            this.grpMatching.Text = "Credential Matching & Access Rules";
            // 
            // credNotifyCheckbox
            // 
            this.credNotifyCheckbox.AutoSize = true;
            this.credNotifyCheckbox.Location = new System.Drawing.Point(15, 25);
            this.credNotifyCheckbox.Name = "credNotifyCheckbox";
            this.credNotifyCheckbox.Size = new System.Drawing.Size(264, 19);
            this.credNotifyCheckbox.TabIndex = 0;
            this.credNotifyCheckbox.Text = "Show notification when credentials are requested";
            this.credNotifyCheckbox.UseVisualStyleBackColor = true;
            // 
            // credMatchingCheckbox
            // 
            this.credMatchingCheckbox.AutoSize = true;
            this.credMatchingCheckbox.Location = new System.Drawing.Point(15, 55);
            this.credMatchingCheckbox.Name = "credMatchingCheckbox";
            this.credMatchingCheckbox.Size = new System.Drawing.Size(325, 19);
            this.credMatchingCheckbox.TabIndex = 1;
            this.credMatchingCheckbox.Text = "Return only best matching entries for an URL instead of all";
            this.credMatchingCheckbox.UseVisualStyleBackColor = true;
            // 
            // unlockDatabaseCheckbox
            // 
            this.unlockDatabaseCheckbox.AutoSize = true;
            this.unlockDatabaseCheckbox.Location = new System.Drawing.Point(15, 85);
            this.unlockDatabaseCheckbox.Name = "unlockDatabaseCheckbox";
            this.unlockDatabaseCheckbox.Size = new System.Drawing.Size(256, 19);
            this.unlockDatabaseCheckbox.TabIndex = 2;
            this.unlockDatabaseCheckbox.Text = "Request for unlocking the database if locked";
            this.unlockDatabaseCheckbox.UseVisualStyleBackColor = true;
            // 
            // hideExpiredCheckbox
            // 
            this.hideExpiredCheckbox.AutoSize = true;
            this.hideExpiredCheckbox.Location = new System.Drawing.Point(15, 115);
            this.hideExpiredCheckbox.Name = "hideExpiredCheckbox";
            this.hideExpiredCheckbox.Size = new System.Drawing.Size(185, 19);
            this.hideExpiredCheckbox.TabIndex = 3;
            this.hideExpiredCheckbox.Text = "Do not return expired entries";
            this.hideExpiredCheckbox.UseVisualStyleBackColor = true;
            // 
            // matchSchemesCheckbox
            // 
            this.matchSchemesCheckbox.AutoSize = true;
            this.matchSchemesCheckbox.Location = new System.Drawing.Point(15, 145);
            this.matchSchemesCheckbox.Name = "matchSchemesCheckbox";
            this.matchSchemesCheckbox.Size = new System.Drawing.Size(258, 19);
            this.matchSchemesCheckbox.TabIndex = 4;
            this.matchSchemesCheckbox.Text = "Match URL schemes (http, https separation)";
            this.matchSchemesCheckbox.UseVisualStyleBackColor = true;
            // 
            // chkUseKpxcSettingsGeneral
            // 
            this.chkUseKpxcSettingsGeneral.AutoSize = true;
            this.chkUseKpxcSettingsGeneral.Location = new System.Drawing.Point(15, 175);
            this.chkUseKpxcSettingsGeneral.Name = "chkUseKpxcSettingsGeneral";
            this.chkUseKpxcSettingsGeneral.Size = new System.Drawing.Size(282, 19);
            this.chkUseKpxcSettingsGeneral.TabIndex = 5;
            this.chkUseKpxcSettingsGeneral.Text = "Use KeePassXC-Browser settings (recommended)";
            this.chkUseKpxcSettingsGeneral.UseVisualStyleBackColor = true;
            // 
            // 
            // 
            // grpSorting
            // 
            this.grpSorting.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpSorting.Controls.Add(this.SortByTitleRadioButton);
            this.grpSorting.Controls.Add(this.SortByUsernameRadioButton);
            this.grpSorting.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpSorting.Location = new System.Drawing.Point(10, 375);
            this.grpSorting.Name = "grpSorting";
            this.grpSorting.Size = new System.Drawing.Size(510, 60);
            this.grpSorting.TabIndex = 2;
            this.grpSorting.TabStop = false;
            this.grpSorting.Text = "Result Sorting";
            // 
            // SortByTitleRadioButton
            // 
            this.SortByTitleRadioButton.AutoSize = true;
            this.SortByTitleRadioButton.Location = new System.Drawing.Point(15, 25);
            this.SortByTitleRadioButton.Name = "SortByTitleRadioButton";
            this.SortByTitleRadioButton.Size = new System.Drawing.Size(87, 19);
            this.SortByTitleRadioButton.TabIndex = 0;
            this.SortByTitleRadioButton.TabStop = true;
            this.SortByTitleRadioButton.Text = "Sort by title";
            this.SortByTitleRadioButton.UseVisualStyleBackColor = true;
            // 
            // SortByUsernameRadioButton
            // 
            this.SortByUsernameRadioButton.AutoSize = true;
            this.SortByUsernameRadioButton.Location = new System.Drawing.Point(150, 25);
            this.SortByUsernameRadioButton.Name = "SortByUsernameRadioButton";
            this.SortByUsernameRadioButton.Size = new System.Drawing.Size(119, 19);
            this.SortByUsernameRadioButton.TabIndex = 1;
            this.SortByUsernameRadioButton.TabStop = true;
            this.SortByUsernameRadioButton.Text = "Sort by username";
            this.SortByUsernameRadioButton.UseVisualStyleBackColor = true;
            // 
            // lblVersion
            // 
            this.lblVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblVersion.AutoSize = true;
            this.lblVersion.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblVersion.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblVersion.Location = new System.Drawing.Point(16, 527);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(127, 13);
            this.lblVersion.TabIndex = 3;
            this.lblVersion.Text = "KeePassNatMsg v2.2.0.0";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.grpSearchDb);
            this.tabPage2.Controls.Add(this.grpDefaultGroup);
            this.tabPage2.Controls.Add(this.grpFields);
            this.tabPage2.Controls.Add(this.grpDangerZone);
            this.tabPage2.Controls.Add(this.labelConnDb);
            this.tabPage2.Controls.Add(this.comboBoxDatabases);
            this.tabPage2.Controls.Add(this.chkUseKpxcSettingsKey);
            this.tabPage2.Controls.Add(this.txtKPXCVerOverride);
            this.tabPage2.Controls.Add(this.lblKPXCVerOverride);
            this.tabPage2.Controls.Add(this.btnMigrateSettings);
            this.tabPage2.Controls.Add(this.btnCheckForLegacyConfig);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(10);
            this.tabPage2.Size = new System.Drawing.Size(530, 472);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Advanced";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // grpSearchDb
            // 
            this.grpSearchDb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpSearchDb.Controls.Add(this.credOnlySearchInSelectedDatabaseRadioButton);
            this.grpSearchDb.Controls.Add(this.credSearchInAllOpenedDatabasesRadioButton);
            this.grpSearchDb.Controls.Add(this.credRestrictSearchInSpecificDatabaseRadioButton);
            this.grpSearchDb.Controls.Add(this.comboBoxSearchDatabases);
            this.grpSearchDb.Location = new System.Drawing.Point(10, 10);
            this.grpSearchDb.Name = "grpSearchDb";
            this.grpSearchDb.Size = new System.Drawing.Size(510, 95);
            this.grpSearchDb.TabIndex = 0;
            this.grpSearchDb.TabStop = false;
            this.grpSearchDb.Text = "Database Search Scope";
            // 
            // credOnlySearchInSelectedDatabaseRadioButton
            // 
            this.credOnlySearchInSelectedDatabaseRadioButton.AutoSize = true;
            this.credOnlySearchInSelectedDatabaseRadioButton.Location = new System.Drawing.Point(15, 20);
            this.credOnlySearchInSelectedDatabaseRadioButton.Name = "credOnlySearchInSelectedDatabaseRadioButton";
            this.credOnlySearchInSelectedDatabaseRadioButton.Size = new System.Drawing.Size(201, 17);
            this.credOnlySearchInSelectedDatabaseRadioButton.TabIndex = 0;
            this.credOnlySearchInSelectedDatabaseRadioButton.TabStop = true;
            this.credOnlySearchInSelectedDatabaseRadioButton.Text = "Search in only selected database";
            this.credOnlySearchInSelectedDatabaseRadioButton.UseVisualStyleBackColor = true;
            // 
            // credSearchInAllOpenedDatabasesRadioButton
            // 
            this.credSearchInAllOpenedDatabasesRadioButton.AutoSize = true;
            this.credSearchInAllOpenedDatabasesRadioButton.Location = new System.Drawing.Point(15, 42);
            this.credSearchInAllOpenedDatabasesRadioButton.Name = "credSearchInAllOpenedDatabasesRadioButton";
            this.credSearchInAllOpenedDatabasesRadioButton.Size = new System.Drawing.Size(184, 17);
            this.credSearchInAllOpenedDatabasesRadioButton.TabIndex = 1;
            this.credSearchInAllOpenedDatabasesRadioButton.TabStop = true;
            this.credSearchInAllOpenedDatabasesRadioButton.Text = "Search in all opened databases";
            this.credSearchInAllOpenedDatabasesRadioButton.UseVisualStyleBackColor = true;
            // 
            // credRestrictSearchInSpecificDatabaseRadioButton
            // 
            this.credRestrictSearchInSpecificDatabaseRadioButton.AutoSize = true;
            this.credRestrictSearchInSpecificDatabaseRadioButton.Location = new System.Drawing.Point(15, 64);
            this.credRestrictSearchInSpecificDatabaseRadioButton.Name = "credRestrictSearchInSpecificDatabaseRadioButton";
            this.credRestrictSearchInSpecificDatabaseRadioButton.Size = new System.Drawing.Size(187, 17);
            this.credRestrictSearchInSpecificDatabaseRadioButton.TabIndex = 2;
            this.credRestrictSearchInSpecificDatabaseRadioButton.TabStop = true;
            this.credRestrictSearchInSpecificDatabaseRadioButton.Text = "Restrict search to specific DB:";
            this.credRestrictSearchInSpecificDatabaseRadioButton.UseVisualStyleBackColor = true;
            // 
            // comboBoxSearchDatabases
            // 
            this.comboBoxSearchDatabases.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSearchDatabases.FormattingEnabled = true;
            this.comboBoxSearchDatabases.Location = new System.Drawing.Point(210, 63);
            this.comboBoxSearchDatabases.Name = "comboBoxSearchDatabases";
            this.comboBoxSearchDatabases.Size = new System.Drawing.Size(280, 21);
            this.comboBoxSearchDatabases.TabIndex = 3;
            // 
            // grpDefaultGroup
            // 
            this.grpDefaultGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpDefaultGroup.Controls.Add(this.lblDefaultGroup);
            this.grpDefaultGroup.Controls.Add(this.txtDefaultGroup);
            this.grpDefaultGroup.Controls.Add(this.chkDefaultGroupAlwaysAllow);
            this.grpDefaultGroup.Location = new System.Drawing.Point(10, 115);
            this.grpDefaultGroup.Name = "grpDefaultGroup";
            this.grpDefaultGroup.Size = new System.Drawing.Size(510, 75);
            this.grpDefaultGroup.TabIndex = 1;
            this.grpDefaultGroup.TabStop = false;
            this.grpDefaultGroup.Text = "Default Group for New Logins";
            // 
            // lblDefaultGroup
            // 
            this.lblDefaultGroup.AutoSize = true;
            this.lblDefaultGroup.Location = new System.Drawing.Point(15, 23);
            this.lblDefaultGroup.Name = "lblDefaultGroup";
            this.lblDefaultGroup.Size = new System.Drawing.Size(71, 13);
            this.lblDefaultGroup.TabIndex = 0;
            this.lblDefaultGroup.Text = "Group Path:";
            // 
            // txtDefaultGroup
            // 
            this.txtDefaultGroup.Location = new System.Drawing.Point(95, 20);
            this.txtDefaultGroup.Name = "txtDefaultGroup";
            this.txtDefaultGroup.Size = new System.Drawing.Size(250, 22);
            this.txtDefaultGroup.TabIndex = 1;
            // 
            // chkDefaultGroupAlwaysAllow
            // 
            this.chkDefaultGroupAlwaysAllow.AutoSize = true;
            this.chkDefaultGroupAlwaysAllow.Location = new System.Drawing.Point(18, 48);
            this.chkDefaultGroupAlwaysAllow.Name = "chkDefaultGroupAlwaysAllow";
            this.chkDefaultGroupAlwaysAllow.Size = new System.Drawing.Size(271, 17);
            this.chkDefaultGroupAlwaysAllow.TabIndex = 2;
            this.chkDefaultGroupAlwaysAllow.Text = "Always allow access for entries in default group";
            this.chkDefaultGroupAlwaysAllow.UseVisualStyleBackColor = true;
            // 
            // grpFields
            // 
            this.grpFields.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpFields.Location = new System.Drawing.Point(10, 198);
            this.grpFields.Name = "grpFields";
            this.grpFields.Size = new System.Drawing.Size(510, 95);
            this.grpFields.TabIndex = 2;
            this.grpFields.TabStop = false;
            this.grpFields.Text = "String Fields & Custom Attributes";
            // 
            // 

            // 
            // 
            // 
            // 
            // 
            // grpDangerZone
            // 
            this.grpDangerZone.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpDangerZone.Controls.Add(this.credAllowAccessCheckbox);
            this.grpDangerZone.Controls.Add(this.credAllowUpdatesCheckbox);
            this.grpDangerZone.Controls.Add(this.removePermissionsButton);
            this.grpDangerZone.ForeColor = System.Drawing.Color.DarkRed;
            this.grpDangerZone.Location = new System.Drawing.Point(10, 300);
            this.grpDangerZone.Name = "grpDangerZone";
            this.grpDangerZone.Size = new System.Drawing.Size(510, 95);
            this.grpDangerZone.TabIndex = 3;
            this.grpDangerZone.TabStop = false;
            this.grpDangerZone.Text = "Danger Zone (Bypass Prompting)";
            // 
            // credAllowAccessCheckbox
            // 
            this.credAllowAccessCheckbox.AutoSize = true;
            this.credAllowAccessCheckbox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.credAllowAccessCheckbox.Location = new System.Drawing.Point(15, 20);
            this.credAllowAccessCheckbox.Name = "credAllowAccessCheckbox";
            this.credAllowAccessCheckbox.Size = new System.Drawing.Size(236, 17);
            this.credAllowAccessCheckbox.TabIndex = 0;
            this.credAllowAccessCheckbox.Text = "Always allow access (skip confirmation)";
            this.credAllowAccessCheckbox.UseVisualStyleBackColor = true;
            // 
            // credAllowUpdatesCheckbox
            // 
            this.credAllowUpdatesCheckbox.AutoSize = true;
            this.credAllowUpdatesCheckbox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.credAllowUpdatesCheckbox.Location = new System.Drawing.Point(15, 43);
            this.credAllowUpdatesCheckbox.Name = "credAllowUpdatesCheckbox";
            this.credAllowUpdatesCheckbox.Size = new System.Drawing.Size(243, 17);
            this.credAllowUpdatesCheckbox.TabIndex = 1;
            this.credAllowUpdatesCheckbox.Text = "Always allow updates (skip confirmation)";
            this.credAllowUpdatesCheckbox.UseVisualStyleBackColor = true;
            // 
            // removePermissionsButton
            // 
            this.removePermissionsButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.removePermissionsButton.Location = new System.Drawing.Point(15, 65);
            this.removePermissionsButton.Name = "removePermissionsButton";
            this.removePermissionsButton.Size = new System.Drawing.Size(280, 23);
            this.removePermissionsButton.TabIndex = 2;
            this.removePermissionsButton.Text = "Remove all stored permissions from entries";
            this.removePermissionsButton.UseVisualStyleBackColor = true;
            this.removePermissionsButton.Click += new System.EventHandler(this.removePermissionsButton_Click);
            // 
            // labelConnDb
            // 
            this.labelConnDb.AutoSize = true;
            this.labelConnDb.Location = new System.Drawing.Point(15, 408);
            this.labelConnDb.Name = "labelConnDb";
            this.labelConnDb.Size = new System.Drawing.Size(123, 13);
            this.labelConnDb.TabIndex = 4;
            this.labelConnDb.Text = "Connection Database:";
            // 
            // comboBoxDatabases
            // 
            this.comboBoxDatabases.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDatabases.FormattingEnabled = true;
            this.comboBoxDatabases.Location = new System.Drawing.Point(145, 404);
            this.comboBoxDatabases.Name = "comboBoxDatabases";
            this.comboBoxDatabases.Size = new System.Drawing.Size(210, 21);
            this.comboBoxDatabases.TabIndex = 5;
            // 
            // chkUseKpxcSettingsKey
            // 
            this.chkUseKpxcSettingsKey.AutoSize = true;
            this.chkUseKpxcSettingsKey.Location = new System.Drawing.Point(18, 435);
            this.chkUseKpxcSettingsKey.Name = "chkUseKpxcSettingsKey";
            this.chkUseKpxcSettingsKey.Size = new System.Drawing.Size(177, 17);
            this.chkUseKpxcSettingsKey.TabIndex = 6;
            this.chkUseKpxcSettingsKey.Text = "Use KeePassXC Settings Key";
            this.chkUseKpxcSettingsKey.UseVisualStyleBackColor = true;
            // 
            // txtKPXCVerOverride
            // 
            this.txtKPXCVerOverride.Location = new System.Drawing.Point(430, 404);
            this.txtKPXCVerOverride.Name = "txtKPXCVerOverride";
            this.txtKPXCVerOverride.Size = new System.Drawing.Size(90, 22);
            this.txtKPXCVerOverride.TabIndex = 8;
            // 
            // lblKPXCVerOverride
            // 
            this.lblKPXCVerOverride.AutoSize = true;
            this.lblKPXCVerOverride.Location = new System.Drawing.Point(370, 408);
            this.lblKPXCVerOverride.Name = "lblKPXCVerOverride";
            this.lblKPXCVerOverride.Size = new System.Drawing.Size(53, 13);
            this.lblKPXCVerOverride.TabIndex = 7;
            this.lblKPXCVerOverride.Text = "Ver Over:";
            // 
            // btnMigrateSettings
            // 
            this.btnMigrateSettings.Location = new System.Drawing.Point(215, 432);
            this.btnMigrateSettings.Name = "btnMigrateSettings";
            this.btnMigrateSettings.Size = new System.Drawing.Size(120, 23);
            this.btnMigrateSettings.TabIndex = 9;
            this.btnMigrateSettings.Text = "Migrate Settings";
            this.btnMigrateSettings.UseVisualStyleBackColor = true;
            this.btnMigrateSettings.Click += new System.EventHandler(this.btnMigrateSettings_Click);
            // 
            // btnCheckForLegacyConfig
            // 
            this.btnCheckForLegacyConfig.Location = new System.Drawing.Point(345, 432);
            this.btnCheckForLegacyConfig.Name = "btnCheckForLegacyConfig";
            this.btnCheckForLegacyConfig.Size = new System.Drawing.Size(120, 23);
            this.btnCheckForLegacyConfig.TabIndex = 10;
            this.btnCheckForLegacyConfig.Text = "Check Legacy";
            this.btnCheckForLegacyConfig.UseVisualStyleBackColor = true;
            this.btnCheckForLegacyConfig.Click += new System.EventHandler(this.btnCheckForLegacyConfig_Click);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.scKeysMain);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(530, 472);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Keys";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // scKeysMain
            // 
            this.scKeysMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scKeysMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.scKeysMain.Location = new System.Drawing.Point(3, 3);
            this.scKeysMain.Name = "scKeysMain";
            this.scKeysMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // scKeysMain.Panel1
            // 
            this.scKeysMain.Panel1.Controls.Add(this.dgvKeys);
            // 
            // scKeysMain.Panel2
            // 
            this.scKeysMain.Panel2.Controls.Add(this.btnRemoveAllKeys);
            this.scKeysMain.Panel2.Controls.Add(this.btnRemoveSelectedKeys);
            this.scKeysMain.Size = new System.Drawing.Size(524, 466);
            this.scKeysMain.SplitterDistance = 425;
            this.scKeysMain.TabIndex = 0;
            // 
            // dgvKeys
            // 
            this.dgvKeys.AllowUserToAddRows = false;
            this.dgvKeys.AllowUserToDeleteRows = false;
            this.dgvKeys.AllowUserToResizeRows = false;
            this.dgvKeys.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvKeys.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvKeys.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colName,
            this.colKey});
            this.dgvKeys.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvKeys.Location = new System.Drawing.Point(0, 0);
            this.dgvKeys.Name = "dgvKeys";
            this.dgvKeys.ReadOnly = true;
            this.dgvKeys.RowHeadersVisible = false;
            this.dgvKeys.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvKeys.Size = new System.Drawing.Size(524, 425);
            this.dgvKeys.TabIndex = 0;
            // 
            // colName
            // 
            this.colName.DataPropertyName = "Name";
            this.colName.HeaderText = "Name";
            this.colName.Name = "colName";
            this.colName.ReadOnly = true;
            // 
            // colKey
            // 
            this.colKey.DataPropertyName = "Key";
            this.colKey.HeaderText = "Key";
            this.colKey.Name = "colKey";
            this.colKey.ReadOnly = true;
            // 
            // btnRemoveAllKeys
            // 
            this.btnRemoveAllKeys.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemoveAllKeys.Location = new System.Drawing.Point(400, 6);
            this.btnRemoveAllKeys.Name = "btnRemoveAllKeys";
            this.btnRemoveAllKeys.Size = new System.Drawing.Size(120, 25);
            this.btnRemoveAllKeys.TabIndex = 1;
            this.btnRemoveAllKeys.Text = "Remove All Keys";
            this.btnRemoveAllKeys.UseVisualStyleBackColor = true;
            this.btnRemoveAllKeys.Click += new System.EventHandler(this.btnRemoveAllKeys_Click);
            // 
            // btnRemoveSelectedKeys
            // 
            this.btnRemoveSelectedKeys.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemoveSelectedKeys.Location = new System.Drawing.Point(265, 6);
            this.btnRemoveSelectedKeys.Name = "btnRemoveSelectedKeys";
            this.btnRemoveSelectedKeys.Size = new System.Drawing.Size(125, 25);
            this.btnRemoveSelectedKeys.TabIndex = 0;
            this.btnRemoveSelectedKeys.Text = "Remove Selected";
            this.btnRemoveSelectedKeys.UseVisualStyleBackColor = true;
            this.btnRemoveSelectedKeys.Click += new System.EventHandler(this.btnRemoveSelectedKeys_Click);
            // 
            // OptionsForm
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(562, 558);
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
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
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.grpChrome.ResumeLayout(false);
            this.grpChrome.PerformLayout();
            this.grpMatching.ResumeLayout(false);
            this.grpMatching.PerformLayout();
            this.grpSorting.ResumeLayout(false);
            this.grpSorting.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.grpSearchDb.ResumeLayout(false);
            this.grpSearchDb.PerformLayout();
            this.grpDefaultGroup.ResumeLayout(false);
            this.grpDefaultGroup.PerformLayout();
            this.grpFields.ResumeLayout(false);
            this.grpFields.PerformLayout();
            this.grpDangerZone.ResumeLayout(false);
            this.grpDangerZone.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.scKeysMain.Panel1.ResumeLayout(false);
            this.scKeysMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scKeysMain)).EndInit();
            this.scKeysMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvKeys)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.SplitContainer scKeysMain;
        private System.Windows.Forms.DataGridView dgvKeys;
        private System.Windows.Forms.DataGridViewTextBoxColumn colName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colKey;
        private System.Windows.Forms.Button btnRemoveAllKeys;
        private System.Windows.Forms.Button btnRemoveSelectedKeys;
        private System.Windows.Forms.GroupBox grpChrome;
        private System.Windows.Forms.Label lblChromeStatus;
        private System.Windows.Forms.Button btnInstallChrome;
        private System.Windows.Forms.Button btnUninstallChrome;
        private System.Windows.Forms.Button btnRefreshChrome;
        private System.Windows.Forms.GroupBox grpMatching;
        private System.Windows.Forms.CheckBox credNotifyCheckbox;
        private System.Windows.Forms.CheckBox credMatchingCheckbox;
        private System.Windows.Forms.CheckBox unlockDatabaseCheckbox;
        private System.Windows.Forms.CheckBox hideExpiredCheckbox;
        private System.Windows.Forms.CheckBox matchSchemesCheckbox;
        private System.Windows.Forms.CheckBox chkUseKpxcSettingsGeneral;
        private System.Windows.Forms.GroupBox grpSorting;
        private System.Windows.Forms.RadioButton SortByTitleRadioButton;
        private System.Windows.Forms.RadioButton SortByUsernameRadioButton;
        private System.Windows.Forms.GroupBox grpSearchDb;
        private System.Windows.Forms.RadioButton credOnlySearchInSelectedDatabaseRadioButton;
        private System.Windows.Forms.RadioButton credSearchInAllOpenedDatabasesRadioButton;
        private System.Windows.Forms.RadioButton credRestrictSearchInSpecificDatabaseRadioButton;
        private System.Windows.Forms.ComboBox comboBoxSearchDatabases;
        private System.Windows.Forms.GroupBox grpDefaultGroup;
        private System.Windows.Forms.Label lblDefaultGroup;
        private System.Windows.Forms.TextBox txtDefaultGroup;
        private System.Windows.Forms.CheckBox chkDefaultGroupAlwaysAllow;
        private System.Windows.Forms.GroupBox grpFields;
        private System.Windows.Forms.GroupBox grpDangerZone;
        private System.Windows.Forms.CheckBox credAllowAccessCheckbox;
        private System.Windows.Forms.CheckBox credAllowUpdatesCheckbox;
        private System.Windows.Forms.Button removePermissionsButton;
        private System.Windows.Forms.Label labelConnDb;
        private System.Windows.Forms.ComboBox comboBoxDatabases;
        private System.Windows.Forms.CheckBox chkUseKpxcSettingsKey;
        private System.Windows.Forms.TextBox txtKPXCVerOverride;
        private System.Windows.Forms.Label lblKPXCVerOverride;
        private System.Windows.Forms.Button btnMigrateSettings;
        private System.Windows.Forms.Button btnCheckForLegacyConfig;
    }
}
