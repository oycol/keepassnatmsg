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
            this.tabMatching = new System.Windows.Forms.TabPage();
            this.grpMatching = new System.Windows.Forms.GroupBox();
            this.credNotifyCheckbox = new System.Windows.Forms.CheckBox();
            this.credMatchingCheckbox = new System.Windows.Forms.CheckBox();
            this.unlockDatabaseCheckbox = new System.Windows.Forms.CheckBox();
            this.hideExpiredCheckbox = new System.Windows.Forms.CheckBox();
            this.matchSchemesCheckbox = new System.Windows.Forms.CheckBox();
            this.chkSearchUrls = new System.Windows.Forms.CheckBox();
            this.grpSorting = new System.Windows.Forms.GroupBox();
            this.SortByTitleRadioButton = new System.Windows.Forms.RadioButton();
            this.SortByUsernameRadioButton = new System.Windows.Forms.RadioButton();
            this.tabDatabase = new System.Windows.Forms.TabPage();
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
            this.picFormLogo = new System.Windows.Forms.PictureBox();
            this.lblVersion = new System.Windows.Forms.Label();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.tabControl.SuspendLayout();
            this.tabIntegration.SuspendLayout();
            this.grpIntegration.SuspendLayout();
            this.tabMatching.SuspendLayout();
            this.grpMatching.SuspendLayout();
            this.grpSorting.SuspendLayout();
            this.tabDatabase.SuspendLayout();
            this.grpDatabase.SuspendLayout();
            this.grpNewLogins.SuspendLayout();
            this.grpDangerZone.SuspendLayout();
            this.tabAssociations.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvKeys)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picFormLogo)).BeginInit();
            this.SuspendLayout();
            //
            // tabControl
            //
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.tabIntegration);
            this.tabControl.Controls.Add(this.tabMatching);
            this.tabControl.Controls.Add(this.tabDatabase);
            this.tabControl.Controls.Add(this.tabAssociations);
            this.tabControl.Location = new System.Drawing.Point(12, 12);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(696, 520);
            this.tabControl.TabIndex = 0;
            this.tabControl.Selected += new System.Windows.Forms.TabControlEventHandler(this.tabControl_Selected);
            //
            // tabIntegration
            //
            this.tabIntegration.Controls.Add(this.grpIntegration);
            this.tabIntegration.Location = new System.Drawing.Point(4, 24);
            this.tabIntegration.Name = "tabIntegration";
            this.tabIntegration.Padding = new System.Windows.Forms.Padding(14);
            this.tabIntegration.Size = new System.Drawing.Size(688, 492);
            this.tabIntegration.TabIndex = 0;
            this.tabIntegration.Text = "Browser Integration";
            this.tabIntegration.UseVisualStyleBackColor = true;
            //
            // grpIntegration
            //
            this.grpIntegration.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
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
            this.grpIntegration.Size = new System.Drawing.Size(660, 310);
            this.grpIntegration.TabIndex = 0;
            this.grpIntegration.TabStop = false;
            this.grpIntegration.Text = "Google Chrome & Microsoft Edge Native Messaging";
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
            // lblProxyStatus
            //
            this.lblProxyStatus.AutoSize = true;
            this.lblProxyStatus.Location = new System.Drawing.Point(20, 68);
            this.lblProxyStatus.Name = "lblProxyStatus";
            this.lblProxyStatus.Size = new System.Drawing.Size(79, 15);
            this.lblProxyStatus.TabIndex = 1;
            this.lblProxyStatus.Text = "Proxy: ...";
            //
            // lblManifestStatus
            //
            this.lblManifestStatus.AutoSize = true;
            this.lblManifestStatus.Location = new System.Drawing.Point(20, 94);
            this.lblManifestStatus.Name = "lblManifestStatus";
            this.lblManifestStatus.Size = new System.Drawing.Size(91, 15);
            this.lblManifestStatus.TabIndex = 2;
            this.lblManifestStatus.Text = "Manifest: ...";
            //
            // lblChromeStatus
            //
            this.lblChromeStatus.AutoSize = true;
            this.lblChromeStatus.Location = new System.Drawing.Point(20, 120);
            this.lblChromeStatus.Name = "lblChromeStatus";
            this.lblChromeStatus.Size = new System.Drawing.Size(107, 15);
            this.lblChromeStatus.TabIndex = 3;
            this.lblChromeStatus.Text = "Google Chrome: ...";
            //
            // lblEdgeStatus
            //
            this.lblEdgeStatus.AutoSize = true;
            this.lblEdgeStatus.Location = new System.Drawing.Point(20, 146);
            this.lblEdgeStatus.Name = "lblEdgeStatus";
            this.lblEdgeStatus.Size = new System.Drawing.Size(103, 15);
            this.lblEdgeStatus.TabIndex = 4;
            this.lblEdgeStatus.Text = "Microsoft Edge: ...";
            //
            // lblIntegrationHint
            //
            this.lblIntegrationHint.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblIntegrationHint.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblIntegrationHint.Location = new System.Drawing.Point(20, 180);
            this.lblIntegrationHint.Name = "lblIntegrationHint";
            this.lblIntegrationHint.Size = new System.Drawing.Size(620, 48);
            this.lblIntegrationHint.TabIndex = 1;
            this.lblIntegrationHint.Text = "Install / Repair deploys the bundled proxy to %LOCALAPPDATA%\\KeePassNatMsg and configures user-level registry keys for Chrome & Edge. No admin rights or external scripts required. Restart browsers after install.";
            // 
            // btnInstallIntegration
            //
            this.btnInstallIntegration.Location = new System.Drawing.Point(20, 246);
            this.btnInstallIntegration.Name = "btnInstallIntegration";
            this.btnInstallIntegration.Size = new System.Drawing.Size(190, 34);
            this.btnInstallIntegration.TabIndex = 6;
            this.btnInstallIntegration.Text = "Install / Repair Integration";
            this.btnInstallIntegration.UseVisualStyleBackColor = true;
            this.btnInstallIntegration.Click += new System.EventHandler(this.btnInstallIntegration_Click);
            //
            // btnRefreshIntegration
            //
            this.btnRefreshIntegration.Location = new System.Drawing.Point(220, 246);
            this.btnRefreshIntegration.Name = "btnRefreshIntegration";
            this.btnRefreshIntegration.Size = new System.Drawing.Size(105, 34);
            this.btnRefreshIntegration.TabIndex = 7;
            this.btnRefreshIntegration.Text = "Refresh";
            this.btnRefreshIntegration.UseVisualStyleBackColor = true;
            this.btnRefreshIntegration.Click += new System.EventHandler(this.btnRefreshIntegration_Click);
            //
            // btnUninstallIntegration
            //
            this.btnUninstallIntegration.Location = new System.Drawing.Point(335, 246);
            this.btnUninstallIntegration.Name = "btnUninstallIntegration";
            this.btnUninstallIntegration.Size = new System.Drawing.Size(105, 34);
            this.btnUninstallIntegration.TabIndex = 8;
            this.btnUninstallIntegration.Text = "Uninstall";
            this.btnUninstallIntegration.UseVisualStyleBackColor = true;
            this.btnUninstallIntegration.Click += new System.EventHandler(this.btnUninstallIntegration_Click);
            //
            // tabMatching
            //
            this.tabMatching.Controls.Add(this.grpMatching);
            this.tabMatching.Controls.Add(this.grpSorting);
            this.tabMatching.Location = new System.Drawing.Point(4, 24);
            this.tabMatching.Name = "tabMatching";
            this.tabMatching.Padding = new System.Windows.Forms.Padding(14);
            this.tabMatching.Size = new System.Drawing.Size(688, 492);
            this.tabMatching.TabIndex = 1;
            this.tabMatching.Text = "Matching Rules";
            this.tabMatching.UseVisualStyleBackColor = true;
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
            this.grpMatching.Controls.Add(this.chkSearchUrls);
            this.grpMatching.Location = new System.Drawing.Point(14, 14);
            this.grpMatching.Name = "grpMatching";
            this.grpMatching.Size = new System.Drawing.Size(660, 220);
            this.grpMatching.TabIndex = 0;
            this.grpMatching.TabStop = false;
            this.grpMatching.Text = "Credential & URL Matching";
            //
            // credNotifyCheckbox
            //
            this.credNotifyCheckbox.AutoSize = true;
            this.credNotifyCheckbox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.credNotifyCheckbox.Location = new System.Drawing.Point(18, 24);
            this.credNotifyCheckbox.Name = "credNotifyCheckbox";
            this.credNotifyCheckbox.Size = new System.Drawing.Size(254, 19);
            this.credNotifyCheckbox.TabIndex = 0;
            this.credNotifyCheckbox.Text = "Notify when credentials are requested";
            this.credNotifyCheckbox.UseVisualStyleBackColor = true;
            // 
            // credMatchingCheckbox
            //
            this.credMatchingCheckbox.AutoSize = true;
            this.credMatchingCheckbox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.credMatchingCheckbox.Location = new System.Drawing.Point(18, 56);
            this.credMatchingCheckbox.Name = "credMatchingCheckbox";
            this.credMatchingCheckbox.Size = new System.Drawing.Size(206, 19);
            this.credMatchingCheckbox.TabIndex = 2;
            this.credMatchingCheckbox.Text = "Return only the best URL matches";
            this.credMatchingCheckbox.UseVisualStyleBackColor = true;
            // 
            // unlockDatabaseCheckbox
            //
            this.unlockDatabaseCheckbox.AutoSize = true;
            this.unlockDatabaseCheckbox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.unlockDatabaseCheckbox.Location = new System.Drawing.Point(18, 88);
            this.unlockDatabaseCheckbox.Name = "unlockDatabaseCheckbox";
            this.unlockDatabaseCheckbox.Size = new System.Drawing.Size(222, 19);
            this.unlockDatabaseCheckbox.TabIndex = 4;
            this.unlockDatabaseCheckbox.Text = "Request database unlock when needed";
            this.unlockDatabaseCheckbox.UseVisualStyleBackColor = true;
            // 
            // hideExpiredCheckbox
            //
            this.hideExpiredCheckbox.AutoSize = true;
            this.hideExpiredCheckbox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.hideExpiredCheckbox.Location = new System.Drawing.Point(18, 120);
            this.hideExpiredCheckbox.Name = "hideExpiredCheckbox";
            this.hideExpiredCheckbox.Size = new System.Drawing.Size(166, 19);
            this.hideExpiredCheckbox.TabIndex = 6;
            this.hideExpiredCheckbox.Text = "Exclude expired entries";
            this.hideExpiredCheckbox.UseVisualStyleBackColor = true;
            // 
            // matchSchemesCheckbox
            //
            this.matchSchemesCheckbox.AutoSize = true;
            this.matchSchemesCheckbox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.matchSchemesCheckbox.Location = new System.Drawing.Point(18, 152);
            this.matchSchemesCheckbox.Name = "matchSchemesCheckbox";
            this.matchSchemesCheckbox.Size = new System.Drawing.Size(206, 19);
            this.matchSchemesCheckbox.TabIndex = 8;
            this.matchSchemesCheckbox.Text = "Require matching URL scheme";
            this.matchSchemesCheckbox.UseVisualStyleBackColor = true;
            // 
            // chkSearchUrls
            //
            this.chkSearchUrls.AutoSize = true;
            this.chkSearchUrls.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkSearchUrls.Location = new System.Drawing.Point(18, 184);
            this.chkSearchUrls.Name = "chkSearchUrls";
            this.chkSearchUrls.Size = new System.Drawing.Size(244, 19);
            this.chkSearchUrls.TabIndex = 10;
            this.chkSearchUrls.Text = "Search additional URL and KP2A_URL fields";
            this.chkSearchUrls.UseVisualStyleBackColor = true;
            // 
            // grpSorting
            //
            this.grpSorting.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpSorting.Controls.Add(this.SortByTitleRadioButton);
            this.grpSorting.Controls.Add(this.SortByUsernameRadioButton);
            this.grpSorting.Location = new System.Drawing.Point(14, 246);
            this.grpSorting.Name = "grpSorting";
            this.grpSorting.Size = new System.Drawing.Size(660, 68);
            this.grpSorting.TabIndex = 1;
            this.grpSorting.TabStop = false;
            this.grpSorting.Text = "Result Sorting Order";
            //
            // SortByTitleRadioButton
            //
            this.SortByTitleRadioButton.AutoSize = true;
            this.SortByTitleRadioButton.Location = new System.Drawing.Point(20, 28);
            this.SortByTitleRadioButton.Name = "SortByTitleRadioButton";
            this.SortByTitleRadioButton.Size = new System.Drawing.Size(86, 19);
            this.SortByTitleRadioButton.TabIndex = 0;
            this.SortByTitleRadioButton.TabStop = true;
            this.SortByTitleRadioButton.Text = "Sort by Title";
            this.SortByTitleRadioButton.UseVisualStyleBackColor = true;
            //
            // SortByUsernameRadioButton
            //
            this.SortByUsernameRadioButton.AutoSize = true;
            this.SortByUsernameRadioButton.Location = new System.Drawing.Point(160, 28);
            this.SortByUsernameRadioButton.Name = "SortByUsernameRadioButton";
            this.SortByUsernameRadioButton.Size = new System.Drawing.Size(118, 19);
            this.SortByUsernameRadioButton.TabIndex = 1;
            this.SortByUsernameRadioButton.TabStop = true;
            this.SortByUsernameRadioButton.Text = "Sort by Username";
            this.SortByUsernameRadioButton.UseVisualStyleBackColor = true;
            //
            // tabDatabase
            //
            this.tabDatabase.Controls.Add(this.grpDatabase);
            this.tabDatabase.Controls.Add(this.grpNewLogins);
            this.tabDatabase.Controls.Add(this.grpDangerZone);
            this.tabDatabase.Location = new System.Drawing.Point(4, 24);
            this.tabDatabase.Name = "tabDatabase";
            this.tabDatabase.Padding = new System.Windows.Forms.Padding(14);
            this.tabDatabase.Size = new System.Drawing.Size(688, 492);
            this.tabDatabase.TabIndex = 2;
            this.tabDatabase.Text = "Database & Security";
            this.tabDatabase.UseVisualStyleBackColor = true;
            //
            // grpDatabase
            //
            this.grpDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpDatabase.Controls.Add(this.credOnlySearchInSelectedDatabaseRadioButton);
            this.grpDatabase.Controls.Add(this.credSearchInAllOpenedDatabasesRadioButton);
            this.grpDatabase.Controls.Add(this.credRestrictSearchInSpecificDatabaseRadioButton);
            this.grpDatabase.Controls.Add(this.comboBoxSearchDatabases);
            this.grpDatabase.Controls.Add(this.labelConnDb);
            this.grpDatabase.Controls.Add(this.comboBoxDatabases);
            this.grpDatabase.Location = new System.Drawing.Point(14, 14);
            this.grpDatabase.Name = "grpDatabase";
            this.grpDatabase.Size = new System.Drawing.Size(660, 140);
            this.grpDatabase.TabIndex = 0;
            this.grpDatabase.TabStop = false;
            this.grpDatabase.Text = "Database Search Scope";
            //
            // credOnlySearchInSelectedDatabaseRadioButton
            //
            this.credOnlySearchInSelectedDatabaseRadioButton.AutoSize = true;
            this.credOnlySearchInSelectedDatabaseRadioButton.Location = new System.Drawing.Point(18, 24);
            this.credOnlySearchInSelectedDatabaseRadioButton.Name = "credOnlySearchInSelectedDatabaseRadioButton";
            this.credOnlySearchInSelectedDatabaseRadioButton.Size = new System.Drawing.Size(201, 19);
            this.credOnlySearchInSelectedDatabaseRadioButton.TabIndex = 0;
            this.credOnlySearchInSelectedDatabaseRadioButton.TabStop = true;
            this.credOnlySearchInSelectedDatabaseRadioButton.Text = "Search only the active database";
            this.credOnlySearchInSelectedDatabaseRadioButton.UseVisualStyleBackColor = true;
            this.credOnlySearchInSelectedDatabaseRadioButton.CheckedChanged += new System.EventHandler(this.rbSearchDatabase_CheckedChanged);
            //
            // credSearchInAllOpenedDatabasesRadioButton
            //
            this.credSearchInAllOpenedDatabasesRadioButton.AutoSize = true;
            this.credSearchInAllOpenedDatabasesRadioButton.Location = new System.Drawing.Point(240, 24);
            this.credSearchInAllOpenedDatabasesRadioButton.Name = "credSearchInAllOpenedDatabasesRadioButton";
            this.credSearchInAllOpenedDatabasesRadioButton.Size = new System.Drawing.Size(184, 19);
            this.credSearchInAllOpenedDatabasesRadioButton.TabIndex = 1;
            this.credSearchInAllOpenedDatabasesRadioButton.TabStop = true;
            this.credSearchInAllOpenedDatabasesRadioButton.Text = "Search all opened databases";
            this.credSearchInAllOpenedDatabasesRadioButton.UseVisualStyleBackColor = true;
            this.credSearchInAllOpenedDatabasesRadioButton.CheckedChanged += new System.EventHandler(this.rbSearchDatabase_CheckedChanged);
            //
            // credRestrictSearchInSpecificDatabaseRadioButton
            //
            this.credRestrictSearchInSpecificDatabaseRadioButton.AutoSize = true;
            this.credRestrictSearchInSpecificDatabaseRadioButton.Location = new System.Drawing.Point(18, 56);
            this.credRestrictSearchInSpecificDatabaseRadioButton.Name = "credRestrictSearchInSpecificDatabaseRadioButton";
            this.credRestrictSearchInSpecificDatabaseRadioButton.Size = new System.Drawing.Size(164, 19);
            this.credRestrictSearchInSpecificDatabaseRadioButton.TabIndex = 2;
            this.credRestrictSearchInSpecificDatabaseRadioButton.TabStop = true;
            this.credRestrictSearchInSpecificDatabaseRadioButton.Text = "Restrict search to target DB:";
            this.credRestrictSearchInSpecificDatabaseRadioButton.UseVisualStyleBackColor = true;
            this.credRestrictSearchInSpecificDatabaseRadioButton.CheckedChanged += new System.EventHandler(this.rbSearchDatabase_CheckedChanged);
            //
            // comboBoxSearchDatabases
            //
            this.comboBoxSearchDatabases.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSearchDatabases.FormattingEnabled = true;
            this.comboBoxSearchDatabases.Location = new System.Drawing.Point(190, 54);
            this.comboBoxSearchDatabases.Name = "comboBoxSearchDatabases";
            this.comboBoxSearchDatabases.Size = new System.Drawing.Size(440, 23);
            this.comboBoxSearchDatabases.TabIndex = 3;
            //
            // labelConnDb
            //
            this.labelConnDb.AutoSize = true;
            this.labelConnDb.Location = new System.Drawing.Point(18, 96);
            this.labelConnDb.Name = "labelConnDb";
            this.labelConnDb.Size = new System.Drawing.Size(123, 15);
            this.labelConnDb.TabIndex = 4;
            this.labelConnDb.Text = "Connection database:";
            //
            // comboBoxDatabases
            //
            this.comboBoxDatabases.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDatabases.FormattingEnabled = true;
            this.comboBoxDatabases.Location = new System.Drawing.Point(190, 93);
            this.comboBoxDatabases.Name = "comboBoxDatabases";
            this.comboBoxDatabases.Size = new System.Drawing.Size(440, 23);
            this.comboBoxDatabases.TabIndex = 5;
            //
            // grpNewLogins
            //
            this.grpNewLogins.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpNewLogins.Controls.Add(this.lblDefaultGroup);
            this.grpNewLogins.Controls.Add(this.txtDefaultGroup);
            this.grpNewLogins.Controls.Add(this.chkDefaultGroupAlwaysAllow);
            this.grpNewLogins.Location = new System.Drawing.Point(14, 164);
            this.grpNewLogins.Name = "grpNewLogins";
            this.grpNewLogins.Size = new System.Drawing.Size(660, 116);
            this.grpNewLogins.TabIndex = 1;
            this.grpNewLogins.TabStop = false;
            this.grpNewLogins.Text = "New Logins & Default Group";
            //
            // lblDefaultGroup
            //
            this.lblDefaultGroup.AutoSize = true;
            this.lblDefaultGroup.Location = new System.Drawing.Point(18, 28);
            this.lblDefaultGroup.Name = "lblDefaultGroup";
            this.lblDefaultGroup.Size = new System.Drawing.Size(108, 15);
            this.lblDefaultGroup.TabIndex = 0;
            this.lblDefaultGroup.Text = "Default group path:";
            //
            // txtDefaultGroup
            //
            this.txtDefaultGroup.Location = new System.Drawing.Point(135, 25);
            this.txtDefaultGroup.Name = "txtDefaultGroup";
            this.txtDefaultGroup.Size = new System.Drawing.Size(495, 23);
            this.txtDefaultGroup.TabIndex = 1;
            //
            // chkDefaultGroupAlwaysAllow
            //
            this.chkDefaultGroupAlwaysAllow.AutoSize = true;
            this.chkDefaultGroupAlwaysAllow.Location = new System.Drawing.Point(21, 60);
            this.chkDefaultGroupAlwaysAllow.Name = "chkDefaultGroupAlwaysAllow";
            this.chkDefaultGroupAlwaysAllow.Size = new System.Drawing.Size(293, 19);
            this.chkDefaultGroupAlwaysAllow.TabIndex = 2;
            this.chkDefaultGroupAlwaysAllow.Text = "Allow entries in the default group without prompting";
            this.chkDefaultGroupAlwaysAllow.UseVisualStyleBackColor = true;
            // 
            // grpDangerZone
            //
            this.grpDangerZone.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpDangerZone.Controls.Add(this.lblDangerWarning);
            this.grpDangerZone.Controls.Add(this.credAllowAccessCheckbox);
            this.grpDangerZone.Controls.Add(this.credAllowUpdatesCheckbox);
            this.grpDangerZone.Controls.Add(this.removePermissionsButton);
            this.grpDangerZone.ForeColor = System.Drawing.Color.DarkRed;
            this.grpDangerZone.Location = new System.Drawing.Point(14, 292);
            this.grpDangerZone.Name = "grpDangerZone";
            this.grpDangerZone.Size = new System.Drawing.Size(660, 150);
            this.grpDangerZone.TabIndex = 2;
            this.grpDangerZone.TabStop = false;
            this.grpDangerZone.Text = "Danger Zone (Prompt Bypass)";
            //
            // lblDangerWarning
            //
            this.lblDangerWarning.ForeColor = System.Drawing.Color.DarkRed;
            this.lblDangerWarning.Location = new System.Drawing.Point(18, 24);
            this.lblDangerWarning.Name = "lblDangerWarning";
            this.lblDangerWarning.Size = new System.Drawing.Size(620, 32);
            this.lblDangerWarning.TabIndex = 0;
            this.lblDangerWarning.Text = "Warning: Bypassing prompts allows any connected browser extension to retrieve or update credentials without approval. Keep disabled unless strictly necessary.";
            //
            // credAllowAccessCheckbox
            //
            this.credAllowAccessCheckbox.AutoSize = true;
            this.credAllowAccessCheckbox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.credAllowAccessCheckbox.Location = new System.Drawing.Point(21, 64);
            this.credAllowAccessCheckbox.Name = "credAllowAccessCheckbox";
            this.credAllowAccessCheckbox.Size = new System.Drawing.Size(210, 19);
            this.credAllowAccessCheckbox.TabIndex = 1;
            this.credAllowAccessCheckbox.Text = "Always allow credential access";
            this.credAllowAccessCheckbox.UseVisualStyleBackColor = true;
            //
            // credAllowUpdatesCheckbox
            //
            this.credAllowUpdatesCheckbox.AutoSize = true;
            this.credAllowUpdatesCheckbox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.credAllowUpdatesCheckbox.Location = new System.Drawing.Point(260, 64);
            this.credAllowUpdatesCheckbox.Name = "credAllowUpdatesCheckbox";
            this.credAllowUpdatesCheckbox.Size = new System.Drawing.Size(204, 19);
            this.credAllowUpdatesCheckbox.TabIndex = 2;
            this.credAllowUpdatesCheckbox.Text = "Always allow credential updates";
            this.credAllowUpdatesCheckbox.UseVisualStyleBackColor = true;
            //
            // removePermissionsButton
            //
            this.removePermissionsButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.removePermissionsButton.Location = new System.Drawing.Point(21, 98);
            this.removePermissionsButton.Name = "removePermissionsButton";
            this.removePermissionsButton.Size = new System.Drawing.Size(260, 32);
            this.removePermissionsButton.TabIndex = 3;
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
            this.tabAssociations.Size = new System.Drawing.Size(688, 492);
            this.tabAssociations.TabIndex = 3;
            this.tabAssociations.Text = "Associations";
            this.tabAssociations.UseVisualStyleBackColor = true;
            //
            // lblKeysHint
            //
            this.lblKeysHint.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblKeysHint.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblKeysHint.Location = new System.Drawing.Point(14, 14);
            this.lblKeysHint.Name = "lblKeysHint";
            this.lblKeysHint.Size = new System.Drawing.Size(650, 36);
            this.lblKeysHint.TabIndex = 0;
            this.lblKeysHint.Text = "Tip: Below are the authorized browser clients for the active database. Key fingerprints (first 8 bytes of SHA-256) are displayed for auditing. Secret keys are never revealed.";
            //
            // dgvKeys
            //
            this.dgvKeys.AllowUserToAddRows = false;
            this.dgvKeys.AllowUserToDeleteRows = false;
            this.dgvKeys.AllowUserToResizeRows = false;
            this.dgvKeys.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvKeys.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvKeys.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvKeys.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colName,
            this.colFingerprint});
            this.dgvKeys.Location = new System.Drawing.Point(14, 58);
            this.dgvKeys.MultiSelect = true;
            this.dgvKeys.Name = "dgvKeys";
            this.dgvKeys.ReadOnly = true;
            this.dgvKeys.RowHeadersVisible = false;
            this.dgvKeys.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvKeys.Size = new System.Drawing.Size(660, 375);
            this.dgvKeys.TabIndex = 1;
            //
            // colName
            //
            this.colName.DataPropertyName = "Name";
            this.colName.HeaderText = "Client Name";
            this.colName.Name = "colName";
            this.colName.ReadOnly = true;
            //
            // colFingerprint
            //
            this.colFingerprint.DataPropertyName = "Fingerprint";
            this.colFingerprint.HeaderText = "Key Fingerprint (SHA-256)";
            this.colFingerprint.Name = "colFingerprint";
            this.colFingerprint.ReadOnly = true;
            //
            // btnRemoveSelectedKeys
            //
            this.btnRemoveSelectedKeys.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemoveSelectedKeys.Location = new System.Drawing.Point(395, 446);
            this.btnRemoveSelectedKeys.Name = "btnRemoveSelectedKeys";
            this.btnRemoveSelectedKeys.Size = new System.Drawing.Size(135, 32);
            this.btnRemoveSelectedKeys.TabIndex = 2;
            this.btnRemoveSelectedKeys.Text = "Remove Selected";
            this.btnRemoveSelectedKeys.UseVisualStyleBackColor = true;
            this.btnRemoveSelectedKeys.Click += new System.EventHandler(this.btnRemoveSelectedKeys_Click);
            //
            // btnRemoveAllKeys
            //
            this.btnRemoveAllKeys.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemoveAllKeys.Location = new System.Drawing.Point(540, 446);
            this.btnRemoveAllKeys.Name = "btnRemoveAllKeys";
            this.btnRemoveAllKeys.Size = new System.Drawing.Size(134, 32);
            this.btnRemoveAllKeys.TabIndex = 3;
            this.btnRemoveAllKeys.Text = "Remove All Keys";
            this.btnRemoveAllKeys.UseVisualStyleBackColor = true;
            this.btnRemoveAllKeys.Click += new System.EventHandler(this.btnRemoveAllKeys_Click);
            //
            // picFormLogo
            //
            this.picFormLogo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.picFormLogo.Location = new System.Drawing.Point(16, 552);
            this.picFormLogo.Name = "picFormLogo";
            this.picFormLogo.Size = new System.Drawing.Size(16, 16);
            this.picFormLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picFormLogo.TabIndex = 1;
            this.picFormLogo.TabStop = false;
            //
            // lblVersion
            //
            this.lblVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblVersion.AutoSize = true;
            this.lblVersion.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblVersion.Location = new System.Drawing.Point(38, 553);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(83, 15);
            this.lblVersion.TabIndex = 2;
            this.lblVersion.Text = "KeePassNatMsg";
            //
            // okButton
            //
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Location = new System.Drawing.Point(525, 544);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(88, 32);
            this.okButton.TabIndex = 3;
            this.okButton.Text = "Save";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            //
            // cancelButton
            //
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(620, 544);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(88, 32);
            this.cancelButton.TabIndex = 4;
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
            this.ClientSize = new System.Drawing.Size(720, 590);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.picFormLogo);
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
            this.tabMatching.ResumeLayout(false);
            this.grpMatching.ResumeLayout(false);
            this.grpMatching.PerformLayout();
            this.grpSorting.ResumeLayout(false);
            this.grpSorting.PerformLayout();
            this.tabDatabase.ResumeLayout(false);
            this.grpDatabase.ResumeLayout(false);
            this.grpDatabase.PerformLayout();
            this.grpNewLogins.ResumeLayout(false);
            this.grpNewLogins.PerformLayout();
            this.grpDangerZone.ResumeLayout(false);
            this.grpDangerZone.PerformLayout();
            this.tabAssociations.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvKeys)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picFormLogo)).EndInit();
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
        private System.Windows.Forms.TabPage tabMatching;
        private System.Windows.Forms.GroupBox grpMatching;
        private System.Windows.Forms.CheckBox credNotifyCheckbox;
        private System.Windows.Forms.CheckBox credMatchingCheckbox;
        private System.Windows.Forms.CheckBox unlockDatabaseCheckbox;
        private System.Windows.Forms.CheckBox hideExpiredCheckbox;
        private System.Windows.Forms.CheckBox matchSchemesCheckbox;
        private System.Windows.Forms.CheckBox chkSearchUrls;
        private System.Windows.Forms.GroupBox grpSorting;
        private System.Windows.Forms.RadioButton SortByTitleRadioButton;
        private System.Windows.Forms.RadioButton SortByUsernameRadioButton;
        private System.Windows.Forms.TabPage tabDatabase;
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
        private System.Windows.Forms.PictureBox picFormLogo;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
    }
}
