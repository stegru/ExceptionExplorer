namespace ExceptionExplorer.UI
{
    partial class AboutForm
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

        #region WindowPositions Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutForm));
            this.btnOK = new System.Windows.Forms.Button();
            this.containerPanel = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.libraryPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.lnkSite = new System.Windows.Forms.LinkLabel();
            this.lblLicencee = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblVersion = new System.Windows.Forms.Label();
            this.updatePanel = new System.Windows.Forms.Panel();
            this.updateTextPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.lblUpdate = new System.Windows.Forms.Label();
            this.lnkUpdateCheck = new System.Windows.Forms.LinkLabel();
            this.picIcon = new System.Windows.Forms.PictureBox();
            this.picLogo = new System.Windows.Forms.PictureBox();
            this.tmrCheckedSince = new System.Windows.Forms.Timer(this.components);
            this.lblFullVersion = new System.Windows.Forms.Label();
            this.containerPanel.SuspendLayout();
            this.panel1.SuspendLayout();
            this.updatePanel.SuspendLayout();
            this.updateTextPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picIcon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(392, 412);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(85, 28);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // containerPanel
            // 
            this.containerPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.containerPanel.AutoScroll = true;
            this.containerPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.containerPanel.Controls.Add(this.panel1);
            this.containerPanel.Location = new System.Drawing.Point(13, 214);
            this.containerPanel.Name = "containerPanel";
            this.containerPanel.Size = new System.Drawing.Size(464, 189);
            this.containerPanel.TabIndex = 2;
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.Controls.Add(this.libraryPanel);
            this.panel1.Controls.Add(this.lnkSite);
            this.panel1.Controls.Add(this.lblLicencee);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.lblFullVersion);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(439, 248);
            this.panel1.TabIndex = 0;
            // 
            // libraryPanel
            // 
            this.libraryPanel.AutoSize = true;
            this.libraryPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.libraryPanel.Location = new System.Drawing.Point(11, 198);
            this.libraryPanel.Name = "libraryPanel";
            this.libraryPanel.Size = new System.Drawing.Size(394, 36);
            this.libraryPanel.TabIndex = 2;
            // 
            // lnkSite
            // 
            this.lnkSite.AutoSize = true;
            this.lnkSite.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lnkSite.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkSite.Location = new System.Drawing.Point(8, 31);
            this.lnkSite.Name = "lnkSite";
            this.lnkSite.Size = new System.Drawing.Size(170, 17);
            this.lnkSite.TabIndex = 1;
            this.lnkSite.TabStop = true;
            this.lnkSite.Text = "http://exceptionexplorer.net";
            this.lnkSite.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkSite_LinkClicked);
            // 
            // lblLicencee
            // 
            this.lblLicencee.AutoSize = true;
            this.lblLicencee.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLicencee.Location = new System.Drawing.Point(91, 123);
            this.lblLicencee.Name = "lblLicencee";
            this.lblLicencee.Size = new System.Drawing.Size(79, 17);
            this.lblLicencee.TabIndex = 0;
            this.lblLicencee.Text = "Trial version";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(5, 67);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(0, 17);
            this.label6.TabIndex = 0;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(8, 169);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(323, 17);
            this.label7.TabIndex = 0;
            this.label7.Text = "This software package contains the following libraries:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(8, 123);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 17);
            this.label3.TabIndex = 0;
            this.label3.Text = "Licensed to:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(8, 87);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(209, 15);
            this.label2.TabIndex = 0;
            this.label2.Text = "Copyright © 2011-2012 Steve Grundell";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(6, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(172, 25);
            this.label1.TabIndex = 0;
            this.label1.Text = "Exception Explorer";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblVersion
            // 
            this.lblVersion.AutoSize = true;
            this.lblVersion.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblVersion.Location = new System.Drawing.Point(372, 181);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(105, 30);
            this.lblVersion.TabIndex = 3;
            this.lblVersion.Text = "Version 1";
            this.lblVersion.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // updatePanel
            // 
            this.updatePanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.updatePanel.Controls.Add(this.updateTextPanel);
            this.updatePanel.Controls.Add(this.picIcon);
            this.updatePanel.Location = new System.Drawing.Point(13, 409);
            this.updatePanel.Name = "updatePanel";
            this.updatePanel.Size = new System.Drawing.Size(373, 35);
            this.updatePanel.TabIndex = 4;
            // 
            // updateTextPanel
            // 
            this.updateTextPanel.AutoSize = true;
            this.updateTextPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.updateTextPanel.Controls.Add(this.lblUpdate);
            this.updateTextPanel.Controls.Add(this.lnkUpdateCheck);
            this.updateTextPanel.Location = new System.Drawing.Point(37, 11);
            this.updateTextPanel.MaximumSize = new System.Drawing.Size(330, 50);
            this.updateTextPanel.Name = "updateTextPanel";
            this.updateTextPanel.Size = new System.Drawing.Size(146, 13);
            this.updateTextPanel.TabIndex = 3;
            this.updateTextPanel.Resize += new System.EventHandler(this.updateTextPanel_Resize);
            // 
            // lblUpdate
            // 
            this.lblUpdate.AutoSize = true;
            this.lblUpdate.Location = new System.Drawing.Point(3, 0);
            this.lblUpdate.Name = "lblUpdate";
            this.lblUpdate.Size = new System.Drawing.Size(73, 13);
            this.lblUpdate.TabIndex = 0;
            this.lblUpdate.Text = "Latest version";
            // 
            // lnkUpdateCheck
            // 
            this.lnkUpdateCheck.AutoSize = true;
            this.lnkUpdateCheck.Location = new System.Drawing.Point(82, 0);
            this.lnkUpdateCheck.Name = "lnkUpdateCheck";
            this.lnkUpdateCheck.Size = new System.Drawing.Size(61, 13);
            this.lnkUpdateCheck.TabIndex = 1;
            this.lnkUpdateCheck.TabStop = true;
            this.lnkUpdateCheck.Text = "Check now";
            this.lnkUpdateCheck.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkUpdateCheck_LinkClicked);
            // 
            // picIcon
            // 
            this.picIcon.Image = global::ExceptionExplorer.Properties.Resources.loading;
            this.picIcon.Location = new System.Drawing.Point(-1, 1);
            this.picIcon.Name = "picIcon";
            this.picIcon.Size = new System.Drawing.Size(32, 32);
            this.picIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picIcon.TabIndex = 2;
            this.picIcon.TabStop = false;
            // 
            // picLogo
            // 
            this.picLogo.Image = global::ExceptionExplorer.Properties.Resources.logo;
            this.picLogo.Location = new System.Drawing.Point(12, 12);
            this.picLogo.Name = "picLogo";
            this.picLogo.Size = new System.Drawing.Size(465, 171);
            this.picLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.picLogo.TabIndex = 0;
            this.picLogo.TabStop = false;
            // 
            // tmrCheckedSince
            // 
            this.tmrCheckedSince.Interval = 5000;
            this.tmrCheckedSince.Tick += new System.EventHandler(this.tmrCheckedSince_Tick);
            // 
            // lblFullVersion
            // 
            this.lblFullVersion.AutoSize = true;
            this.lblFullVersion.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFullVersion.Location = new System.Drawing.Point(8, 59);
            this.lblFullVersion.Name = "lblFullVersion";
            this.lblFullVersion.Size = new System.Drawing.Size(93, 17);
            this.lblFullVersion.TabIndex = 0;
            this.lblFullVersion.Text = "Version 1.0.0.0";
            // 
            // AboutForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.CancelButton = this.btnOK;
            this.ClientSize = new System.Drawing.Size(489, 456);
            this.Controls.Add(this.updatePanel);
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.containerPanel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.picLogo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "About Exception Explorer";
            this.containerPanel.ResumeLayout(false);
            this.containerPanel.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.updatePanel.ResumeLayout(false);
            this.updatePanel.PerformLayout();
            this.updateTextPanel.ResumeLayout(false);
            this.updateTextPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picIcon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picLogo;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Panel containerPanel;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.LinkLabel lnkSite;
        private System.Windows.Forms.Label lblLicencee;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.Panel updatePanel;
        private System.Windows.Forms.LinkLabel lnkUpdateCheck;
        private System.Windows.Forms.Label lblUpdate;
        private System.Windows.Forms.PictureBox picIcon;
        private System.Windows.Forms.FlowLayoutPanel updateTextPanel;
        private System.Windows.Forms.Timer tmrCheckedSince;
        private System.Windows.Forms.FlowLayoutPanel libraryPanel;
        private System.Windows.Forms.Label lblFullVersion;
    }
}