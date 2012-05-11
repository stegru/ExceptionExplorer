using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ExceptionExplorer.Config;
using ExceptionExplorer.General;

namespace ExceptionExplorer.UI
{

    /// <summary>
    /// Options dialog
    /// </summary>
    public partial class OptionsForm : Form
    {
        private Options options;

        private static IEnumerable<Control> GetAllControls(Control control)
        {
            IEnumerable<Control> controls = control.Controls.Cast<Control>();

            return controls.SelectMany(ctrl => GetAllControls(ctrl))
                                      .Concat(controls);
        }

        /// <summary>Initializes a new instance of the <see cref="OptionsForm"/> class.</summary>
        public OptionsForm()
        {
            this.options = Options.Current;
            InitializeComponent();

            this.btnApply.EnabledChanged += new EventHandler(btnApply_EnabledChanged);
        }

        void btnApply_EnabledChanged(object sender, EventArgs e)
        {
            //this.btnOK.Text = this.btnApply.Enabled ? "OK" : "Close";
            //this.btnCancel.Enabled = this.btnApply.Enabled;
        }

        private void OptionsForm_Load(object sender, EventArgs e)
        {
            foreach (VersionCheckFrequency freq in Enum.GetValues(typeof(VersionCheckFrequency)))
            {
                int n = this.cboUpdates.Items.Add(freq);
                if (this.options.Update.CheckFrequency.Value == freq)
                {
                    this.cboUpdates.SelectedIndex = n;
                }
            }

            this.HandleControls(true);

            foreach (Control ctl in GetAllControls(this))
            {
                this.SetControlChangedEvent(ctl);
            }

            Options.Current.Persistence.AddForm(this);

            this.ShowUpdate();
        }

        /// <summary>Perform the control/setting link for all the controls.</summary>
        /// <param name="setControlValues">if set to <c>true</c>, set the control values from their settings. Otherwise, retreive the settings from the control.</param>
        private void HandleControls(bool setControlValues)
        {
            if (!setControlValues)
            {
                this.options.InBulkChange = true;
            }

            this.options.InheritedMemberFullname.HandleControl(setControlValues, this.radInheritedMemberFullNames);
            this.options.SeparateBaseClassItem.HandleControl(setControlValues, this.radSeperateBaseClass);

            this.options.AnalysisOptions.IncludeFramework.HandleControl(setControlValues, this.chkIncludeFramework);
            this.options.AnalysisOptions.UseFrameworkDocumented.HandleControl(setControlValues, this.chkDocumentedFramework);

            this.options.ShowWaiting.HandleControl(setControlValues, this.chkShowWait);
            this.options.ShowSource.HandleControl(setControlValues, this.chkShowSource);

            this.options.Source.AutoDecompile.HandleControl(setControlValues, this.chkDecompileSelection);
            this.options.Source.DecompileLanguageFeatures.HandleControl(setControlValues, this.chkDecompileLanguageFeatures);
            this.options.Source.ShowXmlDoc.HandleControl(setControlValues, this.chkShowXmlDoc);
            this.options.Source.ShowUsing.HandleControl(setControlValues, this.chkShowUsing);
            this.options.ShowStartWindow.HandleControl(setControlValues, this.chkStartScreen);

            if (setControlValues)
            {
                XmlDocumentationUsage docUsage = this.options.AnalysisOptions.XmlDocumentation.Value;

                this.radXmlDocPrefer.Checked = docUsage == XmlDocumentationUsage.Prefer;
                this.radXmlDocCombine.Checked = docUsage == XmlDocumentationUsage.Combine;
                this.radXmlDocNever.Checked = docUsage == XmlDocumentationUsage.Never;
                this.radXmlDocOnly.Checked = docUsage == XmlDocumentationUsage.Only;
            }
            else
            {
                if (this.radXmlDocOnly.Checked)
                {
                    this.options.AnalysisOptions.XmlDocumentation.Value = XmlDocumentationUsage.Only;
                }
                else if (this.radXmlDocCombine.Checked)
                {
                    this.options.AnalysisOptions.XmlDocumentation.Value = XmlDocumentationUsage.Combine;
                }
                else if (this.radXmlDocNever.Checked)
                {
                    this.options.AnalysisOptions.XmlDocumentation.Value = XmlDocumentationUsage.Never;
                }
                else if (this.radXmlDocPrefer.Checked)
                {
                    this.options.AnalysisOptions.XmlDocumentation.Value = XmlDocumentationUsage.Prefer;
                }
            }

            if (setControlValues)
            {
                this.optNoDistinction.Checked = !(this.radInheritedMemberFullNames.Checked || this.radSeperateBaseClass.Checked);
            }

            if (!setControlValues)
            {
                this.options.Update.CheckFrequency.Value = (VersionCheckFrequency)this.cboUpdates.SelectedItem;
            }

            this.btnApply.Enabled = false;

            if (!setControlValues)
            {
                this.options.InBulkChange = false;
            }
        }

        /// <summary>Sets the control changed event.</summary>
        /// <param name="control">The control.</param>
        private void SetControlChangedEvent(Control control)
        {
            if (control is CheckBox)
            {
                ((CheckBox)control).CheckedChanged += new EventHandler(ControlValueChanged);
            }
            else if (control is TextBox)
            {
                control.TextChanged += new EventHandler(ControlValueChanged);
            }
            else if (control is RadioButton)
            {
                ((RadioButton)control).CheckedChanged += new EventHandler(ControlValueChanged);
            }

        }

        /// <summary>Handles a value change event from a control, enables the Apply button.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void ControlValueChanged(object sender, EventArgs e)
        {
            this.btnApply.Enabled = true;
        }


        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.HandleControls(false);
            this.Close();
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            this.HandleControls(false);
        }

        private void chkShowSource_CheckedChanged(object sender, EventArgs e)
        {
            this.chkShowUsing.Enabled =
                this.chkDecompileLanguageFeatures.Enabled =
                this.chkShowXmlDoc.Enabled = 
                this.chkDecompileSelection.Enabled =
                    chkShowSource.Checked;
        }

        private void ShowUpdate()
        {
            if (this.options.Update.LastChecked.Value == DateTime.MinValue)
            {
                lblLastCheck.Text = "never";
                lblLastResponse.Text = "n/a";
                this.tmrUpdate.Enabled = false;
                return;
            }

            string check = TimeSince.GetTimeAgo(this.options.Update.LastChecked.Value);
            lblLastCheck.Text = check.Substring(0, 1).ToUpper() + check.Substring(1);
            this.tmrUpdate.Enabled = true;

            switch (this.options.Update.LastResponse.Value)
            {
                case VersionCheckResponse.OK:
                    lblLastResponse.Text = "You are up to date.";
                    lnkUpdate.Visible = false;
                    break;

                case VersionCheckResponse.NewVersion:
                    lblLastResponse.Text = "New version available: " + AppVersion.Latest;
                    lnkUpdate.Visible = true;
                    break;

                case VersionCheckResponse.Failed:
                    lblLastResponse.Text = "Check for update failed";
                    lnkUpdate.Visible = false;
                    break;
            }
        }

        private void settingsTabs_TabIndexChanged(object sender, EventArgs e)
        {
            if (settingsTabs.SelectedTab.Name == "tabUpdate")
            {
                this.tmrUpdate.Enabled = true;
            }
            else
            {
                this.tmrUpdate.Enabled = false;
            }
        }

        private void tmrUpdate_Tick(object sender, EventArgs e)
        {
            this.ShowUpdate();
        }

        private void btnCheckNow_Click(object sender, EventArgs e)
        {
            this.tmrUpdate.Enabled = false;
            this.lblLastCheck.Text = "Checking now...";
            this.lblLastResponse.Text = "";
            AppVersion.GetLatestAsync(this, (state) =>
            {
                this.ShowUpdate();
            }, true);
        }

        private void lnkUpdate_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Web.OpenSite(SiteLink.NewVersion);
        }

    }
}
