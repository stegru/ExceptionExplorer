// -----------------------------------------------------------------------
// <copyright file="Persistence.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace ExceptionExplorer.Config
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;
    using System.Drawing;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class Persistence : IStorable
    {
        public static IEnumerable<T> GetControlsOfType<T>(Control container) where T : Control
        {
            List<T> list = new List<T>();
            GetControlsOfType(container, list);
            return list;
        }

        private static void GetControlsOfType<T>(Control container, List<T> list) where T : Control
        {
            foreach (Control c in container.Controls)
            {
                if (c is T)
                {
                    if (list == null)
                    {
                        list = new List<T>();
                    }
                    list.Add((T)c);
                }

                GetControlsOfType<T>(c, list);
            }
        }

        private class Controls : HashSet<Control> { }

        private class Values
        {
            private Dictionary<string, object> values = new Dictionary<string, object>();

            public IEnumerable<KeyValuePair<string, object>> GetAllValues()
            {
                return this.values;
            }

            private string GetKey(Control control)
            {
                return control.FindForm().Name + "!" + control.Name;
            }

            private string GetKey(Control control, string index)
            {
                if (string.IsNullOrEmpty(index))
                {
                    return this.GetKey(control);
                }

                return this.GetKey(control) + "!" + index;
            }

            public void AddValue<T>(string key, T value)
            {
                this.values[key] = value;
            }

            public void AddValue<T>(Control control, string index, T value)
            {
                this.AddValue(this.GetKey(control, index), value);
            }

            public void AddValue<T>(Control control, T value)
            {
                this.AddValue(this.GetKey(control), value);
            }

            public T GetValue<T>(Control control, T defaultValue)
            {
                return this.GetValue(control, null, defaultValue);
            }

            public T GetValue<T>(Control control, string index, T defaultValue)
            {
                string key = this.GetKey(control, index);

                object value;
                if (!this.values.TryGetValue(key, out value))
                {
                    return defaultValue;
                }

                if (value is T)
                {
                    return (T)value;
                }
                else if (typeof(T) == typeof(bool))
                {
                    dynamic b = value;
                    value = Convert.ToBoolean(b);
                    return (T)value;
                }
                else if (typeof(T) == typeof(int))
                {
                }
                else if (typeof(T) == typeof(double))
                {
                    double d;
                    if (double.TryParse(value.ToString(), out d))
                    {
                        dynamic dd = d;
                        return dd;
                    }
                }

                return defaultValue;
            }
        }

        private Dictionary<Form, Controls> allControls = new Dictionary<Form, Controls>();
        private Values values = new Values();

        private Controls GetControls(Form form)
        {
            Controls controls;

            if ((form != null) && this.allControls.TryGetValue(form, out controls))
            {
                return controls;
            }

            return null;
        }

        /// <summary>Adds the control.</summary>
        /// <param name="control">The control.</param>
        public void AddControl(Control control)
        {
            Form form = control.FindForm();
            this.AddControl(control, form);
        }

        private void AddControl(Control control, Form form)
        {
            Controls controls = this.GetControls(form);
            if (controls == null)
            {
                form.FormClosed += new FormClosedEventHandler(FormClosed);
                controls = new Controls();
                this.allControls.Add(form, controls);
            }

            if (controls.Add(control))
            {
                this.SetControlValue(control);
            }
        }

        public void AddForm(Form form)
        {
            this.AddControl(form, form);
        }

        private void FormClosed(object sender, FormClosedEventArgs e)
        {
            Form form = sender as Form;
            Controls controls = this.GetControls(form);

            if (controls == null)
            {
                return;
            }

            Control[] all = controls.ToArray();

            foreach (Control control in all)
            {
                this.RemoveControl(control);
            }

            this.allControls.Remove(form);
        }

        /// <summary>Removes the control.</summary>
        /// <param name="control">The control.</param>
        public void RemoveControl(Control control)
        {
            Form form = control.FindForm();
            Controls controls = this.GetControls(form);

            if ((controls != null) && controls.Remove(control))
            {
                // it was a known control - get the value
                this.StoreControlValue(control);
            }
        }

        /// <summary>Sets the control value.</summary>
        /// <param name="control">The control.</param>
        private void SetControlValue(Control control)
        {
            dynamic c = control;
            this.DoControl(c, false);
        }

        /// <summary>Stores the control value.</summary>
        /// <param name="control">The control.</param>
        private void StoreControlValue(Control control)
        {
            dynamic c = control;
            this.DoControl(c, true);
        }

        /// <summary>Save to the specified store.</summary>
        /// <param name="store">The store.</param>
        public void Save(Storage store)
        {
            foreach (Controls controls in this.allControls.Values)
            {
                foreach (Control c in controls)
                {
                    this.StoreControlValue(c);
                }
            }

            foreach (KeyValuePair<string, object> pair in this.values.GetAllValues())
            {
                dynamic value = pair.Value;
                store.SetValue(pair.Key, value);
            }
        }

        /// <summary>Load from the specified store.</summary>
        /// <param name="store">The store.</param>
        public void Load(Storage store)
        {
            foreach (string name in store.GetAllValueNames())
            {
                dynamic value = store.GetObjectValue(name, null);

                if (value != null)
                {
                    this.values.AddValue(name, value);
                }
            }
        }

        private void DoControl(CheckBox checkbox, bool store)
        {
            if (store)
            {
                this.values.AddValue(checkbox, checkbox.Checked);
            }
            else
            {
                checkbox.Checked = this.values.GetValue(checkbox, checkbox.Checked);
            }
        }

        private void DoControl(ListView listView, bool store)
        {
            if (listView.HeaderStyle != ColumnHeaderStyle.None)
            {
                StringBuilder value = new StringBuilder();
                foreach (ColumnHeader column in listView.Columns)
                {
                    string id = string.IsNullOrEmpty(column.Name) ? column.Text : column.Name;
                    if (store)
                    {
                        this.values.AddValue(listView, id, column.Width);
                    }
                    else
                    {
                        column.Width = this.values.GetValue(listView, id, column.Width);
                    }
                }
            }
        }

        private void DoControl(Form form, bool store)
        {
            Rectangle rect = new Rectangle(form.Location, form.Size);

            if ((form.Owner != null) && !store)
            {
                rect.X = (form.Owner.Width - rect.Width) / 2 + form.Owner.Left;
                rect.Y = (form.Owner.Height - rect.Height) / 2 + form.Owner.Top;
            }

            if (form.StartPosition == FormStartPosition.CenterParent)
            {
                if (!store)
                {
                    form.Location = rect.Location;
                    form.Size = rect.Size;
                }
            }
            else
            {
                if (store)
                {
                    if (form.WindowState != FormWindowState.Normal)
                    {
                        rect = form.RestoreBounds;
                    }

                    this.values.AddValue(form, "X", rect.X);
                    this.values.AddValue(form, "Y", rect.Y);
                    this.values.AddValue(form, "Width", rect.Width);
                    this.values.AddValue(form, "Height", rect.Height);
                    this.values.AddValue(form, "State", form.WindowState.ToString());
                }
                else
                {
                    form.StartPosition = FormStartPosition.Manual;

                    rect.X = this.values.GetValue(form, "X", rect.X);
                    rect.Y = this.values.GetValue(form, "Y", rect.Y);
                    rect.Width = Math.Max(this.values.GetValue(form, "Width", rect.Width), form.MinimumSize.Width);
                    rect.Height = Math.Max(this.values.GetValue(form, "Height", rect.Height), form.MinimumSize.Height);

                    // make sure it's on the screen
                    Rectangle screen = SystemInformation.VirtualScreen;
                    if (!SystemInformation.VirtualScreen.IntersectsWith(rect))
                    {
                        if (rect.Right < screen.Left)
                        {
                            rect.X = screen.Left;
                        }
                        else if (rect.Left > screen.Right)
                        {
                            rect.X = Math.Max(0, screen.Right - rect.Width / 2);
                        }

                        if (rect.Bottom < screen.Top)
                        {
                            rect.Y = screen.Top;
                        }
                        else if (rect.Top > screen.Bottom)
                        {
                            rect.Y = Math.Max(0, screen.Bottom - rect.Height / 2);
                        }
                    }

                    FormWindowState state = form.WindowState;
                    Enum.TryParse(this.values.GetValue(form, "State", form.WindowState.ToString()), out state);

                    form.Location = rect.Location;
                    form.Size = rect.Size;
                    form.WindowState = state;
                }
            }

            List<Control> moreControls = new List<Control>();
            moreControls.AddRange(GetControlsOfType<ListView>(form));
            moreControls.AddRange(GetControlsOfType<SplitContainer>(form));

            foreach (Control control in moreControls)
            {
                dynamic c = control;
                this.DoControl(c, store);
            }
        }

        private void DoControl(SplitContainer split, bool store)
        {
            if (split.IsSplitterFixed)
            {
                return;
            }

            double min = split.Panel1MinSize;
            double max = (split.Orientation == Orientation.Vertical ? split.Width : split.Height) - split.Panel2MinSize - min;
            double current = (split.SplitterDistance - min) / max * 100;

            if (store)
            {
                this.values.AddValue(split, current);
            }
            else
            {
                double val = this.values.GetValue<double>(split, -1);
                if (val >= 0)
                {
                    val = min + (max * val / 100);
                    val = Math.Min(max, Math.Max(min, val));

                    try
                    {
                        split.SplitterDistance = (int)val;
                    }
                    catch (InvalidOperationException)
                    {
                    }
                }
            }
        }
    }
}