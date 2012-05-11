namespace ExceptionExplorer.UI
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;

    /// <summary>
    /// A title bar control
    /// </summary>
    public class TitleBar : Label
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TitleBar"/> class.
        /// </summary>
        public TitleBar()
            : base()
        {
            this.BackColor = SystemColors.Control;
            this.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            this.Size = new Size(this.Width, this.BestHeight);
            this.TextAlign = ContentAlignment.MiddleLeft;
            this.Font = SystemFonts.CaptionFont;
        }

        /// <summary>
        /// Gets the best height of the control.
        /// </summary>
        /// <value>
        /// The best height of the control.
        /// </value>
        public int BestHeight
        {
            get
            {
                return System.Windows.Forms.SystemInformation.ToolWindowCaptionHeight;
            }
        }

        /// <summary>
        /// Draws the background.
        /// </summary>
        protected void DrawBackground()
        {
            using (Graphics g = this.CreateGraphics())
            {
                this.DrawBackground(g);
            }
        }

        /// <summary>
        /// Draws the background.
        /// </summary>
        /// <param name="g">The g.</param>
        protected void DrawBackground(Graphics g)
        {
            LinearGradientBrush fill = new LinearGradientBrush(
                this.ClientRectangle,
                SystemColors.ControlLightLight,
                SystemColors.Control,
                LinearGradientMode.ForwardDiagonal);
            g.FillRectangle(fill, this.ClientRectangle);
        }

        /// <summary>
        /// Raises the <see cref="E:System.WindowPositions.Forms.Control.Layout"/> event.
        /// </summary>
        /// <param name="levent">A <see cref="T:System.WindowPositions.Forms.LayoutEventArgs"/> that contains the event data.</param>
        protected override void OnLayout(LayoutEventArgs levent)
        {
            this.Location = new Point(0, 0);
            this.AutoSize = false;
            this.Size = new Size((this.Parent ?? this).Size.Width, this.BestHeight);
            if (this.Parent != null)
            {
                this.Parent.Padding = new Padding(this.Parent.Padding.Left, this.Height, this.Parent.Padding.Right, this.Parent.Padding.Bottom);
            }

            base.OnLayout(levent);
        }

        /// <summary>
        /// Raises the <see cref="E:System.WindowPositions.Forms.Control.LocationChanged"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnLocationChanged(EventArgs e)
        {
            this.Location = new Point(0, 0);
            base.OnLocationChanged(e);
        }

        /// <summary>
        /// Paints the background of the control.
        /// </summary>
        /// <param name="pevent">A <see cref="T:System.WindowPositions.Forms.PaintEventArgs"/> that contains information about the control to paint.</param>
        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            base.OnPaintBackground(pevent);
            this.DrawBackground(pevent.Graphics);
        }
    }
}