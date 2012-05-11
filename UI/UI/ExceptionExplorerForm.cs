namespace ExceptionExplorer.UI
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Windows.Forms;
    using System.ComponentModel;
    using ExceptionExplorer.Config;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Runtime.InteropServices;
    using System.Text;
    using ExceptionExplorer.General;

    /// <summary>
    /// The main window
    /// </summary>
    public partial class ExceptionExplorerForm : Form
    {
        /// <summary>Gets the open dialog.</summary>
        public OpenFileDialog OpenDialog { get; private set; }

        private StartForm startForm;

        protected Options Settings
        {
            get
            {
                return this.Controller.Settings;
            }
        }

        protected ExceptionExplorerController Controller
        {
            get { return this.exceptionControl.Controller; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionExplorerForm"/> class.
        /// </summary>
        public ExceptionExplorerForm()
        {
            this.InitializeComponent();

            Options.Current.Persistence.AddForm(this);

            this.OpenDialog = new OpenFileDialog()
            {
                InitialDirectory = Environment.CurrentDirectory,
                AutoUpgradeEnabled = true,
                CheckFileExists = true,
                Filter = "Assemblies (*.exe; *.dll)|*.exe;*.dll|All Files (*.*)|*.*",
                FilterIndex = 1
            };
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            this.exceptionControl.Controller.Settings.Save();
        }

        /// <summary>
        /// Handles the Load event of the Form1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void ExceptionExplorerForm_Load(object sender, EventArgs e)
        {
            this.toolBar.AddMenuItem(this.mnuFileOpen)
                .AddMenuItem(null)
                .AddMenuItem(this.mnuViewBack)
                .AddMenuItem(this.mnuViewForward)
                .AddMenuItem(this.mnuViewShowSourceCode);

            this.Settings.ShowSource.MenuItem = this.mnuViewShowSourceCode;

            this.Settings.AnalysisOptions.SameAssembly.MenuValueInvert = true;

            this.Settings.MRU.Value.Changed += new EventHandler<EventArgs>(MRU_Changed);
            this.UpdateMru(this.Settings.MRU);

            this.Controller.History.Changed += new EventHandler(HistoryUpdate);
            this.Controller.History.Added += new EventHandler(HistoryUpdate);
            this.HistoryUpdate(this.Controller.History, new EventArgs());

            this.SetActivation();
        }

        internal void SetActivation()
        {
            bool activated = this.Settings.Licence.Status == Licence.LicenceStatus.OK;

            this.mnuHelpLicenceBar.Visible = this.mnuHelpActivate.Visible = this.mnuHelpPurchase.Visible = !activated;
        }

        void HistoryUpdate(object sender, EventArgs e)
        {
            this.mnuViewBack.Enabled = this.Controller.History.CanUndo;
            this.mnuViewForward.Enabled = this.Controller.History.CanRedo;
        }

        private void UpdateMru(MRU mru)
        {
            mnuFileRecent.DropDownItems.Clear();

            List<ToolStripMenuItem> items = new List<ToolStripMenuItem>();

            if (mru.Count == 0)
            {
                items.Add(mnuFileRecentNone);
            }
            else
            {
                int n = 0;
                foreach (string file in mru)
                {
                    n++;
                    string name = ShortPath(file, 60, 75);
                    if (n < 10)
                    {
                        name = String.Format("&{0} {1}", n, name);
                    }
                    else if (n == 10)
                    {
                        name = String.Format("1&0 {1}", n, name);
                    }

                    ToolStripMenuItem item = new ToolStripMenuItem(name)
                    {
                        Tag = file
                    };

                    item.Click += new EventHandler(MRU_Click);
                    items.Add(item);
                }
            }

            mnuFileRecent.DropDownItems.AddRange(items.ToArray());
        }

        void MRU_Click(object sender, EventArgs e)
        {
            ToolStripItem ts = sender as ToolStripItem;
            if (ts != null)
            {
                this.OpenFile(ts.Tag as String);
            }
        }

        private void MRU_Changed(object sender, EventArgs e)
        {
            MRU mru = sender as MRU;

            if (mru == null)
            {
                return;
            }

            this.UpdateMru(mru);

        }

        private OpenFileDialog assemblyLocation = null;

        public string GetAssemblyPath(Assembly requestingAssembly, AssemblyName assemblyName)
        {
            return this.InvokeIfRequired(() => {

                if (this.assemblyLocation == null)
                {
                    this.assemblyLocation = new OpenFileDialog();
                }

                this.assemblyLocation.Title = "Locate " + assemblyName.Name;
                string dir;
                if (requestingAssembly != null)
                {
                    dir = Path.GetDirectoryName(requestingAssembly.Location);
                }
                else
                {
                    dir = Environment.CurrentDirectory;
                }

                this.assemblyLocation.InitialDirectory = dir;
                this.assemblyLocation.AutoUpgradeEnabled = true;
                this.assemblyLocation.CheckFileExists = true;
                this.assemblyLocation.Filter = "Assemblies (*.exe; *.dll)|*.exe;*.dll|All Files (*.*)|*.*";
                this.assemblyLocation.FilterIndex = 1;
                this.assemblyLocation.RestoreDirectory = true;
                this.assemblyLocation.FileName = assemblyName.Name + ".dll";

                if (this.assemblyLocation.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    return this.assemblyLocation.FileName;
                }

                return (string)null;
            });
        }

        [DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
        private static extern bool PathCompactPathEx([Out] StringBuilder pszOut, string szPath, int cchMax, int dwFlags);

        private static string ShortPath(string path, int targetLength, int maxLength)
        {
            string newPath = path;

            if (path.Length <= targetLength)
            {
                // path is short enough
                return newPath;
            }

            // produce nicer output: c:\dir\...\hello\poop.xx
            const string pattern = @"^(\w+:|\\)(\\[^\\]+\\[^\\]+\\).*(\\[^\\]+\\[^\\]+)$";
            const string replacement = "$1$2...$3";
            if (Regex.IsMatch(path, pattern))
            {
                newPath = Regex.Replace(path, pattern, replacement);
            }

            // produce less nice output: c:\dir\aaa\he...\poop.xx
            if (newPath.Length > maxLength)
            {
                StringBuilder sb = new StringBuilder();
                PathCompactPathEx(sb, path, targetLength, 0);
                newPath = sb.ToString();
            }

            return newPath;

        }
        private static bool loadedCommandLine = false;

        /// <summary>
        /// Handles the Shown event of the ExceptionExplorerForm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void ExceptionExplorerForm_Shown(object sender, System.EventArgs e)
        {
            bool opened = false;

            if (!this.Settings.Licence.IsValid)
            {
                Activation.LicenceMessage(this);
            }

            if (!loadedCommandLine)
            {
                loadedCommandLine = false;
                opened = true;

                if (File.Exists(Program.CommandLine.CommandStringNoOptions))
                {
                    this.OpenFile(Program.CommandLine.CommandStringNoOptions.Trim());
                }
                else if (File.Exists(Program.CommandLine.CommandString))
                {
                    this.OpenFile(Program.CommandLine.CommandStringNoOptions.Trim());
                }
                else
                {
                    opened = false;
                    foreach (string file in Program.CommandLine.CommandFiles)
                    {
                        this.OpenFile(file);
                        opened = true;
                    }
                }
            }

            if (this.Settings.Licence.Status == Licence.LicenceStatus.Expired)
            {

            }

            this.Settings.Update.Latest.Changed += new EventHandler<SettingChangedEventArgs>(LatestVersion_Changed);

            if (AppVersion.CheckRequired)
            {
                AppVersion.GetLatestAsync(this, (state) => { }, true);
            }

            this.ShowUpgradeMenuItem();

            if (!opened && this.Settings.ShowStartWindow.Value)
            {
                this.startForm = new StartForm();
                this.startForm.ShowDialog(this);
            }
        }

        private void ShowUpgradeMenuItem()
        {
            if (AppVersion.UpgradeRequired)
            {
                this.mnuUpdate.Visible = true;
            }
            else
            {
                this.mnuUpdate.Visible = false;
            }

        }

        void LatestVersion_Changed(object sender, SettingChangedEventArgs e)
        {
            this.ShowUpgradeMenuItem();
        }
                


        /// <summary>Opens the file.</summary>
        /// <param name="path">The path.</param>
        public void OpenFile(string path)
        {            
            this.Controller.LoadAssembly(path);
        }

        public bool ShowOpenDialog()
        {
            if (this.OpenDialog.ShowDialog(this) != System.Windows.Forms.DialogResult.OK)
            {
                return false;
            }

            this.OpenFile(this.OpenDialog.FileName);
            return true;
        }

        private void mnuFileOpen_Click(object sender, EventArgs e)
        {
            this.ShowOpenDialog();
        }

        private void mnuHelpAbout_Click(object sender, EventArgs e)
        {
            (new AboutForm()).ShowDialog(this);
        }

        private void mnuViewBack_Click(object sender, EventArgs e)
        {
            this.Controller.History.Undo();
        }

        private void mnuViewForward_Click(object sender, EventArgs e)
        {
            this.Controller.History.Redo();
        }

        private void mnuFileExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void mnuViewOptions_Click(object sender, EventArgs e)
        {
            OptionsForm optionsForm = new OptionsForm();
            optionsForm.Show(this);
        }

        private void mnuHelpWebsite_Click(object sender, EventArgs e)
        {
            Web.OpenSite(SiteLink.Home);
        }

        private void mnuHelpPurchase_Click(object sender, EventArgs e)
        {
            Web.OpenSite(SiteLink.Buy);
        }

        private void mnuHelpActivate_Click(object sender, EventArgs e)
        {
            new ActivateForm().Show(this);
        }

        private void mnuHelpUpdate_Click(object sender, EventArgs e)
        {
            AppVersion.GetLatestWithUI(this);
        }

        private void mnuUpdate_Click(object sender, EventArgs e)
        {
            if (AppVersion.CheckRequired)
            {
                AppVersion.GetLatestWithUI(this);
            }
            else
            {
                AppVersion.ShowResponseDialog(this);
            }
        }
    }
}