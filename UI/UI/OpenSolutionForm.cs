namespace ExceptionExplorer.UI
{
    using System;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;
    using ExceptionExplorer.VS2010;
    using System.IO;
    using System.Runtime.InteropServices;
    using ExceptionExplorer.Config;
    using Microsoft.WindowsAPICodePack.Dialogs;
    using System.Collections.Generic;

    public partial class OpenSolutionForm : Form
    {
        public static bool Show(IWin32Window owner, string solutionFile)
        {
            if (!File.Exists(solutionFile))
            {
                Dialog.Show(owner, "This solution does not exist.", string.Format("Could not find {0}.", solutionFile), TaskDialogStandardButtons.Ok, TaskDialogStandardIcon.Warning);
                return false;
            }

            Solution solution = new Solution(solutionFile);
            if (!solution.Success)
            {
                Dialog.Show(owner, "This solution could not be opened.", string.Format("There was a problem with {0}.", solutionFile), TaskDialogStandardButtons.Ok, TaskDialogStandardIcon.Warning);
                return false;
            }

            OpenSolutionForm frm = new OpenSolutionForm()
            {
                Solution = solution
            };

            return frm.ShowDialog(owner) == DialogResult.OK;
        }

        private List<TreeNode> allFileNodes = new List<TreeNode>();
        private bool hasOpened = false;

        protected Solution Solution { get; set; }

        public OpenSolutionForm()
        {
            InitializeComponent();
        }

        private string GetIcon(string file, bool useAssociation)
        {

            string key = useAssociation ? Path.GetExtension(file) : file;

            if (this.imageList.Images.ContainsKey(key))
            {
                return key;
            }

            using (Icon icon = useAssociation ? WinApi.GetAssociatedIcon(key) : WinApi.GetFileIcon(file))
            {
                this.imageList.Images.Add(key, icon.ToBitmap());
            }

            return key;
        }


        private TreeNode NewNode(string text, string file, string type)
        {
            string imageKey = "";
            bool isAssembly = false;

            switch (type)
            {
                case "solution":
                case "project":
                    imageKey = this.GetIcon(file, true);
                    break;
                case "assembly":
                    imageKey = this.GetIcon(file, false);
                    isAssembly = true;
                    break;
                case "platform":
                case "config":
                    imageKey = "folder";
                    break;
                case "none":
                    imageKey = "none";
                    break;
            }

            if (text == file)
            {
                text = Path.GetFileName(file);
            }

            return new TreeNode(text)
            {
                Tag = isAssembly ? file : null,
                ToolTipText = file,
                ImageKey = imageKey,
                StateImageKey = imageKey,
                SelectedImageKey = imageKey
            };
        }

        private void OpenSolutionForm_Load(object sender, EventArgs e)
        {
            this.btnOpen.Enabled = false;

            Solution sln = this.Solution;
            this.allFileNodes.Clear();

            Options.Current.Persistence.AddControl(this.chkAutoClose);

            TreeNode rootNode = this.NewNode(sln.Name, sln.SlnFile, "solution");

            foreach (Project p in sln.GetProjects())
            {
                TreeNode projectNode = this.NewNode(p.Name, p.ProjFile, "project");

                foreach (string config in p.GetConfigurations())
                {
                    TreeNode configNode = this.NewNode(config, null, "config");

                    foreach (string platform in p.GetPlatforms(config))
                    {
                        TreeNode platformNode = this.NewNode(platform, null, "platform");

                        string output = p.GetOutputFile(config, platform);
                        if (File.Exists(output))
                        {
                            TreeNode fileNode = this.NewNode(output, output, "assembly");
                            this.allFileNodes.Add(fileNode);
                            platformNode.Nodes.Add(fileNode);
                            configNode.Nodes.Add(platformNode);
                        }
                    }

                    if (configNode.Nodes.Count == 1)
                    {
                        // only 1 platform for this configuration - merge them
                        TreeNodeCollection nodes = configNode.Nodes[0].Nodes;
                        configNode = this.NewNode(configNode.Text + "/" + configNode.Nodes[0].Text, null, "config");
                        configNode.Nodes.AddRange(nodes.Cast<TreeNode>().ToArray());
                    }

                    if (configNode.Nodes.Count > 0)
                    {
                        projectNode.Nodes.Add(configNode);
                    }
                }

                if (projectNode.Nodes.Count == 0)
                {
                    projectNode.Nodes.Add(this.NewNode("No assemblies found", null, "none"));
                }

                projectNode.Expand();
                rootNode.Nodes.Add(projectNode);

            }

            if (rootNode.Nodes.Count == 0)
            {
                rootNode.Nodes.Add(this.NewNode("No suitable projects found", null, "none"));
            }

            rootNode.Expand();
            this.solutionTreeView.Nodes.Add(rootNode);

            if (this.allFileNodes.Count == 1)
            {
                this.solutionTreeView.SelectedNode = this.allFileNodes[0];
            }
            else if (this.allFileNodes.Count > 0)
            {
                TreeNode newestFile = (from node in this.allFileNodes
                                       orderby File.GetLastWriteTime(node.Tag.ToString()) descending
                                       select node).FirstOrDefault();
                this.solutionTreeView.SelectedNode = newestFile;
            }

        }

        private void SelectionMade()
        {
            if (this.solutionTreeView.SelectedNode == null)
            {
                return;
            }

            string file = this.solutionTreeView.SelectedNode.Tag as string;
            if (string.IsNullOrEmpty(file))
            {
                return;
            }

            Program.ExceptionExplorerForm.OpenFile(file);

            this.hasOpened = true;

            if (this.chkAutoClose.Checked)
            {
                this.Close();
                return;
            }

            this.btnClose.Text = "Close";
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            this.SelectionMade();
        }

        private void solutionTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            this.btnOpen.Enabled = this.solutionTreeView.SelectedNode.Tag != null;
        }

        private void OpenSolutionForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.DialogResult = this.hasOpened ? System.Windows.Forms.DialogResult.OK : System.Windows.Forms.DialogResult.Cancel;
        }

        private void solutionTreeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            this.solutionTreeView.SelectedNode = e.Node;
            this.SelectionMade();
        }
    }
}