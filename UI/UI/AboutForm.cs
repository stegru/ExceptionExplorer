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
using System.Globalization;
using System.Diagnostics;
using ExceptionExplorer.Properties;

namespace ExceptionExplorer.UI
{
    public partial class AboutForm : Form
    {
        private bool downloadLink = false;

       
        private void AddLibraries()
        {
            Font normalFont = libraryPanel.Font;
            Font boldFont = new System.Drawing.Font(normalFont, FontStyle.Bold);

            Action<string, bool> label = (text, bold) =>
            {
                Label lbl = new Label()
                {
                    Text = text,
                    Font =  bold ? boldFont : normalFont,
                    AutoSize = true,
                    Visible = true
                };

                libraryPanel.Controls.Add(lbl);
            };

            Action<string, string> link = (text, url) =>
            {
                LinkLabel lnk = new LinkLabel()
                {
                    Text = text,
                    AutoSize = true,
                    Tag = url,
                    Visible = true
                };

                lnk.Click += new EventHandler(Link_Click);

                libraryPanel.Controls.Add(lnk);
            };

            Action<string> licence = (url) =>{
                link("Licence", url);
            };

            Action gap = () =>
            {
                label(" ", false);
            };

            label("ILSpy", true);
            link("www.ilspy.net", "http://www.ilspy.net");
            licence("https://raw.github.com/icsharpcode/ILSpy/master/README.txt");
            gap();
            label("Windows® API Code Pack for Microsoft® .NET Framework", true);
            link("archive.msdn.microsoft.com/WindowsAPICodePack", "http://archive.msdn.microsoft.com/WindowsAPICodePack");
            licence("http://archive.msdn.microsoft.com/WindowsAPICodePack/Project/License.aspx");
            gap();

        }

        void Link_Click(object sender, EventArgs e)
        {
            LinkLabel link = sender as LinkLabel;
            if ((link != null) && !string.IsNullOrEmpty(link.Tag as string))
            {
                Web.OpenUrl(link.Tag as string);
            }
        }

        public void SetVersionText(Image icon, string text)
        {
            this.SetVersionText(icon, text, null, false);
        }

        public void SetVersionText(Image icon, string text, string linkText)
        {
            this.SetVersionText(icon, text, linkText, false);
        }

        public void SetVersionText(Image icon, string text, string linkText, bool download)
        {
            picIcon.Image = icon;
            lblUpdate.Text = text;
            lnkUpdateCheck.Visible = !string.IsNullOrEmpty(linkText);
            lnkUpdateCheck.Text = linkText ?? string.Empty;
            this.downloadLink = download;
        }

        private void RefreshVersionText()
        {
            if (!AppVersion.UpgradeRequired)
            {
                DateTime lastCheck = Options.Current.Update.LastChecked.Value;
                if ((lastCheck <= DateTime.Now) && (lastCheck - DateTime.Now).TotalDays < 4)
                {
                    string check = TimeSince.GetTimeAgo(lastCheck);
                    this.SetVersionText(Resources.info, string.Format("This is the latest version - last checked {0}", check), "Check now");
                }
            }
        }

        public void CheckLatestVersion()
        {
            this.SetVersionText(Resources.loading, "Checking for the latest version...");
            AppVersion.GetLatestAsync(this, this.GotVersionCheck, false);
        }

        private void GotVersionCheck(AppVersion.VersionCheckState state)
        {
            if (state.Response == VersionCheckResponse.Failed)
            {
                this.SetVersionText(Resources.warning, "Last version check failed.", "Retry");
            }
            else
            {
                this.ShowLatestVersion();
            }
        }
        
        private void ShowLatestVersion()
        {
            this.tmrCheckedSince.Enabled = false;
            if (!AppVersion.UpgradeRequired)
            {
                DateTime lastCheck = Options.Current.Update.LastChecked.Value;
                if (lastCheck == DateTime.MinValue)
                {
                    // never been checked
                    this.SetVersionText(Resources.info, "Latest version is unknown.", "Check now");
                }
                else
                {
                    string check = TimeSince.GetTimeAgo(lastCheck);
                    this.SetVersionText(Resources.info, string.Format("This is the latest version - last checked {0}", check), "Check now");
                    this.tmrCheckedSince.Enabled = true;
                }
            }
            else
            {
                this.SetVersionText(Resources.info, string.Format("A new version is available ({0})", AppVersion.Latest.ToString()), "Download", true);
            }
        }

        public AboutForm()
        {
            InitializeComponent();
            
            this.lblVersion.Text = string.Format("Version {0}", AppVersion.Current.ToString(2));
            this.lblVersion.Left = picLogo.Right - this.lblVersion.Width;

            this.lblFullVersion.Text = string.Format("Version: {0}", AppVersion.Current.ToString(4));
            
            if (Options.Current.Licence.IsValid)
            {
                lblLicencee.Text = string.Format("{0}\n{1}", Options.Current.Licence.Name, Options.Current.Licence.Email);
            }

            this.ShowLatestVersion();
            this.AddLibraries();
        }

        private void lnkSite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Web.OpenSite(SiteLink.Home);
        }

        private void lnkUpdateCheck_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (downloadLink)
            {
                Web.OpenSite(SiteLink.NewVersion);
            }
            else
            {
                this.CheckLatestVersion();
            }
        }

        private void updateTextPanel_Resize(object sender, EventArgs e)
        {
            this.updateTextPanel.Top = (this.updatePanel.ClientRectangle.Height - this.updateTextPanel.Height) / 2;
        }

        private void tmrCheckedSince_Tick(object sender, EventArgs e)
        {
            this.RefreshVersionText();
        }
    }
}
