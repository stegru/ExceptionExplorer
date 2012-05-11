// -----------------------------------------------------------------------
// <copyright file="ExceptionExplorerHub.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace ExceptionExplorer.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using ExceptionExplorer.ExceptionAnalysis;
    using System.Windows.Forms;
    using System.ComponentModel;
    using System.Reflection;
    using System.Threading;
    using Decompiler;
    using ExceptionExplorer.Config;
    using System.IO;
    using ExceptionExplorer.UI.Jobs;
    using ExceptionExplorer.UI.ContextActions;

    public interface IExceptionExplorerControl
    {
        ExceptionExplorerController Controller { get; set; }
    }

    public class ExceptionExplorerController
    {
        public ClassTree ClassTree { get; protected set; }
        public ExceptionTree ExceptionTree { get; protected set; }
        public CallStackList CallStackList { get; protected set; }
        public SourceViewer SourceViewer { get; protected set; }

        public History History { get; protected set; }

        public ExceptionFinder ExceptionFinder { get; protected set; }

        public Func<IEnumerable<ThrownException>, ExceptionTree> CreateExceptionTree;

        public Options Settings { get; protected set; }

        public ContextActionController NodeActions { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionExplorerController"/> class.
        /// </summary>
        /// <param name="classTree">The class tree.</param>
        /// <param name="exceptionTree">The exception tree.</param>
        /// <param name="callStackList">The exception list.</param>
        internal ExceptionExplorerController(ClassTree classTree, ExceptionTree exceptionTree, CallStackList callStackList, SourceViewer sourceViewer)
        {
            this.History = new History();
            this.History.Changed += new EventHandler(HistoryChanged);

            this.CreateExceptionTree = (IEnumerable<ThrownException> list) => { return new ExceptionTree(); };

            this.ClassTree = classTree;
            this.ExceptionTree = exceptionTree;
            this.CallStackList = callStackList;
            this.SourceViewer = sourceViewer;

            this.ClassTree.Controller = this.ExceptionTree.Controller = this.CallStackList.Controller = this;

            this.Settings = Options.Current;

            this.ExceptionFinder = ExceptionFinder.Instance;
            this.ExceptionFinder.DocumentedExceptionsCallback = Decompiler.Extensions.XmlDocumentation.GetDocumentedExceptions;
            this.ExceptionFinder.SynchronizeInvoke = this.ClassTree;

            this.ClassTree.PathSeparator = "\a";
            this.ExceptionTree.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(ExceptionTree_BeforeSelect);
            this.ClassTree.AfterSelect += new TreeViewEventHandler(ClassTree_AfterSelect);

            this.CallStackList.MethodSelected += new EventHandler<UI.CallStackList.MethodEventArgs>(callStackList_MethodSelected);

            this.ClassTree.AllowDrop = true;
            this.ClassTree.DragEnter += new DragEventHandler(ClassTree_DragEnter);
            this.ClassTree.DragDrop += new DragEventHandler(ClassTree_DragDrop);

            this.ExceptionFinder.Settings.Changed += new EventHandler<SettingChangedEventArgs>(SettingsChangeReload);
            this.Settings.InheritedMemberFullname.Changed += new EventHandler<SettingChangedEventArgs>(SettingsChangeUpdate);
            this.Settings.MemberFullname.Changed += new EventHandler<SettingChangedEventArgs>(SettingsChangeUpdate);
            this.Settings.SeparateBaseClassItem.Changed += new EventHandler<SettingChangedEventArgs>(SettingsChangeUpdate);

            this.Settings.BulkChange += new EventHandler<EventArgs>(Settings_BulkChange);

            this.Settings.Source.Changed += new EventHandler<SettingChangedEventArgs>(Source_Changed);
            this.Settings.ShowSource.Changed += new EventHandler<SettingChangedEventArgs>(Source_Changed);

            this.UpdateSourceViewerSettings();

            // context menus
            ContextActionCallbacks callbacks = new ContextActionCallbacks(this);

            this.NodeActions = new ContextActionController();
            ContextAction[] a = 
            {
                new ContextAction("Show Call") {
                    Controls = { this.CallStackList },
                    Callback = callbacks.ShowSource,
                    Predicate = (nodeInfo) =>
                    {
                        return !this.Settings.Source.AutoDecompile.Value || !this.Settings.ShowSource.Value;
                    }
                },

                new ContextAction("Load declaring assembly", ~NodeType.Assembly) {
                    Controls = { this.CallStackList, this.ClassTree },
                    Callback = callbacks.LoadAssembly,
                    Predicate = (nodeInfo) =>
                    {
                        return this.ClassTree.GetAssemblyNode(nodeInfo.AnalysisObject.GetAssembly()) == null;
                    }
                },

                new ContextAction("Show Source", ~(NodeType.Assembly | NodeType.Namespace)) {
                    Controls = { this.ClassTree },
                    Callback = callbacks.ShowSource,
                    Predicate = (nodeInfo) =>
                    {
                        return !this.Settings.Source.AutoDecompile.Value || !this.Settings.ShowSource.Value;
                    }
                },

                new ContextAction("Unload", NodeType.Assembly) {
                    Callback = callbacks.UnloadAssembly,
                    Controls = { this.ClassTree },
                },

                new ContextAction("Copy name") {
                    Callback = callbacks.CopyFullName
                }
            };

            this.NodeActions.AddRange(a);
            this.NodeActions.AddControl(this.ClassTree);
            this.NodeActions.AddControl(this.ExceptionTree);
            this.NodeActions.AddControl(this.CallStackList);
        }

        private class ContextActionCallbacks
        {
            private ExceptionExplorerController controller;

            public ContextActionCallbacks(ExceptionExplorerController controller)
            {
                this.controller = controller;
            }

            public void ShowSource(NodeInfo info)
            {
                ItemInfo itemInfo = info.ExtraData as ItemInfo;

                if (!this.controller.Settings.ShowSource.Value)
                {
                    this.controller.Settings.ShowSource.Value = true;
                }

                if (itemInfo != null)
                {
                    this.controller.callStackList_MethodSelected(this.controller, this.controller.CallStackList.LastMethodSelected);
                }
                else
                {
                    this.controller.ShowMethodSource(info.AnalysisObject as MethodContainer);
                }
            }

            public void LoadAssembly(NodeInfo info)
            {
                Assembly asm = info.AnalysisObject.GetAssembly();
                if (asm == null)
                {
                    return;
                }

                this.controller.ClassTree.LoadAssembly(asm.Location);
            }

            public void UnloadAssembly(NodeInfo info)
            {
                Assembly asm = info.AnalysisObject.GetAssembly();
                if (asm != null)
                {
                    TreeNode node = this.controller.ClassTree.GetAssemblyNode(asm);
                    if (node != null)
                    {
                        if (node.Parent == null)
                        {
                            node.TreeView.Nodes.Remove(node);
                        }
                        else
                        {
                            node.Parent.Nodes.Remove(node);
                        }
                    }
                }
            }

            public void CopyFullName(NodeInfo info)
            {
                string fullName = info.AnalysisObject.FullName;
                Clipboard.SetText(fullName);
            }
        }

        private void ReloadSource()
        {
            Job.General.NewJob(this.SourceViewer, (cancelToken) =>
            {
                this.SourceViewer.Reload(cancelToken);
            }).Execute();
        }

        private void UpdateSourceViewerSettings()
        {
            this.SourceViewer.ShowUsing = this.Settings.Source.ShowUsing.Value;
            this.SourceViewer.ShowXmlDoc = this.Settings.Source.ShowXmlDoc.Value;
            this.SourceViewer.DecompileLanguageFeatures = this.Settings.Source.DecompileLanguageFeatures.Value;
        }

        void Source_Changed(object sender, SettingChangedEventArgs e)
        {
            this.UpdateSourceViewerSettings();
            if (this.Settings.InBulkChange)
            {
                this.needsSourceReload = true;
            }
            else if (this.Settings.ShowSource.Value)
            {
                this.ReloadSource();
            }
        }

        void Settings_BulkChange(object sender, EventArgs e)
        {
            if (!this.Settings.InBulkChange)
            {
                if (this.needsReload)
                {
                    this.SettingsChangeReload(sender, null);
                }
                else if (this.needsUpdate)
                {
                    this.SettingsChangeUpdate(sender, null);
                }

                if (this.needsSourceReload)
                {
                    this.ReloadSource();
                }

                this.needsUpdate = this.needsReload = this.needsSourceReload = false;
            }
        }

        private bool needsUpdate;
        private bool needsReload;
        private bool needsSourceReload;

        void SettingsChangeUpdate(object sender, SettingChangedEventArgs e)
        {
            if (this.Settings.InBulkChange)
            {
                this.needsUpdate = true;
            }
            else
            {
                this.ClassTree.UpdateNodes();
            }
        }

        void SettingsChangeReload(object sender, SettingChangedEventArgs e)
        {
            if (this.Settings.InBulkChange)
            {
                this.needsReload = true;
            }
            else
            {
                this.ClassTree.Reload();
            }
        }

        void HistoryChanged(object sender, EventArgs e)
        {
            if (this.History.CurrentItem != null)
            {
                this.ClassTree.SelectPath(this.History.CurrentItem.Path);
            }
        }

        void callStackList_MethodSelected(object sender, CallStackList.MethodEventArgs e)
        {
            if (this.Settings.Source.AutoDecompile.Value || (sender == this))
            {
                Job job = Job.SourceView.NewJob(this.SourceViewer, (cancelToken) =>
                {
                    // show the call to the selected method
                    if (e.Method != null)
                    {
                        this.ShowMethodCall(e.CallingMethod, e.Method);
                    }
                    else
                    {
                        this.ShowExceptionThrow(e.CallingMethod, e.Exception);
                    }
                    //this.SourceViewer.SetSource(e.Method.MethodBase);
                }).Execute();
            }
        }

        void ShowMethodCall(Method caller, Method called)
        {
            Job.SourceView.NewJob(this.SourceViewer, (cancelToken) =>
            {
                if ((caller == null) || (caller == called))
                {
                    this.SourceViewer.SetSource(called.MethodBase, cancelToken);
                }
                else
                {
                    //this.SourceViewer.SetSource(caller.MethodBase, called.MethodBase, caller.Calls[called]);
                    List<SourceViewer.HighlightItem> items = new List<SourceViewer.HighlightItem>();

                    foreach (int offset in caller.Calls[called])
                    {
                        items.Add(new SourceViewer.HighlightItem(called.MethodBase)
                        {
                            Display = Decompiler.SourceViewer.HighlightDisplay.Highlight,
                            Offset = offset,
                            Tooltip = called.ToString()
                        });
                    }

                    this.SourceViewer.SetSource(caller.MethodBase, items, cancelToken);
                }
            }).Execute();
        }

        void ShowExceptionThrow(Method caller, ThrownException te)
        {
            if (caller == null)
            {
                caller = te.Method;
            }

            Job.SourceView.NewJob(this.SourceViewer, (cancelToken) =>
            {
                if (Environment.TickCount == 123)
                {
                    throw new TimeZoneNotFoundException();
                }
                List<SourceViewer.HighlightItem> items = new List<SourceViewer.HighlightItem>();

                foreach (ThrownException ex in caller.UnhandledExceptions)
                {
                    if (ex.Exception == te.Exception)
                    {
                        SourceViewer.HighlightItem item = new SourceViewer.HighlightItem(te.Exception)
                        {
                            Display = Decompiler.SourceViewer.HighlightDisplay.Highlight,
                            Offset = ex.Offset
                        };

                        if (ex.IsXmlDoc)
                        {
                            item.HighlightType |= Decompiler.SourceViewer.HighlightType.DocumentedException;
                        }

                        items.Add(item);
                    }
                }

                this.SourceViewer.SetSource(caller.MethodBase, items, cancelToken);
            }).Execute();
        }

        void ShowMethodSource(MethodContainer methodContainer)
        {
            Job.SourceView.NewJob(this.SourceViewer, (cancelToken) =>
            {
                List<Method> methods = new List<Method>();

                List<KeyValuePair<MethodBase, IEnumerable<SourceViewer.HighlightItem>>> sources = new List<KeyValuePair<MethodBase, IEnumerable<SourceViewer.HighlightItem>>>();


                if (methodContainer is Method)
                {
                    methods.Add((Method)methodContainer);
                    this.SourceViewer.SetSource(((Method)methodContainer).MethodBase, new SourceViewer.HighlightItem[] { }, cancelToken);
                }
                else
                {
                    Property prop = methodContainer as Property;
                    if (prop != null)
                    {
                        this.SourceViewer.SetSource(prop.PropertyInfo, new SourceViewer.HighlightItem[] { }, cancelToken);
                    }
                }
            }).Execute();
        }


        /// <summary>
        /// Handles the DragDrop event of the ClassTree control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.WindowPositions.Forms.DragEventArgs"/> instance containing the event data.</param>
        void ClassTree_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                this.LoadAssembly(file);
            }
        }

        /// <summary>
        /// Handles the DragEnter event of the ClassTree control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.WindowPositions.Forms.DragEventArgs"/> instance containing the event data.</param>
        void ClassTree_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        public TreeNode FindNode(Predicate<NodeInfo> match)
        {
            return this.FindNode(match, null);
        }
        public TreeNode FindNode(Predicate<NodeInfo> match, TreeNode startFrom)
        {
            return this.FindNode(match, startFrom, int.MaxValue);
        }
        public TreeNode FindNode(Predicate<NodeInfo> match, int maxDepth)
        {
            return this.FindNode(match, null, maxDepth);
        }
        public TreeNode FindNode(Predicate<NodeInfo> match, TreeNode startFrom, int maxDepth)
        {
            return this.ClassTree.FindNode(match, startFrom, maxDepth);
        }

        public TreeNode SelectedClassNode
        {
            get { return this.ClassTree.SelectedNode; }
        }

        public void ShowCallStack(CallStack callStack, ThrownException thrownException)
        {
            TreeNode node = this.SelectedClassNode;
            Job worker = Job.Analysis.NewJob(this.ClassTree, (cancelToken) =>
            {
                this.ExceptionTree.Preselect = thrownException.Exception;
                this.ClassTree.ShowCallStack(callStack, node, cancelToken);
            });
            worker.Execute();
        }


        private NodeInfo lastSelected = null;
        private void ClassTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            NodeInfo nodeInfo = e.Node.GetInfo();
            if (nodeInfo == null)
            {
                return;
            }
            this.lastSelected = nodeInfo;

            this.History.Add(e.Node);

            MethodContainer mc = nodeInfo.AnalysisObject as MethodContainer;

            this.CallStackList.InvokeIfRequired(() =>
            {
                this.CallStackList.Items.Clear();
            });


            Job worker = Job.Analysis.NewJob(this.ClassTree, (cancelToken) =>
            {
                bool waiting = false;

                try
                {
                    this.ExceptionTree.InvokeIfRequired(() =>
                    {
                        try
                        {
                            this.ExceptionTree.BeginUpdate();
                            this.ExceptionTree.Nodes.Clear();

                            if ((mc != null) && !mc.Complete)
                            {
                                waiting = true;
                                this.ExceptionTree.Wait(true);
                            }
                        }
                        finally
                        {
                            this.ExceptionTree.EndUpdate();
                        }
                    });



                    bool isProperty = nodeInfo.Type.HasFlag(NodeType.Property);

                    if (isProperty)
                    {
                        PropertyInfo pi = nodeInfo.AnalysisObject as PropertyInfo;
                        if (pi != null)
                        {
                            mc = new Property(pi);
                            ((Property)mc).ReadProperty(this.ExceptionFinder, cancelToken);
                        }
                    }

                    if (mc != null)
                    {
                        if (!mc.Complete)
                        {
                            this.ClassTree.CompleteMethodContainer(mc, cancelToken);
                        }

                        TreeNode exceptionRoot = null;
                        if (isProperty)
                        {
                            exceptionRoot = this.ExceptionTree.NewNode(new NodeInfo(nodeInfo.Node.Text, nodeInfo.AnalysisObject, nodeInfo.Type));
                        }
                        else
                        {
                            exceptionRoot = null;
                        }

                        this.ExceptionTree.Wait(false);
                        waiting = false;
                        if (this.lastSelected == nodeInfo)
                        {
                            this.ExceptionTree.SetObject(mc, exceptionRoot);
                            if (this.Settings.Source.AutoDecompile.Value)
                            {
                                this.ShowMethodSource(mc);
                            }
                        }
                    }
                }
                finally
                {
                    // always remove this if it's there
                    if (waiting)
                    {
                        this.ExceptionTree.Wait(false);
                    }
                }
            });


            worker.Execute();
        }

        /// <summary>
        /// Returns all the exceptions that a tree nodeInfo and it's child nodeInfo point to.
        /// </summary>
        /// <param name="nodeInfo">The nodeInfo.</param>
        /// <returns>
        /// The exceptions that a nodeInfo and it's child nodeInfo point to.
        /// </returns>
        private IEnumerable<ThrownException> AddList(TreeNode node)
        {
            Class cls = node.GetInfo().GetAnalysisObject<Class>();

            IEnumerable<ThrownException> l = null;

            if (cls != null)
            {
                Type type = cls.ClassType;
                l = this.ExceptionTree.ExceptionDictionary.GetList(type);
            }
            else
            {
                l = new ThrownException[] { };
            }

            foreach (TreeNode child in node.Nodes)
            {
                l = l.Concat(this.AddList(child));
            }

            node.GetInfo().ExtraData = l;
            return l;
        }

        /// <summary>
        /// Handles the BeforeSelect event of the ExceptionTree control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.WindowPositions.Forms.TreeViewCancelEventArgs"/> instance containing the event data.</param>
        private void ExceptionTree_BeforeSelect(object sender, System.Windows.Forms.TreeViewCancelEventArgs e)
        {
            ExceptionTree tree = (ExceptionTree)sender;
            if ((e.Node == null) || (e.Node.GetInfo() == null))
            {
                return;
            }

            IEnumerable<ThrownException> list = e.Node.GetInfo().GetExtra<IEnumerable<ThrownException>>();
            if (list == null)
            {
                MethodAnalysis analysis = tree.MethodContainer.Analysis;
                list = this.AddList(e.Node).Distinct();
            }


            tree.CallStackList.SetExceptions(tree.MethodContainer, list);
        }

        /// <summary>
        /// Loads the assembly onto the treeview.
        /// </summary>
        /// <param name="file">The file.</param>
        public void LoadAssembly(string file)
        {
            this.LoadAssembly(file, false);
        }

        /// <summary>Loads the assembly onto the treeview.</summary>
        /// <param name="file">The file.</param>
        /// <param name="mru">set to <c>true</c> if loaded from the MRU list.</param>
        public void LoadAssembly(string file, bool mru)
        {
            if (string.IsNullOrEmpty(file))
            {
                return;
            }
            file = Path.GetFullPath(file);
            if (this.ClassTree.LoadAssembly(file))
            {
                this.Settings.MRU.Value.Use(file);
            }
            else if (mru)
            {
                this.Settings.MRU.Value.Remove(file);
            }
        }
    }

    /// <summary>
    /// Event data for a MethodContainer
    /// </summary>
    public class MethodContainerEventArgs : EventArgs
    {
        public MethodContainer MethodContainer { get; private set; }
        public ThrownException Exception { get; private set; }

        public MethodContainerEventArgs() { }

        public MethodContainerEventArgs(MethodContainer methodContainer)
            : base()
        {
            this.MethodContainer = methodContainer;
        }
    }

}
