// -----------------------------------------------------------------------
// <copyright file="History.cs" company="">
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



    public class HistoryItem
    {
        /// <summary>Returns a hash code for this instance.</summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return this.Path.GetHashCode();
        }

        /// <summary>Determines whether the specified <see cref="System.Object"/> is equal to this instance.</summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return (obj != null) && (this.Path == ((HistoryItem)obj).Path);
        }

        /// <summary>Implements the operator ==.</summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(HistoryItem a, HistoryItem b)
        {
            if (Object.ReferenceEquals(a, null))
            {
                return Object.ReferenceEquals(b, null);
            }

            return a.Equals(b);
        }

        /// <summary>Implements the operator !=.</summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(HistoryItem a, HistoryItem b)
        {
            return !(a == b);
        }

        /// <summary>Gets the path.</summary>
        public string Path { get; private set; }

        /// <summary>Initializes a new instance of the <see cref="HistoryItem"/> class.</summary>
        /// <param name="path">The path.</param>
        public HistoryItem(string path)
        {
            this.Path = path;
        }
    }

    public class History
    {
        private Stack<HistoryItem> undo = new Stack<HistoryItem>();
        private Stack<HistoryItem> redo = new Stack<HistoryItem>();

        /// <summary>Occurs when undo or redo has been called.</summary>
        public event EventHandler Changed;

        /// <summary>Occurs when an item has been added.</summary>
        public event EventHandler Added;

        /// <summary>Gets the current item.</summary>
        public HistoryItem CurrentItem
        {
            get
            {
                return (this.undo.Count == 0) ? null : this.undo.Peek();
            }
        }

        public bool CanUndo
        {
            get { return this.undo.Count > 1; }
        }

        public bool CanRedo
        {
            get { return this.redo.Count > 0; }
        }

        /// <summary>Called when the current item has changed (on undo/redo).</summary>
        protected void OnChanged()
        {
            if (this.Changed != null)
            {
                this.Changed(this, new EventArgs());
            }
        }

        protected void OnAdded()
        {
            if (this.Added != null)
            {
                this.Added(this, new EventArgs());
            }
        }



        /// <summary>Adds the specified item.</summary>
        /// <param name="item">The item.</param>
        public void Add(HistoryItem item)
        {
            if (item != this.CurrentItem)
            {
                this.undo.Push(item);
                this.redo.Clear();
                this.OnAdded();
            }
        }

        /// <summary>Adds the specified node.</summary>
        /// <param name="node">The node.</param>
        public void Add(TreeNode node)
        {
            this.Add(new HistoryItem(node.FullPath));
        }

        /// <summary>To back to the previous.</summary>
        /// <returns>The current item.</returns>
        public HistoryItem Undo()
        {
            if (this.undo.Count == 0)
            {
                return this.CurrentItem;
            }

            HistoryItem previous = this.undo.Pop();
            this.redo.Push(previous);
            this.OnChanged();

            return this.CurrentItem;
        }

        /// <summary>Return to the next.</summary>
        /// <returns>The current item.</returns>
        public HistoryItem Redo()
        {
            if (this.redo.Count == 0)
            {
                return this.CurrentItem;
            }

            HistoryItem current = this.redo.Pop();
            this.undo.Push(current);
            this.OnChanged();

            return current;
        }

        public void Clear()
        {
            this.redo.Clear();
            this.undo.Clear();
            this.OnChanged();
        }

    }
}
