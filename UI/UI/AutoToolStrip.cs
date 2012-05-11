using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ExceptionExplorer.UI
{
    /// <summary>
    /// A <see cref="ToolStrip"/> that is linked to a <see cref="MenuStrip"/>.
    /// The click events of the buttons trigger the menu item's, and the Checked properties are automatically updated.
    /// </summary>
    public partial class AutoToolStrip : ToolStrip
    {
        internal delegate bool ItemEnum(ToolStripItem item, object param);

        public AutoToolStrip()
        {
        }

        public AutoToolStrip AddMenuItem(ToolStripMenuItem menuItem)
        {
            if (menuItem == null)
            {
                ToolStripSeparator s = new ToolStripSeparator();
                this.Items.Add(s);
                return this;
            }

            AutoToolStripButton item = new AutoToolStripButton(menuItem);
            this.Items.Add(item);

            return this;
        }
    }


    public class AutoToolStripButton : ToolStripButton
    {
        public ToolStripMenuItem MenuItem { get; set; }

        public AutoToolStripButton(ToolStripMenuItem menuItem)
            : base(menuItem.Text, menuItem.Image)
        {
            if (menuItem == null)
            {
                throw new ArgumentNullException("menuItem");
            }
            this.MenuItem = menuItem;

            this.Name = "tool" + (menuItem.Name.StartsWith("mnu") ? menuItem.Name.Substring(3) : menuItem.Name);
            this.ToolTipText = menuItem.ToolTipText;
            this.DisplayStyle = ToolStripItemDisplayStyle.Image;

            this.MenuItem.Disposed += new EventHandler(MenuItem_Disposed);

            // set up the event handlers to change when the menu item changes
            this.MenuItem.CheckStateChanged += new EventHandler(MenuItem_CheckStateChanged);
            this.MenuItem.EnabledChanged += new EventHandler(MenuItem_EnabledChanged);
            this.MenuItem.TextChanged += new EventHandler(MenuItem_TextChanged);
            this.MenuItem.AvailableChanged += new EventHandler(MenuItem_AvailableChanged);
            this.MenuItem.CheckedChanged += new EventHandler(MenuItem_CheckedChanged);

            // call those handlers to set the item's current state.
            EventArgs e = new EventArgs();
            MenuItem_CheckStateChanged(this.MenuItem, e);
            MenuItem_EnabledChanged(this.MenuItem, e);
            MenuItem_TextChanged(this.MenuItem, e);
            MenuItem_AvailableChanged(this.MenuItem, e);
            MenuItem_CheckedChanged(this.MenuItem, e);
        }

        void MenuItem_Disposed(object sender, EventArgs e)
        {
            if (Environment.TickCount == 123)
            {
                throw new DivideByZeroException();
            }
            if (sender == this.MenuItem)
            {
                this.MenuItem = null;
            }
        }

        void MenuItem_CheckedChanged(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
            if (menuItem != null)
            {
                this.Checked = menuItem.Checked;
            }
        }

        void MenuItem_AvailableChanged(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
            if (menuItem != null)
            {
                this.Available = menuItem.Available;
            }
        }

        void MenuItem_TextChanged(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
            if (menuItem != null)
            {
                this.Text = menuItem.Text;
            }
        }

        void MenuItem_EnabledChanged(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
            if (menuItem != null)
            {
                this.Enabled = menuItem.Enabled;
            }
        }

        void MenuItem_CheckStateChanged(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
            if (menuItem != null)
            {
                this.CheckState = menuItem.CheckState;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.WindowPositions.Forms.Control.Click"/> event for the menu item.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnClick(EventArgs e)
        {
            this.MenuItem.PerformClick();
            base.OnClick(e);
        }
    }

}
