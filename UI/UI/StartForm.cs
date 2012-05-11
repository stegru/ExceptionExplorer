using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ExceptionExplorer.Config;
using System.IO;
using Microsoft.Win32;
using System.Security;
using Microsoft.WindowsAPICodePack.Dialogs;


namespace ExceptionExplorer.UI
{
    public partial class StartForm : Form
    {
        public StartForm()
        {
            InitializeComponent();
        }

        private string GetIcon(string file, bool useAssociation)
        {

            string key = useAssociation ? Path.GetExtension(file) : file;

            if (this.images.Images.ContainsKey(key))
            {
                return key;
            }

            using (Icon icon = useAssociation ? WinApi.GetAssociatedIcon(key) : WinApi.GetFileIcon(file))
            {
                this.images.Images.Add(key, icon.ToBitmap());
            }

            return key;
        }


        private void recentListView_SizeChanged(object sender, EventArgs e)
        {
            this.recentListView.TileSize = new Size(Math.Max(5, this.recentListView.ClientRectangle.Width - 2), this.recentListView.TileSize.Height);
        }

        private void visualStudioListView_SizeChanged(object sender, EventArgs e)
        {
            this.visualStudioListView.TileSize = new Size(Math.Max(5, this.visualStudioListView.ClientRectangle.Width - 2), this.visualStudioListView.TileSize.Height);
        }

        private void splitContainer1_SizeChanged(object sender, EventArgs e)
        {
            this.splitContainer1.SplitterDistance = this.splitContainer1.ClientRectangle.Width / 2;

        }

        private void StartForm_Load(object sender, EventArgs e)
        {
            Options.Current.ShowStartWindow.HandleControl(true, this.chkShowAtStart);

            foreach (string file in Options.Current.MRU.Value)
            {
                ListViewItem item = new ListViewItem(Path.GetFileName(file), this.recentListView.Groups["recent"])
                {
                    ImageKey = "assembly",
                    ToolTipText = file,
                    Tag = file
                };
                this.recentListView.Items.Add(item);
            }

            string[] list = this.GetVisualStudioMRU();

            if (list != null)
            {
                foreach (string file in list)
                {
                    ListViewItem item = new ListViewItem(Path.GetFileName(file), this.visualStudioListView.Groups[0])
                    {
                        ImageKey = this.GetIcon(file, false),
                        ToolTipText = file,
                        Tag = file
                    };
                    this.visualStudioListView.Items.Add(item);
                }
            }

            Options.Current.Persistence.AddForm(this);
        }

        private void ItemClicked(ListViewItem item)
        {
            string file = item.Tag as string;

            bool isRecent = item.Group.Name == "recent";
            bool isSolution = item.ListView == this.visualStudioListView;

            item.ListView.SelectedItems.Clear();

            if (isRecent && !isSolution)
            {
                if (!File.Exists(file))
                {
                    if (Dialog.Show(this, "That file no longer exists", "Would you like to remove it from the list?", "File: " + file, TaskDialogStandardButtons.Yes | TaskDialogStandardButtons.No, TaskDialogStandardIcon.Information) == Microsoft.WindowsAPICodePack.Dialogs.TaskDialogResult.Yes)
                    {
                        Options.Current.MRU.Value.Remove(file);
                    }

                    return;
                }
            }

            this.Hide();

            if (!string.IsNullOrEmpty(file))
            {
                if (file == "*browse")
                {
                    if (isSolution)
                    {

                    }
                    else
                    {
                        if (Program.ExceptionExplorerForm.ShowOpenDialog())
                        {
                            return;
                        }
                    }
                }
                else
                {
                    if (isSolution)
                    {
                        if (OpenSolutionForm.Show(this.Owner ?? this, file))
                        {
                            this.Close();
                            return;
                        }
                    }
                    else
                    {
                        Program.ExceptionExplorerForm.OpenFile(file);
                        this.Close();
                        return;
                    }
                }
            }

            this.Show(this.Owner);
        }

        private void ListView_ItemActivate(object sender, EventArgs e)
        {
            ListView list = (ListView)sender;
            ListViewItem item = list.SelectedItems.Cast<ListViewItem>().FirstOrDefault();

            if (item != null)
            {
                this.ItemClicked(item);
            }
        }

        private void ListView_MouseClick(object sender, MouseEventArgs e)
        {
            ListView list = (ListView)sender;
            ListViewItem item = list.GetItemAt(e.X, e.Y);

            if (item != null)
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    this.ItemClicked(item);
                }
                else if (e.Button == System.Windows.Forms.MouseButtons.Right)
                {
                }
            }
        }

        private string[] GetVisualStudioMRU()
        {
            //HKEY_CURRENT_USER\Software\Microsoft\VisualStudio\10.0\ProjectMRUList
            const string keyName = @"Software\Microsoft\VisualStudio\10.0\ProjectMRUList";

            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(keyName, false))
                {
                    IEnumerable<string> names = from s in key.GetValueNames()
                                                where s.StartsWith("File", StringComparison.InvariantCultureIgnoreCase)
                                                orderby s.Substring(4)
                                                select s;

                    List<string> files = new List<string>();

                    foreach (string name in names)
                    {
                        string value = key.GetValue(name, null) as string;
                        if (!string.IsNullOrEmpty(value))
                        {
                            string file = value.Split(new char[] { '|' }, 2).FirstOrDefault();
                            if (!string.IsNullOrEmpty(file))
                            {
                                files.Add(file);
                            }
                        }
                    }

                    return files.ToArray();
                }
            }
            catch (SecurityException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (IOException)
            {
            }

            return null;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void StartForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Options.Current.ShowStartWindow.HandleControl(false, this.chkShowAtStart);
        }
    }
}
