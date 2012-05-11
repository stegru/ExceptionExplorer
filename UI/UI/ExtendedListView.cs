namespace ExceptionExplorer.UI
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;
    using System.Linq;

    /// <summary>
    /// Extensions related to the TreeView control
    /// </summary>
    public class ExtendedListView : ListView
    {

        private void SortGroup(ListViewGroup group)
        {
            IEnumerable<ListViewItem> items = group.Items.Cast<ListViewItem>().OrderBy(i => i.Index);
            group.Items.Clear();

            foreach (ListViewItem item in items)
            {
                group.Items.Add(item);
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedListView"/> class.
        /// </summary>
        public ExtendedListView()
        {
            // prevents flicker
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.EnableNotifyMessage, true);
        }

        protected ItemInfo HotItem { get; set; }

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

        /// <summary>
        /// Creates the handle.
        /// </summary>
        protected override void CreateHandle()
        {
            base.CreateHandle();
            WinApi.SetWindowTheme(this.Handle, "explorer", null);
            IntPtr handle = WinApi.SendMessage(this.Handle, WinApi.LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero);
            WinApi.SetWindowTheme(handle, "explorer", null);
        }

        /// <summary>Creates a new listview item, base on the item info class given.</summary>
        /// <param name="itemInfo">The item info.</param>
        /// <returns>The new list view item.</returns>
        protected virtual ListViewItem NewItem(ItemInfo itemInfo)
        {
            return itemInfo.Item;
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
        /// Gets the image expander rect.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        protected Rectangle GetImageExpanderRect(ListViewItem item)
        {
            Size sz = item.ImageList.ImageSize;
            Rectangle rc = new Rectangle(item.Bounds.Location, sz);

            if (sz.Height != item.Bounds.Height)
            {
                rc.Y += (item.Bounds.Height - item.ImageList.ImageSize.Height) / 2;
            }

            return rc;
        }

        protected virtual void ItemExpanded(ItemInfo info, bool expanded)
        {
            ListViewItem item = info.Item;
            info.Expanded = expanded;

            if (item.SubItems == null)
            {
                return;
            }

            // show or hide the sub-items
            try
            {
                this.BeginUpdate();

                if (info.Expanded)
                {
                    int index = item.Index;
                    foreach (ListViewItem itm in info.SubItems)
                    {
                        itm.Group = info.Item.Group;
                        this.Items.Insert(++index, itm);
                    }
                }
                else
                {
                    foreach (ListViewItem i in info.SubItems)
                    {
                        this.Items.Remove(i);
                    }
                }

                return;
            }
            finally
            {
                this.EndUpdate();
            }
        }

        private void SetExpand(ItemInfo info, bool expanded)
        {
            if ((info != null) && info.Expandable && (info.Expanded != expanded))
            {
                this.ItemExpanded(info, expanded);
            }
        }

        public void ExpandItem(ListViewItem item)
        {
            this.SetExpand(item.Tag as ItemInfo, true);
        }

        public void CollapseItem(ListViewItem item)
        {
            this.SetExpand(item.Tag as ItemInfo, false);
        }

        protected bool MouseOverExpander(ItemInfo info, int x, int y)
        {
            if ((info != null) && (info.Expandable))
            {
                Rectangle rc = this.GetImageExpanderRect(info.Item);
                if (rc.Contains(x, y))
                {
                    return true;
                }
            }
            return false;
        }

        protected ItemInfo MouseOverExpander(int x, int y)
        {
            ListViewItem item = this.GetItemAt(x, y);
            if (item != null)
            {
                ItemInfo info = item.Tag as ItemInfo;
                if (this.MouseOverExpander(info, x, y))
                {
                    return info;
                }
            }
            return null;
        }

        /// <summary>
        /// Raises the <see cref="E:System.WindowPositions.Forms.Control.MouseDown"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.WindowPositions.Forms.MouseEventArgs"/> that contains the event data.</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                ItemInfo info = this.MouseOverExpander(e.X, e.Y);
                if (info != null)
                {
                    this.SetExpand(info, !info.Expanded);
                }
            }

            base.OnMouseDown(e);
        }

        protected void SetHotItem(ItemInfo info)
        {
            if (this.HotItem != null)
            {
                this.HotItem.ExpanderHot = false;
                this.Invalidate(this.GetImageExpanderRect(this.HotItem.Item));
            }

            this.HotItem = info;

            if (this.HotItem != null)
            {
                this.HotItem.ExpanderHot = true;
                this.HotItem.Item.Text = this.HotItem.Item.Text + " ";
                this.Invalidate(this.GetImageExpanderRect(this.HotItem.Item));
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            ItemInfo info = this.MouseOverExpander(e.X, e.Y);
            this.SetHotItem(info);
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            this.SetHotItem(null);
            base.OnMouseLeave(e);
        }

        #region OwnerDraw stuff
        protected override void OnDrawColumnHeader(DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
            base.OnDrawColumnHeader(e);
        }

        protected void DrawExpander(ItemInfo info, Graphics g)
        {
            Rectangle rc = this.GetImageExpanderRect(info.Item);

            bool drawn = false;
            if (VisualStyleRenderer.IsSupported)
            {

                VisualStyleElement old, explorer, hover = null;

                // create the default one
                old = info.Expanded ? VisualStyleElement.TreeView.Glyph.Opened : VisualStyleElement.TreeView.Glyph.Closed;

                // create the fancy explorer one
                explorer = VisualStyleElement.CreateElement("Explorer::TreeView", old.Part, old.State);

                // create the explorer one that is highlighted
                if (info.ExpanderHot)
                {
                    hover = VisualStyleElement.CreateElement(explorer.ClassName, 4, explorer.State);
                }

                // try each one (fancy one first)                
                foreach (VisualStyleElement e in new VisualStyleElement[] { hover, explorer, old })
                {
                    if (e == null)
                    {
                        continue;
                    }

                    if (VisualStyleRenderer.IsElementDefined(e))
                    {
                        VisualStyleRenderer renderer = new VisualStyleRenderer(e);
                        renderer.DrawBackground(g, rc);
                        drawn = true;
                        break;
                    }
                }
            }


            if (!drawn)
            {
                g.DrawRectangle(Pens.Blue, rc);
            }


        }

        protected override void OnDrawItem(DrawListViewItemEventArgs e)
        {
            e.DrawDefault = true;
            base.OnDrawItem(e);

            ItemInfo info = e.Item.Tag as ItemInfo;

            if ((info != null) && info.Expandable)
            {
                this.DrawExpander(info, e.Graphics);
            }
        }

        protected override void OnDrawSubItem(DrawListViewSubItemEventArgs e)
        {
            e.DrawDefault = true;
            base.OnDrawSubItem(e);
        }

        #endregion

    }

    /// <summary>
    /// Extra information about a list view item, which is stored in the item's .Tag property.
    /// </summary>
    public class ItemInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemInfo"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="data">The data.</param>
        /// <param name="type">The type.</param>
        public ItemInfo(string text, object data, NodeType type)
            : this(new ListViewItem(text), data, type)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemInfo"/> class.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="data">The data.</param>
        /// <param name="type">The type.</param>
        public ItemInfo(ListViewItem item, object data, NodeType type)
        {
            this.Data = data;
            this.Type = type;
            this.Item = item;
            this.Item.Tag = this;
            if (string.IsNullOrEmpty(this.Item.ImageKey))
            {
                this.Item.ImageKey = ItemInfo.ImageKeyFromObject(type);
            }
        }

        /// <summary>Gets or sets the data.</summary>
        /// <value>The data.</value>
        public object Data { get; set; }

        /// <summary>Gets or sets the item.</summary>
        /// <value>The item.</value>
        public ListViewItem Item { get; set; }

        /// <summary>Gets or sets the sub items.</summary>
        /// <value>The sub items.</value>
        public List<ListViewItem> SubItems { get; set; }

        /// <summary>
        /// Gets or sets the previous child - the child item that is listed before this one.
        /// </summary>
        /// <value>
        /// The previous child. null for the first.
        /// </value>
        public ItemInfo PreviousChild { get; set; }

        /// <summary>
        /// Gets or sets the parent item (if this one is a child).
        /// </summary>
        /// <value>
        /// The parent.
        /// </value>
        public ItemInfo Parent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ItemInfo"/> is expandable.
        /// </summary>
        /// <value>
        ///   <c>true</c> if expandable; otherwise, <c>false</c>.
        /// </value>
        public bool Expandable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ItemInfo"/> is expanded.
        /// </summary>
        /// <value>
        ///   <c>true</c> if expanded; otherwise, <c>false</c>.
        /// </value>
        public bool Expanded { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the expander has the mouse over it.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the expander is hot; otherwise, <c>false</c>.
        /// </value>
        public bool ExpanderHot { get; set; }

        /// <summary>Gets or sets the type.</summary>
        /// <value>The type.</value>
        public NodeType Type { get; set; }

        /// <summary>
        /// Images the type of the key from.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// Last word triggers "of the" reordering
        /// </returns>
        public static string ImageKeyFromType(NodeType type)
        {
            return NodeInfo.ImageKeyFromType(type);
        }

        public static string ImageKeyFromObject(object data)
        {
            return NodeInfo.ImageKeyFromObject(data);
        }
    }
}