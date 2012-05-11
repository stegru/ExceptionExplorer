namespace ExceptionExplorer.UI
{
    partial class ResolveAssemblyForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.txtAssembly = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(219, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "The following assembly could not be located:";
            // 
            // txtAssembly
            // 
            this.txtAssembly.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAssembly.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtAssembly.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtAssembly.Location = new System.Drawing.Point(27, 56);
            this.txtAssembly.Multiline = true;
            this.txtAssembly.Name = "txtAssembly";
            this.txtAssembly.ReadOnly = true;
            this.txtAssembly.Size = new System.Drawing.Size(270, 62);
            this.txtAssembly.TabIndex = 2;
            this.txtAssembly.Text = "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
            // 
            // ResolveAssemblyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(309, 262);
            this.Controls.Add(this.txtAssembly);
            this.Controls.Add(this.label1);
            this.MinimumSize = new System.Drawing.Size(300, 300);
            this.Name = "ResolveAssemblyForm";
            this.Text = "ResolveAssemblyForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtAssembly;
    }
}