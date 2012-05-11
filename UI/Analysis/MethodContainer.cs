// -----------------------------------------------------------------------
// <copyright file="MethodContainer.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace ExceptionExplorer.Analysis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Collections.ObjectModel;
    using ExceptionExplorer.Tasks;
using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Something that can contain methods
    /// </summary>
    public abstract class MethodContainer : INotifyCollectionChanged
    {
        /// <summary>The analysis</summary>
        private MethodAnalysis analysis;
        
        public ExceptionFinder ExceptionFinder { get; set; }

        internal MethodContainer()
        {
            this.CalledMethods = new ObservableCollection<MethodContainer>();
            this.UnhandledExceptions = new List<ThrownException>();
            this.ThrownExceptions = new List<ThrownException>();
            this.DocumentedThrownExceptions = new List<ThrownException>();
        }

        /// <summary>
        /// Gets or sets a value indicating whether the analysis of this <see cref="MethodContainer"/> is complete.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the analysis is complete; otherwise, <c>false</c>.
        /// </value>
        public virtual bool Complete { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="MethodContainer"/> is currently being analysed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if analysing; otherwise, <c>false</c>.
        /// </value>
        public bool Analysing { get; internal set; }

        /// <summary>
        /// Gets or sets the methods called by this method.
        /// </summary>
        /// <value>
        /// The called methods.
        /// </value>
        public ObservableCollection<MethodContainer> CalledMethods { get; protected set; }

        /// <summary>
        /// Gets or sets the unhandled exceptions that are thrown by this method (a subset of UnhandledExceptions).
        /// </summary>
        /// <value>
        /// The exceptions thrown by this method.
        /// </value>
        public List<ThrownException> ThrownExceptions { get; protected set; }

        /// <summary>
        /// Gets or sets the unhandled exceptions for this method.
        /// </summary>
        /// <value>
        /// The unhandled exceptions.
        /// </value>
        public List<ThrownException> UnhandledExceptions { get; protected set; }

        public List<ThrownException> DocumentedThrownExceptions { get; set; }

        /// <summary>
        /// Gets the method analysis.
        /// </summary>
        public MethodAnalysis Analysis
        {
            get
            {
                return this.analysis ?? (this.analysis = new MethodAnalysis(this));
            }
        }

        public virtual void Reset()
        {
            this.analysis = null;
            this.CalledMethods.Clear();
            this.DocumentedThrownExceptions.Clear();
            this.ThrownExceptions.Clear();
            this.UnhandledExceptions.Clear();
            this.Complete = false;
        }

        /// <summary>Gets the ID of this instance</summary>
        /// <returns>The id</returns>
        public abstract string GetId();

        /// <summary>
        /// Finds the shortest callstack to the provided method.
        /// </summary>
        /// <param name="method">The method to search for.</param>
        /// <returns>
        /// The shortest call-stack from this method to the specified method.
        /// </returns>
        public virtual CallStack FindStack(Method method)
        {
            CallStack shortest = null;
            foreach (Method m in this.CalledMethods)
            {
                CallStack cs = m.FindStack(method);
                if ((cs != null) && (cs.Count > 0) && ((shortest == null) || (cs.Count < shortest.Count)))
                {
                    shortest = cs;
                }
            }

            return shortest;
        }

        /// <summary>Loads the child containers.</summary>
        protected abstract void LoadChildContainers(CancellationToken cancelToken);

        public Task Load()
        {
            return Work.UI.Start(this.LoadChildContainers);
        }

        /// <summary>Returns a <see cref="System.String"/> that represents this instance.</summary>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        public abstract override string ToString();

        /// <summary>Occurs when the collection changes.</summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>Called when an item is added.</summary>
        /// <param name="methodContainer">The new method container.</param>
        protected void OnItemAdded(MethodContainer methodContainer)
        {
            if (this.CollectionChanged != null)
            {
                object[] args = new object[] {
                    this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, methodContainer, null)
                };

                foreach (Delegate d in this.CollectionChanged.GetInvocationList())
                {
                    ISynchronizeInvoke sync = d.Target as ISynchronizeInvoke;
                    if (sync == null)
                    {
                        d.DynamicInvoke(args);
                    }
                    else
                    {
                        sync.BeginInvoke(d, args);
                    }
                }
            }
        }
    }

    /// <summary>
    /// A call-stack
    /// </summary>
    public class CallStack : Stack<Method>
    {
        /// <summary>Initializes a new instance of the <see cref="CallStack"/> class.</summary>
        public CallStack() : base() { }

        /// <summary>Initializes a new instance of the <see cref="CallStack"/> class.</summary>
        /// <param name="stack">The stack.</param>
        public CallStack(CallStack stack)
            : base(stack)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="CallStack"/> class.</summary>
        /// <param name="collection">The collection.</param>
        public CallStack(IEnumerable<Method> collection)
            : base(collection)
        {
        }

    }

    /// <summary>
    /// A method call
    /// </summary>
    public class MethodCall
    {
        /// <summary>Initializes a new instance of the <see cref="MethodCall"/> class.</summary>
        /// <param name="offset">The offset.</param>
        /// <param name="method">The method.</param>
        public MethodCall(int offset, Method method)
        {
            this.Method = method;
            this.Offset = offset;
        }

        /// <summary>Gets or sets the method.</summary>
        /// <value>The method.</value>
        public Method Method { get; protected set; }

        /// <summary>Gets or sets the offset the call occurs.</summary>
        /// <value>The offset.</value>
        public int Offset { get; protected set; }
    }

}
