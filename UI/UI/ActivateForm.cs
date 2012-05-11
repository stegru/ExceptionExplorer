using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ExceptionExplorer.Config;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace ExceptionExplorer.UI
{
    public partial class ActivateForm : Form
    {
        public ActivateForm()
        {
            InitializeComponent();
        }

        public void ActivateLicence()
        {
            string text = this.GetLicenceText();
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            Licence lic = Activation.LoadLicenceFromString(text);

            if (!lic.IsValid)
            {
                Dialog.Show(this, "The code you have provided is not valid.", Microsoft.WindowsAPICodePack.Dialogs.TaskDialogStandardButtons.Ok, Microsoft.WindowsAPICodePack.Dialogs.TaskDialogStandardIcon.Information);
                return;
            }

            if (Activation.Activate(lic))
            {
                Dialog.Show(this, "Licence accepted", "Thank you for your purchase.", TaskDialogStandardButtons.Close, TaskDialogStandardIcon.None);
                this.Close();
            }
        }

        private string GetLicenceText()
        {
            string text = this.txtLicence.Text.Trim();

            if (string.IsNullOrEmpty(text) || (text.Length < 200))
            {
                return null;
            }

            return this.txtLicence.Text;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnActivate_Click(object sender, EventArgs e)
        {
            this.ActivateLicence();
        }
    }
}
