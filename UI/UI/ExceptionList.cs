namespace ExceptionExplorer.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;
    using ExceptionExplorer.Analysis;
    using System.ComponentModel;
    using System.Drawing;
    using System.Reflection;
    using System.Globalization;

    /// <summary>
    /// Listview for the exception call stacks.
    /// </summary>
    public class ExceptionList : ExtendedListView, IExceptionExplorerControl
    {
        private class MethodItem
        {
            public Method Method { get; set; }
            public Method CalledBy { get; set; }
            public ThrownException Exception { get; set; }
            public MethodItem(Method method, ThrownException exception)
            {
                this.Method = method;
                this.Exception = exception;
            }
        }

        public ExceptionExplorerController Controller { get; set; }

        ////private class ItemInfo
        ////{
        ////    public object Data { get; private set; }
        ////    public List<ListViewItem> Children { get; set; }
        ////    public bool Expanded { get; set; }

        ////    public ItemInfo(object data)
        ////    {
        ////        this.Data = data;
        ////        this.Children = null;
        ////    }
        ////}

        public class MethodEventArgs : EventArgs
        {
            public Method Method { get; private set; }
            public Method CallingMethod { get; private set; }

            public ThrownException Exception { get; private set; }

            public MethodEventArgs(Method method, Method callingMethod, ThrownException exception)
            {
                this.Method = method;
                this.CallingMethod = callingMethod;
                this.Exception = exception;
            }
        }

        public event EventHandler<MethodEventArgs> MethodSelected;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionList"/> class.
        /// </summary>
        public ExceptionList()
            : base()
        {
            this.BackColor = System.Drawing.SystemColors.Window;
            this.FullRowSelect = true;
            this.UseCompatibleStateImageBehavior = false;
            this.View = System.Windows.Forms.View.Details;
            this.InitHeaders();

            this.OwnerDraw = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to group the exceptions by type.
        /// </summary>
        /// <value>
        ///   <c>true</c> if grouping exceptions by their type; otherwise, <c>false</c>.
        /// </value>
        public bool GroupByType { get; set; }

        /// <summary>Gets or sets the exceptions.</summary>
        /// <value>The exceptions.</value>
        protected IEnumerable<ThrownException> Exceptions { get; set; }

        protected MethodContainer MethodContainer { get; set; }

        public event EventHandler<MethodContainerEventArgs> ObjectChanged;

        /// <summary>Sets the exceptions.</summary>
        /// <param name="exceptions">The exceptions.</param>
        public void SetExceptions(MethodContainer methodContainer, IEnumerable<ThrownException> exceptions)
        {
            this.MethodContainer = methodContainer;
            this.Exceptions = exceptions;

            if (this.ObjectChanged != null)
            {
                this.ObjectChanged(this, new MethodContainerEventArgs(this.MethodContainer));
            }

            this.UpdateList();
        }

        /// <summary>
        /// Gets the child items for the given item.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <returns></returns>
        protected List<ListViewItem> GetChildren(ItemInfo info)
        {
            MethodItem me = info.Data as MethodItem;

            if (me.Exception != null)
            {
                return this.GetCallstackItems(info, me.Exception);
            }

            return null;
        }

        private List<ListViewItem> GetCallstackItems(ItemInfo info, ThrownException ex)
        {
            List<ListViewItem> items = new List<ListViewItem>();


            //MethodContainer.CallStack stack = this.MethodContainer.Analysis.GetCallstack(ex.Method);
            CallStack stack = ((MethodItem)info.Data).Method.Analysis.GetCallstack(ex.Method);

            // add the methods in it
            if (stack != null)
            {
                bool first = true;
                ItemInfo previous = null;
                foreach (Method m in stack)
                {
                    if (first)
                    {
                        first = false;
                        continue;
                    }

                    ItemInfo child = MakeItem(null, m);
                    if (previous != null)
                    {
                        ((MethodItem)child.Data).CalledBy = ((MethodItem)previous.Data).Method;
                    }
                    else
                    {
                        ((MethodItem)child.Data).CalledBy = ((MethodItem)info.Data).Method;
                    }

                    child.Item.IndentCount = info.Item.IndentCount + 1;

                    child.Parent = info;
                    child.PreviousChild = previous;

                    previous = child;

                    items.Add(child.Item);
                }

                // add an item for the throw statement
                ItemInfo throwItem = MakeItem(ex);
                throwItem.Item.Text = "throw " + ex.Exception.Name;
                throwItem.Item.IndentCount++;

                throwItem.PreviousChild = previous;
                throwItem.Parent = info;

                items.Add(throwItem.Item);
            }
            return items;
        }

        protected override void ItemExpanded(ItemInfo info, bool expanded)
        {
            if (info.SubItems == null)
            {
                info.SubItems = this.GetChildren(info);
            }

            base.ItemExpanded(info, expanded);
        }

        /// <summary>
        /// Makes a new item.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="method">The method.</param>
        /// <param name="group">The group.</param>
        /// <returns></returns>
        protected ItemInfo MakeItem(ThrownException exception, Method method = null, ListViewGroup group = null)
        {
            bool isMethod = (method != null);
            bool isProperty = isMethod && method.MethodBase.IsProperty();
            bool isThrow = !isMethod && (exception != null);

            if (!isMethod)
            {
                method = exception.Method;
            }

            ListViewItem item;
            PropertyInfo pi = null;
            if (isProperty)
            {
                bool getter = method.MethodBase.Name.StartsWith("get_");
                pi = method.MethodBase.GetMethodProperty();
                item = new ListViewItem(string.Format(CultureInfo.InvariantCulture, "{0} ({1})", pi.Name, getter ? "get" : "set"));
            }
            else
            {
                item = new ListViewItem(method.ToString());
            }


            if (isProperty)
            {
                item.ImageKey = NodeInfo.ImageKeyFromObject(pi);
            }
            else if (isMethod)
            {
                // use the method for the icon
                item.ImageKey = NodeInfo.ImageKeyFromObject(method.MethodBase);
            }
            else
            {
                // use the exception for the icon
                item.ImageKey = NodeInfo.ImageKeyFromObject(exception.Exception);
            }

            ItemInfo info;
            MethodItem me = new MethodItem(method, exception);

            if (exception == null)
            {
                info = new ItemInfo(item, me, NodeType.Method);
                item.SubItems.Add("");
            }
            else
            {
                info = new ItemInfo(item, me, NodeType.Method);
                info.Expandable = !isThrow;
                item.SubItems.Add(exception.Exception.Name);

            }

            item.Tag = info;

            item.SubItems.Add(method.MethodBase.DeclaringType.Name);
            item.SubItems.Add(method.MethodBase.DeclaringType.Module.Name);

            item.IndentCount = 1;

            if (group != null)
            {
                item.Group = group;
            }

            return info;
        }

        /// <summary>Updates the list.</summary>
        protected void UpdateList()
        {
            Worker worker = new Worker(this, (cancelToken) =>
            {
                this.InvokeIfRequired(() =>
                {
                    try
                    {
                        this.BeginUpdate();
                        this.Items.Clear();
                    }
                    finally
                    {
                        this.EndUpdate();
                    }
                });

                List<ListViewItem> items = new List<ListViewItem>();


                if (this.MethodContainer is Class)
                {
                    HashSet<string> got = new HashSet<string>();

                    foreach (Method m in this.MethodContainer.CalledMethods)
                    {
                        foreach (ThrownException ex in m.UnhandledExceptions)
                        {
                            if (this.Exceptions.Contains(ex))
                            {
                                string id = ex.Method.GetId() + ex.Exception.FullName;
                                if (!got.Contains(id))
                                {
                                    got.Add(id);
                                    //ListViewItem item = MakeItem(ex, ex.Method).Item;
                                    ListViewItem item = MakeItem(ex, m).Item;
                                    items.Add(item);
                                }
                            }
                        }
                    }
                }
                else if (this.GroupByType)
                {
                    // group the exceptions by type
                    ThrownExceptionDictionary dict = new ThrownExceptionDictionary(this.Exceptions);

                    foreach (IEnumerable<ThrownException> list in dict.GetLists())
                    {
                        Type type = list.First().Exception;
                        ListViewGroup lvg = this.Groups.Add(type.FullName, type.Name);
                        list.ToList().ForEach(item => items.Add(MakeItem(item, item.Method, lvg).Item));
                    }
                }
                else
                {
                    IEnumerable<Method> methods;

                    if (this.MethodContainer is Method)
                    {

                    }
                    else
                    {
                        methods = new Method[] { this.MethodContainer as Method }.Concat(this.MethodContainer.CalledMethods);
                    }
                    foreach (ThrownException ex in this.Exceptions)
                    {
                        Method m = null;
                        if (ex.Method == this.MethodContainer)
                        {
                            m = this.MethodContainer as Method;
                        }
                        else
                        {
                            m = this.MethodContainer.CalledMethods.FirstOrDefault(md => md.UnhandledExceptions.Contains(ex));
                            if (m == null)
                            {
                                m = this.MethodContainer as Method;
                            }
                        }
                        ListViewItem item = MakeItem(ex, m).Item;
                        items.Add(item);
                    }

                }

                this.InvokeIfRequired(() =>
                {
                    try
                    {
                        this.BeginUpdate();
                        this.Items.AddRange(items.ToArray());
                    }
                    finally
                    {
                        this.EndUpdate();
                    }
                });
            });

            worker.RunWorkerAsync();
        }

        private ItemInfo selected = null;

        protected override void OnItemSelectionChanged(ListViewItemSelectionChangedEventArgs e)
        {
            ItemInfo info = e.Item.Tag as ItemInfo;

            if (selected == info)
            {
                return;
            }


            if ((info != null) && (this.MethodSelected != null) && e.IsSelected)
            {
                ThrownException te = null;

                MethodItem me = info.Data as MethodItem;
                Method caller = me.CalledBy;
                Method method = me.Method;

                ////if (info.Parent != null)
                ////{
                ////    if (me.Exception == null)
                ////    {
                ////        if (info.PreviousChild != null)
                ////        {
                ////            MethodItem previousMe = info.PreviousChild.Data as MethodItem;
                ////            if (previousMe != null)
                ////            {
                ////                caller = previousMe.Method;
                ////            }
                ////        }
                ////        else
                ////        {
                ////            ItemInfo parentInfo = info.Parent.Item.Tag as ItemInfo;
                ////            if (parentInfo != null)
                ////            {
                ////                MethodItem parentMe = parentInfo.Data as MethodItem;
                ////                if (parentMe != null)
                ////                {
                ////                    caller = parentMe.Method;
                ////                }
                ////            }
                ////        }
                ////    }
                ////}

                Method m = null;
                if (me.Exception != null)
                {
                    caller = method;
                    method = null;
                }

                MethodEventArgs ea = new MethodEventArgs(method, caller, me.Exception);
                this.MethodSelected(this, ea);


            }

            base.OnItemSelectionChanged(e);
        }

        protected CallStack GetCallStackForChild(ItemInfo info)
        {
            // child item - get the stack by moving up the list
            CallStack stack = new CallStack();
            ItemInfo previous = info;

            while (previous != null)
            {
                MethodItem te = previous.Data as MethodItem;
                stack.Push(te.Method);
                previous = previous.PreviousChild;
            }

            return stack;
        }

        /// <summary>Initialises the headers.</summary>
        private void InitHeaders()
        {
            this.Columns.Clear();

            ColumnHeader hdrException = new ColumnHeader();
            ColumnHeader hdrMethod = new ColumnHeader();
            ColumnHeader hdrClass = new ColumnHeader();
            ColumnHeader hdrAssembly = new ColumnHeader();

            hdrException.DisplayIndex = 0;
            hdrException.Text = "Method";
            hdrException.Width = 400;

            hdrMethod.DisplayIndex = 1;
            hdrMethod.Text = "Exception";
            hdrMethod.Width = 140;

            hdrClass.Text = "Class";

            hdrAssembly.Text = "Assembly";

            this.Columns.AddRange(new System.Windows.Forms.ColumnHeader[]
            {
                hdrException,
                hdrMethod,
                hdrClass,
                hdrAssembly
            });
        }
    }

}