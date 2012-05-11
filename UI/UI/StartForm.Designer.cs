namespace ExceptionExplorer.UI
{
    partial class StartForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StartForm));
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("Open Assembly", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("Recent assemblies", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("Browse...", "open");
            System.Windows.Forms.ListViewGroup listViewGroup3 = new System.Windows.Forms.ListViewGroup("Visual Studio Solutions", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem(new string[] {
            "Open...",
            "hello",
            "poop"}, "open");
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.images = new System.Windows.Forms.ImageList(this.components);
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.recentListView = new ExceptionExplorer.UI.ExtendedListView();
            this.visualStudioListView = new ExceptionExplorer.UI.ExtendedListView();
            this.btnClose = new System.Windows.Forms.Button();
            this.chkShowAtStart = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.White;
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(583, 122);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // images
            // 
            this.images.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("images.ImageStream")));
            this.images.TransparentColor = System.Drawing.Color.Transparent;
            this.images.Images.SetKeyName(0, "open");
            this.images.Images.SetKeyName(1, "assembly");
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(32, 127);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.recentListView);
            this.splitContainer1.Panel1.Padding = new System.Windows.Forms.Padding(1, 1, 20, 1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.visualStudioListView);
            this.splitContainer1.Panel2.Padding = new System.Windows.Forms.Padding(20, 1, 1, 1);
            this.splitContainer1.Size = new System.Drawing.Size(519, 276);
            this.splitContainer1.SplitterDistance = 252;
            this.splitContainer1.SplitterWidth = 1;
            this.splitContainer1.TabIndex = 2;
            this.splitContainer1.SizeChanged += new System.EventHandler(this.splitContainer1_SizeChanged);
            // 
            // recentListView
            // 
            this.recentListView.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.recentListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.recentListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.recentListView.FullRowSelect = true;
            listViewGroup1.Header = "Open Assembly";
            listViewGroup1.Name = "open";
            listViewGroup2.Header = "Recent assemblies";
            listViewGroup2.Name = "recent";
            this.recentListView.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1,
            listViewGroup2});
            this.recentListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            listViewItem1.Group = listViewGroup1;
            listViewItem1.StateImageIndex = 0;
            listViewItem1.Tag = "*browse";
            this.recentListView.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1});
            this.recentListView.LargeImageList = this.images;
            this.recentListView.Location = new System.Drawing.Point(1, 1);
            this.recentListView.MultiSelect = false;
            this.recentListView.Name = "recentListView";
            this.recentListView.ShowItemToolTips = true;
            this.recentListView.Size = new System.Drawing.Size(231, 274);
            this.recentListView.SmallImageList = this.images;
            this.recentListView.TabIndex = 1;
            this.recentListView.TileSize = new System.Drawing.Size(200, 28);
            this.recentListView.UseCompatibleStateImageBehavior = false;
            this.recentListView.View = System.Windows.Forms.View.Tile;
            this.recentListView.ItemActivate += new System.EventHandler(this.ListView_ItemActivate);
            this.recentListView.SizeChanged += new System.EventHandler(this.recentListView_SizeChanged);
            this.recentListView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ListView_MouseClick);
            // 
            // visualStudioListView
            // 
            this.visualStudioListView.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.visualStudioListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.visualStudioListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.visualStudioListView.FullRowSelect = true;
            listViewGroup3.Header = "Visual Studio Solutions";
            listViewGroup3.Name = "visualStudioGroup";
            this.visualStudioListView.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup3});
            this.visualStudioListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            listViewItem2.Group = listViewGroup3;
            listViewItem2.StateImageIndex = 0;
            this.visualStudioListView.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem2});
            this.visualStudioListView.LargeImageList = this.images;
            this.visualStudioListView.Location = new System.Drawing.Point(20, 1);
            this.visualStudioListView.MultiSelect = false;
            this.visualStudioListView.Name = "visualStudioListView";
            this.visualStudioListView.ShowItemToolTips = true;
            this.visualStudioListView.Size = new System.Drawing.Size(245, 274);
            this.visualStudioListView.SmallImageList = this.images;
            this.visualStudioListView.TabIndex = 1;
            this.visualStudioListView.TileSize = new System.Drawing.Size(200, 28);
            this.visualStudioListView.UseCompatibleStateImageBehavior = false;
            this.visualStudioListView.View = System.Windows.Forms.View.Tile;
            this.visualStudioListView.ItemActivate += new System.EventHandler(this.ListView_ItemActivate);
            this.visualStudioListView.SizeChanged += new System.EventHandler(this.visualStudioListView_SizeChanged);
            this.visualStudioListView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ListView_MouseClick);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(464, 420);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(87, 26);
            this.btnClose.TabIndex = 3;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // chkShowAtStart
            // 
            this.chkShowAtStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkShowAtStart.AutoSize = true;
            this.chkShowAtStart.Location = new System.Drawing.Point(33, 426);
            this.chkShowAtStart.Name = "chkShowAtStart";
            this.chkShowAtStart.Size = new System.Drawing.Size(122, 17);
            this.chkShowAtStart.TabIndex = 4;
            this.chkShowAtStart.Text = "Show this on startup";
            this.chkShowAtStart.UseVisualStyleBackColor = true;
            // 
            // StartForm
            // 
            this.AcceptButton = this.btnClose;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(583, 463);
            this.Controls.Add(this.chkShowAtStart);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.pictureBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(450, 350);
            this.Name = "StartForm";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Exception Explorer";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.StartForm_FormClosed);
            this.Load += new System.EventHandler(this.StartForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private ExtendedListView recentListView;
        private System.Windows.Forms.ImageList images;
        private ExtendedListView visualStudioListView;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.CheckBox chkShowAtStart;
    }
}