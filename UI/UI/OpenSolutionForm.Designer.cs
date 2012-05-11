namespace ExceptionExplorer.UI
{
    partial class OpenSolutionForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OpenSolutionForm));
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.btnOpen = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.chkAutoClose = new System.Windows.Forms.CheckBox();
            this.solutionTreeView = new ExceptionExplorer.UI.ExtendedTreeView();
            this.SuspendLayout();
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "folder");
            this.imageList.Images.SetKeyName(1, "none");
            // 
            // btnOpen
            // 
            this.btnOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOpen.Location = new System.Drawing.Point(271, 49);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(86, 26);
            this.btnOpen.TabIndex = 1;
            this.btnOpen.Text = "&Open";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(20, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(195, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Select an assembly to open for analysis:";
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(271, 81);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(86, 26);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "&Cancel";
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // chkAutoClose
            // 
            this.chkAutoClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.chkAutoClose.AutoSize = true;
            this.chkAutoClose.Checked = true;
            this.chkAutoClose.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAutoClose.Location = new System.Drawing.Point(281, 294);
            this.chkAutoClose.Name = "chkAutoClose";
            this.chkAutoClose.Size = new System.Drawing.Size(76, 17);
            this.chkAutoClose.TabIndex = 3;
            this.chkAutoClose.Text = "Auto-close";
            this.chkAutoClose.UseVisualStyleBackColor = true;
            // 
            // solutionTreeView
            // 
            this.solutionTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.solutionTreeView.HideSelection = false;
            this.solutionTreeView.ImageIndex = 0;
            this.solutionTreeView.ImageList = this.imageList;
            this.solutionTreeView.Indent = 10;
            this.solutionTreeView.ItemHeight = 18;
            this.solutionTreeView.Location = new System.Drawing.Point(23, 49);
            this.solutionTreeView.Name = "solutionTreeView";
            this.solutionTreeView.SelectedImageIndex = 0;
            this.solutionTreeView.ShowLines = false;
            this.solutionTreeView.Size = new System.Drawing.Size(230, 262);
            this.solutionTreeView.TabIndex = 0;
            this.solutionTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.solutionTreeView_AfterSelect);
            this.solutionTreeView.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.solutionTreeView_NodeMouseDoubleClick);
            // 
            // OpenSolutionForm
            // 
            this.AcceptButton = this.btnOpen;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(380, 334);
            this.Controls.Add(this.chkAutoClose);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnOpen);
            this.Controls.Add(this.solutionTreeView);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(350, 250);
            this.Name = "OpenSolutionForm";
            this.Padding = new System.Windows.Forms.Padding(20);
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.Text = "Select an assembly";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OpenSolutionForm_FormClosing);
            this.Load += new System.EventHandler(this.OpenSolutionForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ExtendedTreeView solutionTreeView;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.CheckBox chkAutoClose;
    }
}