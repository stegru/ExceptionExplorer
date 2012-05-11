namespace ExceptionExplorer.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;
    using ExceptionExplorer.Properties;
    using System.Drawing;
    using ExceptionExplorer.Config;

    public static class WaitingExtension
    {
        public static void SetWaiting(this Control control, bool isbusy)
        {
            control.InvokeIfRequired(() =>
            {
                Waiting.SetWaiting(control, isbusy);
            });
        }
    }

    public class Waiting : IDisposable
    {
        private PictureBox pictureBox;
        private Control control;
        private bool isLoaded;
        private bool isWaiting;
        private static Dictionary<Control, Waiting> waits;

        static Waiting()
        {
            Waiting.waits = new Dictionary<Control, Waiting>();
        }

        private bool ShowAnim
        {
            get
            {
                return Options.Current.ShowWaiting.Value;
            }
        }

        private Waiting(Control control)
        {
            this.control = control;
            this.control.Disposed += new EventHandler(control_Disposed);
        }

        void control_Disposed(object sender, EventArgs e)
        {
            Waiting.waits.Remove(this.control);
        }

        private void LoadIcon()
        {
            this.pictureBox = new PictureBox();
            this.pictureBox.Image = Resources.loading;
            this.pictureBox.Visible = false;
            this.pictureBox.Size = this.pictureBox.Image.Size;
            this.SetLocation();
            this.control.Controls.Add(this.pictureBox);
            this.control.Resize += new EventHandler(control_Resize);
            this.isLoaded = true;
        }

        private void SetLocation()
        {
            this.pictureBox.Location = new Point((this.control.ClientRectangle.Width - this.pictureBox.Size.Width) / 2, (this.control.ClientRectangle.Height - this.pictureBox.Size.Height) / 4);
        }

        void control_Resize(object sender, EventArgs e)
        {
            this.SetLocation();
        }

        private System.Timers.Timer timer;

        private bool Busy
        {
            get
            {
                return this.isWaiting;
            }
            set
            {
                if (this.isWaiting != value)
                {
                    this.isWaiting = value;

                    if (!this.isWaiting)
                    {
                        this.control.Cursor = Cursors.Default;

                        if (this.ShowAnim && this.timer != null)
                        {
                            this.timer.Stop();
                            this.timer.Enabled = false;
                            this.timer = null;
                        }

                        this.Visible = false;
                    }
                    else
                    {
                        this.control.Cursor = Cursors.AppStarting;

                        if (this.ShowAnim)
                        {
                            // show it soon
                            if (this.timer == null)
                            {
                                this.timer = new System.Timers.Timer(800);
                                this.timer.Elapsed += new System.Timers.ElapsedEventHandler(BusyTimer_Elapsed);
                                this.timer.AutoReset = false;
                            }
                            this.timer.Stop();
                            this.timer.Enabled = true;
                            this.timer.Start();
                        }
                    }
                }
            }
        }

        private void BusyTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (this.isWaiting && !this.Visible)
            {
                this.control.InvokeIfRequired(() =>
                {
                    this.Visible = true;
                });
            }
        }

        private bool Visible
        {
            get
            {
                return this.isLoaded ? this.pictureBox.Visible : false;
            }

            set
            {
                if (!this.isLoaded)
                {
                    this.LoadIcon();
                }

                if (value)
                {
                    this.SetLocation();
                }

                this.pictureBox.Visible = value;
            }
        }

        public static void SetWaiting(Control control, bool isbusy)
        {
            Waiting waiting;

            if (!Waiting.waits.TryGetValue(control, out waiting))
            {
                waiting = new Waiting(control);
                Waiting.waits.Add(control, waiting);
            }

            waiting.Busy = isbusy;
        }

        public void Dispose()
        {
            if (this.timer != null)
            {
                this.timer.Dispose();
            }
        }
    }
}
