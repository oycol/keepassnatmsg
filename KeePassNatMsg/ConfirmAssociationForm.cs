using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KeePassNatMsg
{
    public partial class ConfirmAssociationForm : Form
    {
        public ConfirmAssociationForm()
        {
            InitializeComponent();
            Saved = false;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // Inherit the KeePass window icon so the dialog has a proper logo.
            // The caller already assigns f.Icon = win.Icon before ShowDialog, but
            // we also render it into the PictureBox for a visible in-form logo.
            try
            {
                if (this.Icon != null)
                    picLogo.Image = this.Icon.ToBitmap();
            }
            catch { }
        }

        private void Save_Click(object sender, EventArgs e)
        {
            var value = KeyName.Text;
            if (value != null && value.Trim() != "")
            {
                Saved = true;
                Close();
            }
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        public string KeyId
        {
            get
            {
                return Saved ? KeyName.Text : null;
            }
        }

        public bool Saved { get; private set; }
        public string Key
        {
            get
            { 
                return KeyLabel.Text;
            }
            set
            {
                KeyLabel.Text = value;
            }
        }
    }
}
