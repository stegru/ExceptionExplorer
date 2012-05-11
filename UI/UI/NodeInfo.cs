namespace ExceptionExplorer.UI
{
    using System;
    using System.Reflection;
    using System.Windows.Forms;
    using ExceptionExplorer.ExceptionAnalysis;
    using System.Drawing;
    using ExceptionExplorer.Config;

    /// <summary>
    /// Types of a nodeInfo
    /// </summary>
    [Flags]
    public enum NodeType
    {

        /// <summary>Not set</summary>
        None = 0,

        /// <summary>An assembly</summary>
        Assembly = 1 << 0,

        /// <summary>A namespace</summary>
        Namespace = 1 << 1,

        /// <summary>A class</summary>
        Class = 1 << 2,

        /// <summary>A method</summary>
        Method = 1 << 3,

        /// <summary>A property</summary>
        Property = 1 << 4,

        /// <summary>A constructor</summary>
        Ctor = Method | 1 << 5,

        /// <summary>An exception</summary>
        Exception = Class | 1 << 6,

        /// <summary>An event.</summary>
        Event = 1 << 7,

        /// <summary>A base class node.</summary>
        BaseClass = Class | 1 << 8
    }

    /// <summary>
    /// Contains information about a nodeInfo.
    /// This is stored in the TreeNode's Tag property
    /// </summary>
    public class NodeInfo
    {
        private bool ignore;

        private bool autoText;

        private MethodContainer methodContainer;

        /// <summary>Gets or sets the data.</summary>
        /// <value>The data.</value>
        public IAnalysisObject AnalysisObject { get; set; }

        /// <summary>Gets or sets the extra data.</summary>
        /// <value>The extra data.</value>
        public object ExtraData { get; set; }

        /// <summary>Gets or sets the nodeInfo.</summary>
        /// <value>The nodeInfo.</value>
        public TreeNode Node { get; set; }

        /// <summary>Gets or sets the type.</summary>
        /// <value>The type.</value>
        public NodeType Type { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="NodeInfo"/> is lazy loading.
        /// </summary>
        /// <value>
        ///   <c>true</c> if lazy; otherwise, <c>false</c>.
        /// </value>
        public bool Lazy { get; set; }

        /// <summary>Gets or sets a value indicating whether this <see cref="NodeInfo"/> is ignored.</summary>
        /// <value><c>true</c> if ignored; otherwise, <c>false</c>.</value>
        public bool Ignore
        {
            get
            {
                return this.ignore;
            }

            set
            {
                if (this.ignore != value)
                {
                    this.ignore = value;
                    this.UpdateNode();
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeInfo"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="data">The data.</param>
        /// <param name="type">The type.</param>
        public NodeInfo(string text, IAnalysisObject data, NodeType type)
            : this(new TreeNode(text), data, type)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeInfo"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="data">The data.</param>
        /// <param name="type">The type.</param>
        public NodeInfo(string text, IAnalysisObject data, NodeType type, bool lazy)
            : this(new TreeNode(text), data, type)
        {
            this.Lazy = lazy;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeInfo"/> class.
        /// </summary>
        /// <param name="nodeInfo">The nodeInfo.</param>
        /// <param name="data">The data.</param>
        /// <param name="type">The type.</param>
        public NodeInfo(TreeNode node, IAnalysisObject data, NodeType type)
        {
            this.autoText = (node.Text == null) || (node.Text.Length == 0);
            this.methodContainer = data as MethodContainer;



            if (!(this.methodContainer != null))
            {
                if (data is MethodBase)
                {
                    Method m = Method.GetMethod((MethodBase)data);
                    data = m;
                }
                else if (data is PropertyInfo)
                {
                    data = new Property((PropertyInfo)data);
                }
                else if (data is Type)
                {
                    data = Class.GetClass((Type)data);
                }
                else if (data is Assembly)
                {
                }
                else if (data is Namespace)
                {
                }
                else
                {
                }
            }


            // make Exception classes look like exceptions
            if (type.HasFlag(NodeType.Class))
            {
                Type t = ((Class)data).ClassType;
                if (t.IsException())
                {
                    type = NodeType.Exception;
                }
            }

            this.Node = node;
            this.AnalysisObject = data;
            this.Type = type;
            this.Node.Tag = this;
            this.ignore = false;

            if (this.methodContainer != null)
            {
                this.methodContainer.Changed += new EventHandler<ChangedEventArgs>(MethodContainer_Changed);
            }

            this.UpdateNode();
        }

        void MethodContainer_Changed(object sender, EventArgs e)
        {
            this.UpdateNode();
        }

        static Random rnd = new Random();

        public void UpdateNode(bool recursive)
        {
            this.UpdateNode();
            if (recursive)
            {
                foreach (TreeNode node in this.Node.Nodes)
                {
                    NodeInfo info = node.GetInfo();
                    if (info != null)
                    {
                        info.UpdateNode(true);
                    }
                }
            }
        }

        /// <summary>Updates the tree node.</summary>
        public void UpdateNode()
        {
            Action doIt = () =>
            {
                try
                {
                    this.methodContainer = this.GetAnalysisObject<MethodContainer>();

                    if (Node.TreeView != null)
                    {
                        //((ExtendedTreeView)Node.TreeView).BeginUpdate();
                    }

                    TreeNode node = this.Node;

                    string key = ImageKeyFromType(this.Type);
                    if (node.ImageKey != key)
                    {
                        node.ImageKey = node.SelectedImageKey = key;
                    }

                    //node.ForeColor = Color.FromArgb(rnd.Next());
                    // this.ignore ? SystemColors.GrayText : SystemColors.WindowText;

                    if (this.autoText)
                    {
                        IAnalysisObject parent;
                        if (node.Parent != null)
                        {
                            parent = node.Parent.GetInfo().AnalysisObject;
                        }
                        else
                        {
                            parent = this.AnalysisObject.Parent;
                        }

                        string s = this.AnalysisObject.GetDisplayName(parent);

                        if (node.GetInfo().Type.HasFlag(NodeType.BaseClass))
                        {
                            s = string.Concat("base {", s, "}");
                        }

                        if (node.Text != s)
                        {
                            node.Text = s;
                        }
                    }


                    if (this.methodContainer != null)
                    {
                        if (this.methodContainer.Complete && this.Lazy && (this.methodContainer.UnhandledExceptions.Count == 0))
                        {
                            this.Lazy = false;
                            if ((node.Nodes.Count == 1) && (node.Nodes[0].Tag == null))
                            {
                                node.Nodes.Clear();
                            }
                        }
                    }
                }
                finally
                {
                    if (Node.TreeView != null)
                    {
                        //((ExtendedTreeView)Node.TreeView).EndUpdate();
                    }
                }
            };

            if (this.Node.TreeView == null)
            {
                doIt();
            }
            else
            {
                this.Node.TreeView.InvokeIfRequired(doIt);
            }

        }
        /// <summary>Images the key from object.</summary>
        /// <param name="data">The data.</param>
        /// <returns>A suitable image key</returns>
        public static string ImageKeyFromObject(object data)
        {
            NodeType type = NodeType.None;

            if (data is Method)
            {
                data = ((Method)data).MethodBase;
            }

            if (data is MethodBase)
            {
                MethodBase method = (MethodBase)data;
                if (method.IsConstructor)
                {
                    type = NodeType.Ctor;
                }
                else
                {
                    type = NodeType.Method;
                }
            }
            else if (data is PropertyInfo)
            {
                type = NodeType.Property;
            }
            else if (data is AssemblyItem)
            {
                type = NodeType.Assembly;
            }
            else if (data is Type)
            {
                Type t = (Type)data;
                if (t.IsException())
                {
                    type = NodeType.Exception;
                }
                else
                {
                    type = NodeType.Class;
                }
            }

            return ImageKeyFromType(type);
        }

        /// <summary>
        /// Images the type of the key from.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// Last word triggers "of the" reordering
        /// </returns>
        public static string ImageKeyFromType(NodeType type)
        {
            if (type == NodeType.BaseClass)
            {
                type = NodeType.Class;
            }

            return type.ToString().ToLower();
        }

        public static NodeType TypeFromObject(IAnalysisObject analysisObject)
        {
            if (analysisObject == null)
            {
                return NodeType.None;
            }

            if (analysisObject is AssemblyItem)
            {
                return NodeType.Assembly;
            }
            else if (analysisObject is Namespace)
            {
                return NodeType.Namespace;
            }
            else if (analysisObject is Class)
            {
                if (((Class)analysisObject).ClassType.IsException())
                {
                    return NodeType.Exception;
                }

                return NodeType.Class;
            }
            else if (analysisObject is Method)
            {
                if (((Method)analysisObject).MethodBase.IsConstructor)
                {
                    return NodeType.Ctor;
                }
                else
                {
                    return NodeType.Method;
                }
            }
            else if (analysisObject is Property)
            {
                return NodeType.Property;
            }
            else
            {
            }
            return NodeType.None;
        }

        /////// <summary>Generates the name of the nodeInfo.</summary>
        /////// <param name="data">The data.</param>
        /////// <param name="type">The type.</param>
        /////// <returns></returns>
        ////protected virtual string GenerateName()
        ////{
        ////    if (this.Type.HasFlag(NodeType.Class))
        ////    {
        ////        return this.GetData<Class>().ClassType.Name;
        ////    }

        ////    MethodContainer mc = this.GetData<MethodContainer>();
        ////    if (mc != null)
        ////    {
        ////        return mc.Name;
        ////    }

        ////    return this.Data.ToString();
        ////}

        /// <summary>
        /// Gets the data associated with this nodeInfo.
        /// </summary>
        /// <typeparam name="T">The type of the data.</typeparam>
        /// <returns>The Data property</returns>
        public T GetAnalysisObject<T>() where T : class
        {
            return this.AnalysisObject as T;
        }

        /// <summary>
        /// Gets the extra data associated with this nodeInfo.
        /// </summary>
        /// <typeparam name="T">The type of the data</typeparam>
        /// <returns>The ExtraData property</returns>
        public T GetExtra<T>() where T : class
        {
            return this.ExtraData as T;
        }
    }
}