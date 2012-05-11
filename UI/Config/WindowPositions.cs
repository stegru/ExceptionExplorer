// -----------------------------------------------------------------------
// <copyright file="WindowPositions.cs" company="">
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
    public class WindowPositions : OptionsBase
    {
        private class Pos : IStorable
        {
            Rectangle rect;
            FormWindowState state;
            Dictionary<string, int> splitters;
            public Form Form { get; private set; }

            public Pos()
            {
                this.splitters = new Dictionary<string, int>();
            }

            public void SetForm(Form form)
            {
                this.Form = form;
                this.Subscribe();
            }

            private void Subscribe()
            {
                this.Form.FormClosed += new FormClosedEventHandler(FormClosed);
                this.Form.Disposed += new EventHandler(FormDisposed);
            }

            private void Unsubscribe()
            {
                this.Form.FormClosed -= FormClosed;
                this.Form = null;
            }

            void FormDisposed(object sender, EventArgs e)
            {
                this.Form = null;
            }

            private void FormClosed(object sender, FormClosedEventArgs e)
            {
                if (sender != Form)
                {
                    return;
                }

                this.Update();
                this.Unsubscribe();
            }

            private IEnumerable<T> GetControlsOfType<T>(Control container) where T : Control
            {
                List<T> list = new List<T>();
                this.GetControlsOfType(container, list);
                return list;
            }

            private void GetControlsOfType<T>(Control container, List<T> list) where T : Control
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

                    this.GetControlsOfType<T>(c, list);
                }
            }

            public void AdjustForm()
            {
                this.Form.WindowState = FormWindowState.Normal;
                this.Form.Location = this.rect.Location;
                this.Form.Size = this.rect.Size;
                this.Form.WindowState = this.state;
                this.AdjustSplitters();
            }

            private void AdjustSplitters()
            {

                if (this.splitters == null)
                {
                    return;
                }

                foreach (SplitContainer split in this.GetControlsOfType<SplitContainer>(this.Form))
                {
                    int value;
                    if (this.splitters.TryGetValue(split.Name, out value))
                    {
                        split.SplitterDistance = value;
                        ////if (split.Orientation == Orientation.Horizontal)
                        ////{
                        ////    split.Height = value;
                        ////}
                        ////else
                        ////{
                        ////    split.Width = value;
                        ////}
                    }
                }
            }

            private void UpdateSplitters(Control container)
            {
                if (this.splitters == null)
                {
                    this.splitters = new Dictionary<string, int>();
                }

                foreach (SplitContainer split in this.GetControlsOfType<SplitContainer>(this.Form))
                {
                    this.splitters[split.Name] = split.SplitterDistance;
                        ////split.Orientation == Orientation.Horizontal
                        ////? split.Height
                        ////: split.Width;
                }

            }

            public void Update()
            {
                this.state = this.Form.WindowState;

                if (this.state == FormWindowState.Normal)
                {
                    this.rect = new Rectangle(this.Form.Location, this.Form.Size);
                }
                else
                {
                    this.rect = this.Form.RestoreBounds;
                }

                this.UpdateSplitters(this.Form);
            }

            public void Save(Storage store)
            {
                this.Update();
                store.SetValue("X", this.rect.X);
                store.SetValue("Y", this.rect.Y);
                store.SetValue("Width", this.rect.Width);
                store.SetValue("Height", this.rect.Height);

                store.SetValue("State", this.state);

                if (this.splitters != null)
                {
                    foreach (KeyValuePair<string, int> pair in this.splitters)
                    {
                        store.SetValue(pair.Key, pair.Value);
                    }
                }
            }

            public void Load(Storage store)
            {
                this.rect.X = store.GetValue("X", this.rect.X);
                this.rect.Y = store.GetValue("Y", this.rect.Y);
                this.rect.Width = store.GetValue("Width", this.rect.Width);
                this.rect.Height = store.GetValue("height", this.rect.Height);

                this.state = store.GetValue("State", this.state);

                if (this.Form != null)
                {
                    if (this.splitters == null)
                    {
                        this.splitters = new Dictionary<string, int>();
                    }

                    foreach (SplitContainer split in this.GetControlsOfType<SplitContainer>(this.Form))
                    {
                        int currentValue = split.SplitterDistance;
                            ////split.Orientation == Orientation.Horizontal
                            ////? split.Height
                            ////: split.Width;

                        this.splitters[split.Name] = store.GetValue(split.Name, currentValue);

                    }
                }
            }
        }

        private Dictionary<string, Pos> positions;
        private List<Form> forms;

        /// <summary>Gets the Form id.</summary>
        /// <param name="Form">The Form.</param>
        /// <returns>A string identifying the Form.</returns>
        private static string GetFormId(Form form)
        {
            return form.GetType().Name;
        }

        private Pos GetFormPos(Form form)
        {
            if (form == null)
            {
                return null;
            }

            Pos pos;
            string id = GetFormId(form);

            if (this.positions.TryGetValue(id, out pos))
            {
                return pos;
            }

            return null;
        }

        /// <summary>Initializes a new instance of the <see cref="WindowPositions"/> class.</summary>
        public WindowPositions()
            : base()
        {
            this.positions = new Dictionary<string, Pos>();
            this.forms = new List<Form>();
        }

        /// <summary>Adds the specified form.</summary>
        /// <param name="form">The form.</param>
        public void Add(Form form)
        {
            string id = GetFormId(form);

            Pos pos = this.GetFormPos(form);

            if (pos == null)
            {
                pos = new Pos();
                pos.SetForm(form);
                pos.Update();

                using (Storage store = this.GetStore(false))
                {
                    using (Storage subStore = store.CreateSubStore(id))
                    {
                        pos.Load(subStore);
                    }
                }

                pos.AdjustForm();
                this.positions.Add(id, pos);
            }
            else
            {
                if (pos.Form == null)
                {
                    pos.SetForm(form);
                }

                pos.AdjustForm();
            }
        }

        /// <summary>Load from the specified store.</summary>
        /// <param name="store">The store.</param>
        public override void Load(Storage store)
        {
            this.Location = store.Location;
            foreach (KeyValuePair<string, Pos> p in this.positions)
            {
                using (Storage s = store.CreateSubStore(p.Key))
                {
                    p.Value.Load(s);
                }
            }
        }

        /// <summary>Save to the specified store.</summary>
        /// <param name="store">The store.</param>
        public override void Save(Storage store)
        {
            this.Location = store.Location;
            foreach (KeyValuePair<string, Pos> p in this.positions)
            {
                using (Storage s = store.CreateSubStore(p.Key))
                {
                    p.Value.Save(s);
                }
            }
        }


    }
}
