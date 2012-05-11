namespace ExceptionExplorer.UI.ContextActions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;
    using System.Collections;
    using ExceptionExplorer.ExceptionAnalysis;
    using System.Reflection;

    /// <summary>
    /// 
    /// </summary>
    public class ContextActionController
    {
        private List<ContextAction> actions = new List<ContextAction>();
        private HashSet<Control> controls = new HashSet<Control>();

        private Control currentControl;
        private NodeInfo selectedNode;

        public void AddRange(IEnumerable<ContextAction> actions)
        {
            actions.ToList().ForEach(a => this.Add(a));
        }

        public void Add(ContextAction action)
        {
            this.AddControls(action.Controls);
            this.actions.Add(action);
        }

        public void AddControls(IEnumerable<Control> controls)
        {
            if (controls != null)
            {
                controls.ToList().ForEach(c => this.AddControl(c));
            }
        }

        public void AddControl(Control control)
        {
            if (this.controls.Add(control))
            {
                control.GotFocus += new EventHandler(Control_GotFocus);
                if (control is ExtendedTreeView)
                {
                    control.MouseUp += new MouseEventHandler(TreeView_MouseUp);
                }
                else if (control is ExtendedListView)
                {
                    control.MouseUp += new MouseEventHandler(ListView_MouseUp);
                }
            }
        }

        void ListView_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
            {
                return;
            }

            ExtendedListView lv = (ExtendedListView)sender;

            ListViewItem selected = lv.GetItemAt(e.X, e.Y);

            if (selected == null)
            {
                selected = (lv.SelectedItems.Count > 0) ? lv.SelectedItems[0] : null;
            }

            if (selected != null)
            {
                NodeInfo nodeInfo = CallStackList.CreateNodeInfo(selected);

                this.ShowContextMenu(lv, nodeInfo, e.X, e.Y);
            }
        }

        private void TreeView_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
            {
                return;
            }

            ExtendedTreeView tv = (ExtendedTreeView)sender;
            
            TreeNode selectedNode = tv.GetNodeAt(e.X, e.Y);

            if (selectedNode == null)
            {
                selectedNode = tv.SelectedNode;
            }

            NodeInfo nodeInfo = selectedNode.GetInfo();

            this.ShowContextMenu(tv, nodeInfo, e.X, e.Y);
        }

        private void Control_GotFocus(object sender, EventArgs e)
        {
            NodeInfo selectedNode = null;

            if (sender is ExtendedTreeView)
            {
                selectedNode = ((ExtendedTreeView)sender).SelectedNode.GetInfo();
            }
            else if (sender is ExtendedListView)
            {
                //selectedNode = ((ExtendedListView)sender)
            }

            if (selectedNode != null)
            {
                this.SetContext((Control)sender, selectedNode);
            }
        }

        public void SetContext(Control control, NodeInfo selection)
        {
            this.currentControl = control;
            this.selectedNode = selection;
        }

        private ToolStripMenuItem CreateMenuItem(ContextAction action, NodeInfo node)
        {
            string text;

            if (action.FormattedText)
            {
                string name, fullname;
                fullname = node.AnalysisObject.FullName;
                if (node.AnalysisObject.IsMethodContainer)
                {
                    name = ((MethodContainer)node.AnalysisObject).Name;
                }
                else
                {
                    name = node.Node.Text;
                }

                text = string.Format(action.Text, node.Node.Text, name, fullname);
            }
            else
            {
                text = action.Text;
            }

            ToolStripMenuItem item = new ToolStripMenuItem(text);
            action.MenuItem = item;
            item.Click += new EventHandler(MenuItem_Click);
            return item;
        }

        public ContextMenuStrip CreateMenu(NodeInfo node)
        {
            ContextMenuStrip menu = new ContextMenuStrip();

            foreach (ContextAction action in this.actions)
            {
                if ((action.Controls.Count > 0) && !action.Controls.Contains(this.currentControl))
                {
                    continue;
                }

                if (!action.NoSelection)
                {
                    if (node == null)
                    {
                        continue;
                    }

                    if ((action.NodeType != 0) && !action.NodeType.HasFlag(node.Type))
                    {
                        continue;
                    }

                    if ((action.Predicate != null) && !action.Predicate(node))
                    {
                        continue;
                    }
                }

                ToolStripMenuItem item = this.CreateMenuItem(action, node);
                menu.Items.Add(item);
            }

            return menu;
        }

        public void ShowContextMenu(Control control, NodeInfo selection, int x, int y)
        {
            this.SetContext(control, selection);
            ContextMenuStrip menu = this.CreateMenu(selection);
            menu.Show(control, x, y);
        }

        void MenuItem_Click(object sender, EventArgs e)
        {
            IEnumerable<ContextAction> matches = from a in this.actions
                                              where a.MenuItem == sender
                                              select a;

            foreach (ContextAction action in matches)
            {
                if (action.Callback != null)
                {
                    action.Callback(this.selectedNode);
                }
            }
        }



    }
}
