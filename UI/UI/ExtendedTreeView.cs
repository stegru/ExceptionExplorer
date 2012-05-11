namespace ExceptionExplorer.UI
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Linq;
    using System.Collections;
    using System.ComponentModel;
    using System.Collections.Generic;
    using ExceptionExplorer.ExceptionAnalysis;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    /// <summary>
    /// Extensions related to the TreeView control
    /// </summary>
    public static class ExtendedTreeViewExtensions
    {
        public static IEnumerable<TreeNode> GetDescendants(this TreeNode node)
        {
            IEnumerable<TreeNode> nodes = node.Nodes.Cast<TreeNode>();

            foreach (TreeNode n in node.Nodes)
            {
                nodes = nodes.Concat(n.GetDescendants());
            }

            return nodes;
        }

        /// <summary>Finds a nodeInfo.</summary>
        /// <param name="nodeInfo">The nodeInfo.</param>
        /// <param name="match">The match.</param>
        /// <returns>The matching nodeInfo, or null</returns>
        public static TreeNode FindNode(this TreeNode node, Predicate<NodeInfo> match, int maxDepth)
        {
            // check if it matches
            NodeInfo nodeInfo = node.GetInfo();
            if ((nodeInfo != null) && match.Invoke(nodeInfo))
            {
                return node;
            }

            if (maxDepth < 0)
            {
                return null;
            }

            // do the same for the child nodeInfo
            foreach (TreeNode child in node.Nodes)
            {
                TreeNode found = child.FindNode(match, maxDepth - 1);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        public static TreeNode FindNode(this TreeNode node, Predicate<NodeInfo> match)
        {
            return node.FindNode(match, int.MaxValue);
        }

        /// <summary>Gets the NodeInfo from the nodeInfo's Tag property</summary>
        /// <param name="nodeInfo">The nodeInfo.</param>
        /// <returns>The get info</returns>
        public static NodeInfo GetInfo(this TreeNode node)
        {
            return (node == null) ? null : node.Tag as NodeInfo;
        }
    }

    /// <summary>
    /// An enhanced TreeView control.
    /// Contains general stuff related to ExceptionExplorer, but not necessarily other applications
    /// </summary>
    public class ExtendedTreeView : TreeView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedTreeView"/> class.
        /// </summary>
        public ExtendedTreeView()
        {
            // prevents flicker
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.EnableNotifyMessage, true);
            this.HideSelection = false;
            this.ShowLines = false;
        }

        /// <summary>Overrides <see cref="M:System.Windows.Forms.Control.WndProc(System.Windows.Forms.Message@)"/>.</summary>
        /// <param name="m">The Windows <see cref="T:System.Windows.Forms.Message"/> to process.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            if (m.Msg != 0x14)
            {
                base.WndProc(ref m);
            }
        }

        /// <summary>
        /// Creates the handle.
        /// </summary>
        protected override void CreateHandle()
        {
            base.CreateHandle();
            WinApi.SetWindowTheme(this.Handle, "explorer", null);
        }

        /// <summary>
        /// Expands the nodeInfo.
        /// </summary>
        /// <param name="nodeInfo">The nodeInfo.</param>
        /// <returns>false to cancel</returns>
        public bool ExpandNode(TreeNode node)
        {
            // get rid of the dummy 
            TreeNode[] removeNodes = node.Nodes.Cast<TreeNode>().ToArray();
            NodeInfo nodeInfo = node.GetInfo();

            return this.AddSubNodes(nodeInfo, () =>
            {
                // remove the original nodeInfo when finished
                foreach (TreeNode n in removeNodes)
                {
                    this.Nodes.Remove(n);
                }
            });
        }

        /// <summary>Finds a nodeInfo.</summary>
        /// <param name="match">The match.</param>
        /// <returns>The matching nodeInfo, or null</returns>
        public TreeNode FindNode(Predicate<NodeInfo> match, int maxDepth)
        {
            foreach (TreeNode node in this.Nodes)
            {
                TreeNode found = node.FindNode(match, maxDepth - 1);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        /// <summary>Finds the nodeInfo.</summary>
        /// <param name="match">The match.</param>
        /// <param name="startFrom">The start from.</param>
        /// <returns>The matching nodeInfo, or null</returns>
        public TreeNode FindNode(Predicate<NodeInfo> match, TreeNode startFrom, int maxDepth)
        {
            if (startFrom != null)
            {
                return startFrom.FindNode(match, maxDepth);
            }

            return this.FindNode(match, maxDepth);
        }

        public TreeNode FindNode(Predicate<NodeInfo> match)
        {
            return this.FindNode(match, null, int.MaxValue);
        }
        public TreeNode FindNode(Predicate<NodeInfo> match, TreeNode startFrom)
        {
            return this.FindNode(match, startFrom, int.MaxValue);
        }

        /// <summary>
        /// Sets the scroll posistion.
        /// </summary>
        /// <param name="position">The position.</param>
        public void SetScrollPos(Point position)
        {
            this.SetScrollPos(position.X, position.Y);
        }

        /// <summary>
        /// Sets the scroll posistion.
        /// </summary>
        /// <param name="x">The x position.</param>
        /// <param name="y">The y position.</param>
        public void SetScrollPos(int x, int y)
        {
            WinApi.SetScrollPos(this.Handle, WinApi.SB_HORZ, x, false);
            WinApi.SetScrollPos(this.Handle, WinApi.SB_VERT, y, true);
        }

        /// <summary>Sets the scroll position</summary>
        /// <param name="pos">The position.</param>
        /// <param name="horizontal">if set to <c>true</c> set the horizontal position.</param>
        public void SetScrollPos(int pos, bool horizontal)
        {
            WinApi.GetScrollPos(this.Handle, horizontal ? WinApi.SB_HORZ : WinApi.SB_VERT);
        }

        /// <summary>
        /// Adds the sub nodeInfo.
        /// </summary>
        /// <param name="nodeInfo">The nodeInfo info.</param>
        /// <param name="callback">The callback.</param>
        /// <returns>false to cancel</returns>
        protected virtual bool AddSubNodes(NodeInfo nodeInfo, Action callback)
        {
            return true;
        }

        /// <summary>Creates a new nodeInfo that is expandable, and will contain nodeInfo that are added when expanded</summary>
        /// <param name="data">The data.</param>
        /// <param name="type">The type.</param>
        /// <returns>The new nodeInfo</returns>
        internal virtual TreeNode NewNode(IAnalysisObject data, NodeType type)
        {
            return this.NewNode(null, data, type);
        }

        /// <summary>Creates a new nodeInfo that is expandable, and will contain nodeInfo that are added when expanded</summary>
        /// <param name="data">The data.</param>
        /// <param name="type">The type.</param>
        /// <param name="lazy">if set to <c>true</c> (default), add the child.</param>
        /// <returns>The new nodeInfo</returns>
        internal virtual TreeNode NewNode(IAnalysisObject data, NodeType type, bool lazy)
        {
            return this.NewNode(null, data, type, lazy);
        }

        /// <summary>Creates a new nodeInfo that is expandable, and will contain nodeInfo that are added when expanded</summary>
        /// <param name="text">The text.</param>
        /// <param name="data">The data.</param>
        /// <param name="type">The type.</param>
        /// <returns>The new nodeInfo</returns>
        internal virtual TreeNode NewNode(string text, IAnalysisObject data, NodeType type)
        {
            return this.NewNode(text, data, type, true);
        }

        /// <summary>Creates a new nodeInfo that is expandable, and will contain nodeInfo that are added when expanded</summary>
        /// <param name="text">The text.</param>
        /// <param name="data">The data.</param>
        /// <param name="type">The type.</param>
        /// <param name="lazy">if set to <c>true</c> (default), add the child.</param>
        /// <returns>The new nodeInfo</returns>
        internal virtual TreeNode NewNode(string text, IAnalysisObject data, NodeType type, bool lazy)
        {
            return this.NewNode(new NodeInfo(text, data, type, lazy));
        }

        /// <summary>
        /// Creates a new nodeInfo that is expandable, and will contain nodeInfo that are added when expanded
        /// </summary>
        /// <param name="nodeInfo">The nodeInfo info.</param>
        /// <returns>The new nodeInfo</returns>
        internal virtual TreeNode NewNode(NodeInfo nodeInfo)
        {
            if (nodeInfo.Lazy && (this is ClassTree))
            {
                MethodContainer mc = nodeInfo.GetAnalysisObject<MethodContainer>();

                if ((mc != null) && mc.Complete)
                {
                    nodeInfo.Lazy = mc.UnhandledExceptions.Count > 0;
                }
            }

            if (nodeInfo.Lazy)
            {
                this.InvokeIfRequired(() =>
                {
                    // add a dummy nodeInfo to show the '+' next to this one
                    nodeInfo.Node.Nodes.Add(this.CreateLazyDummyNode());
                });
            }

            return nodeInfo.Node;
        }

        protected TreeNode CreateLazyDummyNode()
        {
            return new TreeNode()
            {
                Tag = null,
                Text = "Loading...",
                ImageKey = NodeInfo.ImageKeyFromType(NodeType.None),
                StateImageKey = NodeInfo.ImageKeyFromType(NodeType.None)
            };
        }

        /// <summary>
        /// Raises the <see cref="E:System.WindowPositions.Forms.TreeView.BeforeExpand"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.WindowPositions.Forms.TreeViewCancelEventArgs"/> that contains the event data.</param>
        protected override void OnBeforeExpand(TreeViewCancelEventArgs e)
        {
            if (e.Node.Tag is NodeInfo)
            {
                if ((e.Node.Nodes.Count == 0) || (e.Node.Nodes[0].Tag == null))
                {
                    e.Cancel = this.ExpandNode(e.Node);
                }
            }

            base.OnBeforeExpand(e);
        }

        /// <summary>
        /// Notifies the control of WindowPositions messages.
        /// </summary>
        /// <param name="m">A <see cref="T:System.WindowPositions.Forms.Message"/> that represents the WindowPositions message.</param>
        protected override void OnNotifyMessage(Message m)
        {
            // prevents flicker
            if (m.Msg != WinApi.WM_ERASEBKGND)
            {
                base.OnNotifyMessage(m);
            }
        }

        /// <summary>
        /// Disables any redrawing of the tree view.
        /// </summary>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/>
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/>
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/>
        ///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/>
        ///   </PermissionSet>
        public new void BeginUpdate()
        {
            base.BeginUpdate();
        }

        /// <summary>
        /// Enables the redrawing of the tree view.
        /// </summary>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/>
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/>
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/>
        ///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/>
        ///   </PermissionSet>
        public new void EndUpdate()
        {
            base.EndUpdate();
        }
    }
}