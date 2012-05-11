namespace ExceptionExplorer.UI
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;
    using ExceptionExplorer.ExceptionAnalysis;
    using System.ComponentModel;
    using ExceptionExplorer.Properties;

    /// <summary>
    /// Tree for the exceptions
    /// </summary>
    public class ExceptionTree : ExtendedTreeView, IExceptionExplorerControl
    {
        /// <summary>Gets or sets the controller.</summary>
        /// <value>The controller.</value>
        public ExceptionExplorerController Controller { get; set; }

        /// <summary>
        /// Gets or sets the call stack list control.
        /// </summary>
        /// <value>The call stack list.</value>
        public CallStackList CallStackList { get; set; }

        public event EventHandler<MethodContainerEventArgs> ObjectChanged;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionTree"/> class.
        /// </summary>
        public ExceptionTree()
            : base()
        {
        }

        /// <summary>
        /// Gets or sets the method.
        /// </summary>
        /// <value>
        /// The method.
        /// </value>
        public Method Method { get; protected set; }

        /// <summary>Gets the method container.</summary>
        public MethodContainer MethodContainer { get; private set; }

        /// <summary>Gets the class.</summary>
        public Class Class { get; private set; }

        /// <summary>
        /// Gets or sets the exception that is to be selected the next time the tree is updated.
        /// </summary>
        /// <value>
        /// The preselect.
        /// </value>
        public Type Preselect { get; set; }

        public Type SelectedException { get; private set; }

        protected override void OnBeforeSelect(TreeViewCancelEventArgs e)
        {
            Type previous = this.SelectedException;

            NodeInfo node = e.Node.GetInfo();
            if (node != null)
            {
                Class cls = node.GetAnalysisObject<Class>();
                this.SelectedException = cls == null ? null : cls.ClassType;
            }
            else
            {
                this.SelectedException = null;
            }

            base.OnBeforeSelect(e);

            if (e.Cancel)
            {
                this.SelectedException = previous;
            }
        }

        /// <summary>Sets the object.</summary>
        /// <param name="methodContainer">The method container.</param>
        /// <param name="rootNode">The root node.</param>
        public void SetObject(MethodContainer methodContainer, TreeNode rootNode)
        {
            this.InvokeIfRequired(() =>
            {
                if (this.ObjectChanged != null)
                {
                    this.ObjectChanged(this, new MethodContainerEventArgs(methodContainer));
                }

                if (rootNode == null)
                {
                    try
                    {
                        this.BeginUpdate();
                        this.Nodes.Clear();
                    }
                    finally
                    {
                        this.EndUpdate();
                    }
                }
            });

            this.MethodContainer = methodContainer;

            this.Method = this.MethodContainer as Method;
            if (this.Method != null)
            {
                this.LoadMethod(rootNode);
                return;
            }

            this.Class = this.MethodContainer as Class;
            if (this.Class != null)
            {
                this.LoadClass(rootNode);
                return;
            }

            this.LoadMethod(rootNode);

        }

        private void LoadClass(TreeNode rootNode)
        {
            //MethodAnalysis analysis = this.Method.Analysis;

            this.LoadMethod(rootNode);


        }

        public void Wait(bool waiting)
        {
            this.InvokeIfRequired(() =>
            {
                if (waiting)
                {
                    if (this.Nodes.Count == 0)
                    {
                        this.Nodes.Add(
                                new TreeNode()
                                {
                                    Tag = null,
                                    Text = "Loading...",
                                    ImageKey = NodeInfo.ImageKeyFromType(NodeType.None),
                                    StateImageKey = NodeInfo.ImageKeyFromType(NodeType.None)
                                }
                            );
                    }
                }
                else
                {
                    this.Cursor = Cursors.Default;
                    if ((this.Nodes.Count == 1) && (this.Nodes[0].Tag == null))
                    {
                        this.Nodes.Clear();
                    }
                }

                this.SetWaiting(waiting);
            });
        }

        internal ThrownExceptionDictionary ExceptionDictionary { get; private set; }

        private void LoadMethod(TreeNode rootNode)
        {
            MethodAnalysis analysis = this.MethodContainer.Analysis;

            Type referencedBy = this.MethodContainer.MemberInfo.DeclaringType;

            if ((referencedBy == null) && (this.MethodContainer is Class))
            {
                referencedBy = ((Class)this.MethodContainer).ClassType;
            }

            IEnumerable<ThrownException> exceptions = from te in analysis.AllExceptions
                                                      where te.Method.ShouldAnalyse(referencedBy)
                                                      select te;

            this.ExceptionDictionary = new ThrownExceptionDictionary(exceptions);
            //analysis.ExceptionDictionary;

            List<Stack<Type>> stacks = new List<Stack<Type>>();

            HashSet<Type> whatsThrown = new HashSet<Type>();
            IEnumerable<ThrownExceptionCollection> lists = this.ExceptionDictionary.GetLists();

            // make a stack of each exception type
            foreach (ThrownExceptionCollection list in lists)
            {
                Type type = list.First().Exception;

                whatsThrown.Add(type);

                // keep getting the base class until Exception is hit
                Stack<Type> stack = new Stack<Type>();

                stack.Push(type);
                Type t = type;
                while ((t = t.BaseType) != null)
                {
                    if (t == typeof(object))
                    {
                        break;
                    }

                    stack.Push(t);
                }

                stacks.Add(stack);
            }

            TreeNode root = new TreeNode();

            // add them to the tree
            foreach (Stack<Type> stack in stacks)
            {
                TreeNode node = null, parent = null;

                while (stack.Count > 0)
                {
                    Type t = stack.Pop();

                    node = ((node == null) ? root : node).FindNode(ni =>
                        (ni.Type == NodeType.Exception) && (ni.GetAnalysisObject<Class>().ClassType == t)
                        );

                    if (node == null)
                    {
                        string name;
                        bool thrown = whatsThrown.Contains(t);

                        name = t.IsNested ? t.FullName : t.Name;

                        NodeInfo nodeInfo = new NodeInfo(name, Class.GetClass(t), NodeType.Exception);
                        node = this.NewNode(nodeInfo);

                        if (!thrown)
                        {
                            node.ForeColor = SystemColors.GrayText;
                        }

                        if (parent == null)
                        {
                            root.Nodes.Add(node);
                        }
                        else
                        {
                            parent.Nodes.Add(node);
                        }
                    }
                    else
                    {
                    }

                    parent = node;
                }
            }

            this.InvokeIfRequired(() =>
            {
                try
                {
                    this.BeginUpdate();

                    if (rootNode == null)
                    {
                        this.Nodes.AddRange(root.Nodes.Cast<TreeNode>().ToArray());
                    }
                    else
                    {
                        rootNode.Nodes.AddRange(root.Nodes.Cast<TreeNode>().ToArray());
                        if (rootNode.Nodes.Count > 0)
                        {
                            this.Nodes.Add(rootNode);
                        }
                    }

                    if (this.Nodes.Count == 0)
                    {
                        TreeNode none = this.NewNode("No unhandled exceptions", null, NodeType.None, false);
                        this.Nodes.Add(none);
                    }

                    this.ExpandAll();
                    this.Sort();
                    if (this.Preselect != null)
                    {
                        this.SelectedNode = this.FindNode(ni => ni.GetAnalysisObject<Class>().ClassType == this.Preselect);
                        this.Preselect = null;
                    }

                    this.SetScrollPos(0, 0);


                }
                finally
                {
                    this.EndUpdate();
                }

            });

        }
    }
}