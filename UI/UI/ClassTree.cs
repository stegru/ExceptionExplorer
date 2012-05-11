namespace ExceptionExplorer.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Windows.Forms;
    using ExceptionExplorer.ExceptionAnalysis;
    using System.ComponentModel;
    using System.Threading;
    using System.IO;
    using System.Globalization;
    using ExceptionExplorer.UI.Jobs;
    using ExceptionExplorer.Config;
    using System.Collections;
    using Microsoft.WindowsAPICodePack.Dialogs;

    /// <summary>
    /// Tree for the class/method exploration
    /// </summary>
    public class ClassTree : ExtendedTreeView, IExceptionExplorerControl
    {
        private BindingFlags Binding {
            get
            {
                BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
                return Options.Current.SeparateBaseClassItem.Value ? flags | BindingFlags.FlattenHierarchy : flags;
            }
        }

        public ExceptionExplorerController Controller { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassTree"/> class.
        /// </summary>
        public ClassTree()
            : base()
        {
            Options.Current.SeparateBaseClassItem.Changed += new EventHandler<SettingChangedEventArgs>(SeparateBaseClassItem_Changed);
        }

        /// <summary>
        /// Gets the excpetion finder.
        /// </summary>
        /// <value>The excpetion finder.</value>
        protected ExceptionFinder ExceptionFinder
        {
            get
            {
                return this.Controller.ExceptionFinder;
            }
        }

        public TreeNode LoadAssembly(Assembly asm, CancellationToken cancelToken)
        {
            AssemblyItem asmItem = new AssemblyItem(asm);

            bool first = this.Nodes.Count == 0;

            TreeNode node = this.FindNode((n) =>
            {
                return n.Node.GetInfo() != null && n.Node.GetInfo().GetAnalysisObject<AssemblyItem>() == asmItem;
            }, 2);

            if (node != null)
            {
                return node;
            }

            node = this.NewNode(asmItem, NodeType.Assembly, false);

            Action<CancellationToken> addNodes = (ct) =>
            {
                TreeNode[] nodes = this.CreateSubNodes(node.GetInfo(), ct);
                if (!ct.IsCancellationRequested)
                {
                    Action add = () =>
                    {
                        node.Nodes.AddRange(nodes);
                        node.Expand();
                    };

                    if (node.TreeView != null)
                    {
                        node.TreeView.InvokeIfRequired(add);
                    }
                    else
                    {
                        add();
                    }
                }
            };

            if (JobGroup.Current != Job.Analysis)
            {
                Job.Analysis.NewJob(this, addNodes)
                    .Execute()
                    .Wait();
            }
            else
            {
                addNodes(cancelToken);
            }


            return node;
        }

        /// <summary>
        /// Loads the assembly onto the treeview.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns>true on success.</returns>
        public bool LoadAssembly(string file)
        {
            Assembly asm = null;
            string message = null;

            try
            {
                string fullPath = Path.GetFullPath(file);
                asm = Assembly.ReflectionOnlyLoadFrom(fullPath);

                if (asm == null)
                {
                    return false;
                }

                TreeNode node = GetAssemblyNode(asm);
                if (node != null)
                {
                    this.SelectedNode = node;
                    node.Expand();
                    return true;
                }

            }
            catch (FileNotFoundException)
            {
                message = string.Format(CultureInfo.InvariantCulture, "{0}:\nFile not found.", file);
                return false;
            }
            catch (ArgumentException ex)
            {
                message = string.Format(CultureInfo.InvariantCulture, "{0}:\n{1}", file, ex.Message);
                return false;
            }
            catch (IOException ex)
            {
                message = string.Format(CultureInfo.InvariantCulture, "{0}:\n{1}", file, ex.Message);
                return false;
            }
            finally
            {
                if (message != null)
                {
                    Dialog.Show(this, "There was a problem opening the file.", message, TaskDialogStandardButtons.Ok | TaskDialogStandardButtons.Retry, TaskDialogStandardIcon.Warning);
                }
            }


            Job worker = Job.General.NewJob(this, (cancelToken) =>
            {
                TreeNode root = this.LoadAssembly(asm, cancelToken);

                this.InvokeIfRequired(() =>
                {
                    if (root.TreeView == null)
                    {
                        this.Nodes.Add(root);
                    }
                    root.Expand();
                    this.SelectedNode = root;
                    root.EnsureVisible();

                    if (this.Nodes.Count == 1)
                    {
                        this.SetScrollPos(0, 0);
                    }
                    else
                    {
                        this.SetScrollPos(0, true);
                    }
                });
            });

            worker.Execute();


            return true;
        }

        public TreeNode GetAssemblyNode(Assembly asm)
        {
            TreeNode node = this.FindNode(ni => ni.AnalysisObject is AssemblyItem && ((AssemblyItem)ni.AnalysisObject).FullName == asm.FullName, 1);
            return node;
        }

        /// <summary>
        /// Called when a nodeInfo that points to a Method that needs to be analysed.
        /// </summary>
        /// <param name="method">The method.</param>
        public void CompleteMethodContainer(MethodContainer methodContainer, CancellationToken cancelToken)
        {
            if ((methodContainer != null) && !methodContainer.Complete)
            {
                Method method = methodContainer as Method;
                if (method != null)
                {
                    this.ExceptionFinder.ReadMethod(method.MethodBase, cancelToken);
                    return;
                }
                else
                {
                    Class cls = methodContainer as Class;
                    if (cls != null)
                    {
                        this.ExceptionFinder.ReadClass(cls, cancelToken, true);
                        return;
                    }
                    else
                    {
                        Property prop = methodContainer as Property;
                        if (prop != null)
                        {
                            prop.ReadProperty(this.ExceptionFinder, cancelToken);
                        }
                    }
                }
            }
        }

        private class ReloadNode
        {
            public string Filename { get; private set; }
            public bool Expanded { get; private set; }


            public static ReloadNode Create(TreeNode node)
            {
                AssemblyItem asm = node.GetInfo().GetAnalysisObject<AssemblyItem>();

                if (asm == null)
                {
                    return null;
                }

                return new ReloadNode()
                {
                    Filename = asm.Assembly.Location,
                    Expanded = node.IsExpanded
                };
            }
        }

        public void Reload()
        {
            string path = null;
            if (this.SelectedNode != null)
            {
                path = this.SelectedNode.FullPath;
            }

            this.Controller.History.Clear();

            List<ReloadNode> assemblies = new List<ReloadNode>();

            foreach (TreeNode node in this.Nodes)
            {
                ReloadNode rn = ReloadNode.Create(node);
                if (rn != null)
                {
                    assemblies.Add(rn);
                }
            }

            this.Nodes.Clear();
            this.Controller.ExceptionTree.Nodes.Clear();
            this.Controller.CallStackList.Items.Clear();

            Job worker = Job.General.NewJob(this, (cancelToken) =>
            {
                // kill any running analysis
                Job.Analysis.CancelAll(true);
                this.ExceptionFinder.ResetAllMethods();

                foreach (ReloadNode rn in assemblies)
                {
                    Assembly asm = null;
                    try
                    {
                        asm = Assembly.ReflectionOnlyLoadFrom(rn.Filename);
                    }
                    catch (IOException)
                    {
                        continue;
                    }

                    if (asm != null)
                    {
                        TreeNode root = this.LoadAssembly(asm, cancelToken);

                        this.InvokeIfRequired(() =>
                        {
                            this.Nodes.Add(root);

                            if (rn.Expanded)
                            {
                                root.Expand();
                            }
                        });
                    }
                }

                this.InvokeIfRequired(() =>
                {
                    if (path != null)
                    {
                        //this.SelectPath(path);
                    }
                    else if (this.Nodes.Count > 0)
                    {
                        this.Nodes[0].EnsureVisible();
                    }
                });
            });

            worker.Execute();

        }

        public void ReorderNodes()
        {
            try
            {
                this.BeginUpdate();
                string path = (this.SelectedNode != null) ? this.SelectedNode.FullPath : "";
                this.Sort();
                this.SelectPath(path);
            }
            finally
            {
                this.EndUpdate();
            }
        }

        void SeparateBaseClassItem_Changed(object sender, SettingChangedEventArgs e)
        {
            bool newValue = (bool)e.Value;

            foreach (TreeNode node in this.Nodes)
            {
                this.ReloadClassNodes(node);
            }
           

        }

        public void ReloadClassNodes(TreeNode node)
        {
            NodeInfo info = node.GetInfo();

            if ((info != null) && info.Type.HasFlag(NodeType.Class))
            {
                node.Collapse(true);
                node.Nodes.Clear();
                node.Nodes.Add(this.CreateLazyDummyNode());
                return;
            }

            foreach (TreeNode n in node.Nodes)
            {
                this.ReloadClassNodes(n);
            }

        }

        public void UpdateNodes()
        {
            try
            {
                this.BeginUpdate();
                foreach (TreeNode node in this.Nodes)
                {
                    this.UpdateNode(node);
                }
            }
            finally
            {
                this.EndUpdate();
            }
        }

        private void UpdateNode(TreeNode node)
        {
            NodeInfo info = node.GetInfo();
            if (info != null)
            {
                node.GetInfo().UpdateNode();
            }

            foreach (TreeNode child in node.Nodes)
            {
                this.UpdateNode(child);
            }
        }

        public TreeNode EnsureAssemblyLoaded(Assembly asm, CancellationToken cancelToken)
        {
            TreeNode node = this.FindNode((ni) => (ni.AnalysisObject is AssemblyItem) && ((AssemblyItem)ni.AnalysisObject).FullName == asm.FullName);
            if (node == null)
            {
                node = this.LoadAssembly(asm, cancelToken);
            }

            return node;
        }

        public void ShowCallStack(CallStack callStack, TreeNode startAt, CancellationToken cancelToken)
        {
            if (callStack.Count == 0)
            {
                return;
            }

            // start from the bottom of the stack
            CallStack stack = new CallStack(callStack.Reverse());

            TreeNode lastExistingNode = null;
            TreeNode newNodesRoot = null;
            NodeInfo nodeInfo = startAt.GetInfo();
            TreeNode node = nodeInfo.Node;
            TreeNode lastNode = node;

            bool first = nodeInfo.GetAnalysisObject<Method>() == stack.Peek();

            while (stack.Count >= 0)
            {
                if ((node.Nodes.Count == 0) || (node.Nodes[0].Tag == null))
                {
                    // hasn't got the child nodeInfo yet.

                    nodeInfo = node.GetInfo();

                    if (newNodesRoot == null)
                    {
                        lastExistingNode = node;
                        node = newNodesRoot = new TreeNode();
                    }
                    else
                    {
                        node.Nodes.Clear();
                    }

                    TreeNode[] childNodes = this.CreateSubNodes(nodeInfo, cancelToken);
                    node.Nodes.AddRange(childNodes);
                }

                lastNode = node;

                if (stack.Count == 0)
                {
                    break;
                }

                Method method = stack.Pop();
                if (!first)
                {
                    node = this.FindNode(ni => ni.GetAnalysisObject<Method>() == method, node, 1);
                }
                first = false;
            }

            this.InvokeIfRequired(() =>
            {
                if ((lastExistingNode.Nodes.Count == 0) || (lastExistingNode.Nodes[0].Tag == null))
                {
                    lastExistingNode.Nodes.Clear();
                }

                lastExistingNode.Nodes.AddRange(newNodesRoot.Nodes.Cast<TreeNode>().ToArray());
                lastNode.EnsureVisible();
                this.SelectedNode = lastNode;
            });


        }

        public void SelectPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            CancellationToken cancelToken = new CancellationToken();
            // "ExceptionExplorer, Version=0.2.4557.27328, Culture=neutral, PublicKeyToken=null\\ExceptionExplorer\\CommandLine\\BaseCommandLine.Parse()"

            string[] parts = path.Split(new string[] { this.PathSeparator }, StringSplitOptions.None);

            TreeNode node = null;
            TreeNode lastNode = null;
            bool first = true;

            foreach (string part in parts)
            {
                if (first)
                {
                    first = false;
                    node = this.FindNode(ni => ni.Node.Text == part, node, 1);

                    if (node == null)
                    {
                        Assembly asm = Assembly.Load(part);
                        node = this.LoadAssembly(asm, cancelToken);

                        if (node == null)
                        {
                            return;
                        }

                        this.Nodes.Add(node);
                    }
                }
                else
                {
                    if ((node.Nodes.Count == 0) || (node.Nodes[0].Tag == null))
                    {
                        TreeNode[] childNodes = this.CreateSubNodes(node.GetInfo(), cancelToken);
                        node.Nodes.AddRange(childNodes);
                    }

                    node = node.FindNode(ni => ni.Node.Text == part, 1);
                    if (node == null)
                    {
                        return;
                    }
                }

                if (node != null)
                {
                    lastNode = node;
                }
            }

            if (lastNode != null)
            {
                this.SelectedNode = lastNode;
                this.SelectedNode.EnsureVisible();
            }
        }

        /// <summary>
        /// Adds the sub nodeInfo, asyncronously.
        /// </summary>
        /// <param name="nodeInfo">The nodeInfo info.</param>
        /// <param name="callback">The callback.</param>
        /// <returns>false to cancel the expand</returns>
        protected override bool AddSubNodes(NodeInfo nodeInfo, Action callback)
        {
            Job worker = Job.Analysis.NewJob(this, (cancelToken) =>
            {
                TreeNode[] nodes = this.CreateSubNodes(nodeInfo, cancelToken);
                this.InvokeIfRequired(() =>
                {
                    try
                    {
                        this.BeginUpdate();

                        nodeInfo.Node.Nodes.AddRange(nodes);

                        if (Options.Current.InheritedMemberFullname.Value)
                        {
                            foreach (TreeNode node in nodes)
                            {
                                node.GetInfo().UpdateNode(true);
                            }
                        }

                        nodeInfo.Node.Expand();
                        if (callback != null)
                        {
                            callback();
                        }
                    }
                    finally
                    {
                        this.EndUpdate();
                    }
                });
            });

            worker.Execute(true);
            return false;
        }

        private bool ShouldAnalyse(MethodContainer memberContainer, Type referencedBy)
        {
            if (this.Controller.Settings.SeparateBaseClassItem.Value)
            {
                if ((memberContainer.MemberInfo.DeclaringType != null) && (memberContainer.MemberInfo.DeclaringType != referencedBy))
                {
                    return false;
                }
            }

            return memberContainer.ShouldAnalyse(referencedBy);
        }

        /// <summary>Creates the sub nodeInfo.</summary>
        /// <param name="nodeInfo">The nodeInfo info.</param>
        public TreeNode[] CreateSubNodes(NodeInfo nodeInfo, CancellationToken cancelToken)
        {
            TreeNode node = new TreeNode();

            if (nodeInfo.Type.HasFlag(NodeType.Class))
            {
                Class cls = nodeInfo.GetAnalysisObject<Class>();
                Type type = cls.ClassType;

                if (this.Controller.Settings.SeparateBaseClassItem.Value)
                {
                    // add a node for the base class
                    Type baseType = type.BaseType;
                    if (baseType != null)
                    {
                        Class baseClass = Class.GetClass(baseType);
                        if (this.ShouldAnalyse(baseClass, type))
                        {
                            TreeNode baseClassNode = this.NewNode(Class.GetClass(baseType), NodeType.BaseClass);
                            node.Nodes.Add(baseClassNode);
                        }
                    }
                }

                // add the nested classes
                foreach (Type t in type.GetNestedTypes(Binding))
                {
                    Class c = Class.GetClass(t);
                    
                    if (this.ShouldAnalyse(c, type))
                    {
                        TreeNode classNode = this.NewNode(c, NodeType.Class);
                        classNode.Nodes.AddRange(this.CreateSubNodes(classNode.GetInfo(), cancelToken));
                        node.Nodes.Add(classNode);
                    }
                }

                HashSet<string> done = new HashSet<string>();

                // handle properties first
                PropertyInfo[] properties = type.GetProperties(Binding);

                foreach (PropertyInfo propInfo in properties)
                {
                    if (!(propInfo.CanRead || propInfo.CanWrite))
                    {
                        continue;
                    }

                    Property prop = new Property(propInfo);

                    if (this.ShouldAnalyse(prop, type))
                    {

                        string name = (propInfo.DeclaringType == type) ? propInfo.Name : String.Format("{0}.{1}", propInfo.DeclaringType, propInfo.Name);

                        TreeNode propNode = this.NewNode(prop, NodeType.Property);
                        propNode.Nodes.Clear();

                        if (propInfo.CanRead)
                        {
                            propNode.Nodes.Add(this.NewNode("get", prop.Get, NodeType.Method));
                        }

                        if (propInfo.CanWrite)
                        {
                            propNode.Nodes.Add(this.NewNode("set", prop.Set, NodeType.Method));
                        }

                        node.Nodes.Add(propNode);
                    }
                }

                // do the constructors
                ConstructorInfo[] ctors = type.GetConstructors(Binding);
                foreach (ConstructorInfo ctor in ctors)
                {
                    Method m = Method.GetMethod(ctor);
                    if (this.ShouldAnalyse(m, type))
                    {
                        node.Nodes.Add(this.NewNode(m, NodeType.Ctor));
                    }
                }

                // the rest of the methods
                MethodInfo[] methods = type.GetMethods(Binding);
                foreach (MethodInfo method in methods)
                {
                    if (method.IsConstructor || method.IsProperty() || method.IsEvent())
                    {
                        continue;
                    }
                    Method m = Method.GetMethod(method);
                    if (this.ShouldAnalyse(m, type))
                    {
                        node.Nodes.Add(this.NewNode(m, NodeType.Method));
                    }
                }
            }
            else if (nodeInfo.Type.HasFlag(NodeType.Method))
            {
                Method method = nodeInfo.AnalysisObject as Method;
                if (method == null)
                {
                    return null;
                }

                if (!method.Complete)
                {
                    this.CompleteMethodContainer(method, cancelToken);
                }

                // add the interesting methods that this method calls

                // do the property get/setters first
                Dictionary<string, NodeInfo> properties = this.GetProperties(node, method);
                // events
                Dictionary<string, NodeInfo> events = this.GetEvents(node, method);

                Type declaringType = method.MethodBase.DeclaringType;
                IEnumerable<Method> calledMethods = from m in method.CalledMethods
                                              where m.ShouldAnalyse(declaringType)
                                              select m;

                // constructors
                foreach (Method m in calledMethods)
                {
                    if (!m.MethodBase.IsConstructor)
                    {
                        continue;
                    }

                    if ((m.UnhandledExceptions.Count > 0) && m.ShouldAnalyse(method.MethodBase.DeclaringType))
                    {
                        node.Nodes.Add(this.NewNode(m, NodeType.Ctor));
                    }
                }

                foreach (Method m in calledMethods)
                {
                    MethodBase mb = m.MethodBase;

                    if (m.MethodBase.IsConstructor)
                    {
                        continue;
                    }
                    else if (mb.IsProperty())
                    {
                        if (properties.ContainsKey(mb.Name.Substring(4)))
                        {
                            continue;
                        }
                    }

                    if (m.UnhandledExceptions.Count > 0)
                    {
                        node.Nodes.Add(this.NewNode(m, NodeType.Method));
                    }
                }
            }
            else if (nodeInfo.Type.HasFlag(NodeType.Property))
            {
                // already added the get/set nodes
            }
            else if (nodeInfo.Type.HasFlag(NodeType.Assembly))
            {
                AssemblyItem ass = nodeInfo.GetAnalysisObject<AssemblyItem>();

                // add the types for each namespace
                foreach (Namespace ns in ass.Namespaces.OrderBy(n => n.Name))
                {
                    TreeNode namespaceNode = this.NewNode(ns.Name, ns, NodeType.Namespace, false);
                    node.Nodes.Add(namespaceNode);

                    foreach (Type type in ns.GetTypes().OrderBy(t => t.Name))
                    {
                        if (!type.IsNested)
                        {
                            Class cls = Class.GetClass(type);
                            if (cls.ShouldAnalyse(type))
                            {
                                namespaceNode.Nodes.Add(this.NewNode(cls, NodeType.Class));
                            }
                        }
                    }
                }
            }
            else if (nodeInfo.Type.HasFlag(NodeType.Namespace))
            {
                // nodes already added on assembly load
            }

            return node.Nodes.Cast<TreeNode>().ToArray();
        }

        private Dictionary<string, NodeInfo> GetEvents(TreeNode node, Method method)
        {
            Dictionary<string, NodeInfo> events = new Dictionary<string, NodeInfo>();
            foreach (Method m in method.CalledMethods)
            {
            }
            return events;
        }

        private Dictionary<string, NodeInfo> GetProperties(TreeNode node, Method method)
        {
            Dictionary<string, NodeInfo> properties = new Dictionary<string, NodeInfo>();
            foreach (Method m in method.CalledMethods)
            {
                MethodBase mb = m.MethodBase;
                if (mb.Name.Contains("get_UserInt"))
                {
                }

                // see if it's a getter/setter
                if (mb.IsSpecialName && mb.IsHideBySig)
                {
                    bool getter = mb.Name.StartsWith("get_");
                    if (getter || mb.Name.StartsWith("set_"))
                    {
                        NodeInfo ni = null;
                        string name = mb.Name.Substring(4);
                        PropertyInfo pi = null;
                        bool alreadyFound = properties.ContainsKey(name);
                        if (!alreadyFound)
                        {
                            try
                            {
                                pi = mb.DeclaringType.GetProperty(name, Binding);
                            }
                            catch (AmbiguousMatchException)
                            {
                                // be a bit more specific with the search
                                Type returnType = typeof(void);
                                if (mb is MethodInfo)
                                {
                                    returnType = ((MethodInfo)mb).ReturnType;
                                }

                                IEnumerable<Type> paras = from p in mb.GetParameters() select p.ParameterType;
                                if (!getter)
                                {
                                    returnType = paras.Last();
                                    paras = paras.Take(paras.Count() - 1);
                                }

                                pi = mb.DeclaringType.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, returnType, paras.ToArray(), null);
                            }

                            if (pi == null)
                            {
                                continue;
                            }

                            ni = this.NewNode(new Property(pi), NodeType.Property).GetInfo();
                            ni.Node.Nodes.Clear();
                            properties.Add(name, ni);
                        }
                        else
                        {
                            ni = properties[name];
                        }

                        if (m.UnhandledExceptions.Count > 0)
                        {
                            ni.Node.Nodes.Add(this.NewNode(getter ? "get" : "set", m, NodeType.Method));
                            if (!alreadyFound)
                            {
                                node.Nodes.Add(ni.Node);
                            }
                        }
                    }
                }
            }
            return properties;
        }


    }

}