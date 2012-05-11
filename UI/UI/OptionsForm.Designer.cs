namespace ExceptionExplorer.UI
{
    partial class OptionsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OptionsForm));
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnApply = new System.Windows.Forms.Button();
            this.settingsTabs = new System.Windows.Forms.TabControl();
            this.tabDisplay = new System.Windows.Forms.TabPage();
            this.chkStartScreen = new System.Windows.Forms.CheckBox();
            this.chkShowWait = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.optNoDistinction = new System.Windows.Forms.RadioButton();
            this.radInheritedMemberFullNames = new System.Windows.Forms.RadioButton();
            this.radSeperateBaseClass = new System.Windows.Forms.RadioButton();
            this.tabAnalysis = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.radXmlDocOnly = new System.Windows.Forms.RadioButton();
            this.radXmlDocPrefer = new System.Windows.Forms.RadioButton();
            this.radXmlDocCombine = new System.Windows.Forms.RadioButton();
            this.radXmlDocNever = new System.Windows.Forms.RadioButton();
            this.chkDocumentedFramework = new System.Windows.Forms.CheckBox();
            this.chkIncludeFramework = new System.Windows.Forms.CheckBox();
            this.tabSource = new System.Windows.Forms.TabPage();
            this.chkDecompileSelection = new System.Windows.Forms.CheckBox();
            this.chkDecompileLanguageFeatures = new System.Windows.Forms.CheckBox();
            this.chkShowXmlDoc = new System.Windows.Forms.CheckBox();
            this.chkShowUsing = new System.Windows.Forms.CheckBox();
            this.chkShowSource = new System.Windows.Forms.CheckBox();
            this.tabUpdates = new System.Windows.Forms.TabPage();
            this.lnkUpdate = new System.Windows.Forms.LinkLabel();
            this.btnCheckNow = new System.Windows.Forms.Button();
            this.lblLastResponse = new System.Windows.Forms.Label();
            this.lblLastCheck = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cboUpdates = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tmrUpdate = new System.Windows.Forms.Timer(this.components);
            this.settingsTabs.SuspendLayout();
            this.tabDisplay.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabAnalysis.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tabSource.SuspendLayout();
            this.tabUpdates.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(127, 292);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(82, 25);
            this.btnOK.TabIndex = 0;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(215, 292);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(82, 25);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnApply
            // 
            this.btnApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnApply.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnApply.Location = new System.Drawing.Point(303, 292);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(82, 25);
            this.btnApply.TabIndex = 2;
            this.btnApply.Text = "&Apply";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // settingsTabs
            // 
            this.settingsTabs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.settingsTabs.Controls.Add(this.tabDisplay);
            this.settingsTabs.Controls.Add(this.tabAnalysis);
            this.settingsTabs.Controls.Add(this.tabSource);
            this.settingsTabs.Controls.Add(this.tabUpdates);
            this.settingsTabs.Location = new System.Drawing.Point(13, 13);
            this.settingsTabs.Name = "settingsTabs";
            this.settingsTabs.SelectedIndex = 0;
            this.settingsTabs.Size = new System.Drawing.Size(372, 273);
            this.settingsTabs.TabIndex = 3;
            this.settingsTabs.TabIndexChanged += new System.EventHandler(this.settingsTabs_TabIndexChanged);
            // 
            // tabDisplay
            // 
            this.tabDisplay.Controls.Add(this.chkStartScreen);
            this.tabDisplay.Controls.Add(this.chkShowWait);
            this.tabDisplay.Controls.Add(this.groupBox1);
            this.tabDisplay.Location = new System.Drawing.Point(4, 22);
            this.tabDisplay.Name = "tabDisplay";
            this.tabDisplay.Size = new System.Drawing.Size(364, 247);
            this.tabDisplay.TabIndex = 0;
            this.tabDisplay.Text = "Display";
            this.tabDisplay.UseVisualStyleBackColor = true;
            // 
            // chkStartScreen
            // 
            this.chkStartScreen.AutoSize = true;
            this.chkStartScreen.Location = new System.Drawing.Point(17, 55);
            this.chkStartScreen.Name = "chkStartScreen";
            this.chkStartScreen.Size = new System.Drawing.Size(103, 17);
            this.chkStartScreen.TabIndex = 4;
            this.chkStartScreen.Text = "Show start page";
            this.chkStartScreen.UseVisualStyleBackColor = true;
            // 
            // chkShowWait
            // 
            this.chkShowWait.AutoSize = true;
            this.chkShowWait.Location = new System.Drawing.Point(17, 32);
            this.chkShowWait.Name = "chkShowWait";
            this.chkShowWait.Size = new System.Drawing.Size(173, 17);
            this.chkShowWait.TabIndex = 4;
            this.chkShowWait.Text = "Show animation during analysis";
            this.chkShowWait.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.optNoDistinction);
            this.groupBox1.Controls.Add(this.radInheritedMemberFullNames);
            this.groupBox1.Controls.Add(this.radSeperateBaseClass);
            this.groupBox1.Location = new System.Drawing.Point(17, 101);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(232, 115);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Inherited members";
            // 
            // optNoDistinction
            // 
            this.optNoDistinction.AutoSize = true;
            this.optNoDistinction.Location = new System.Drawing.Point(18, 76);
            this.optNoDistinction.Name = "optNoDistinction";
            this.optNoDistinction.Size = new System.Drawing.Size(89, 17);
            this.optNoDistinction.TabIndex = 0;
            this.optNoDistinction.TabStop = true;
            this.optNoDistinction.Text = "No distinction";
            this.optNoDistinction.UseVisualStyleBackColor = true;
            // 
            // radInheritedMemberFullNames
            // 
            this.radInheritedMemberFullNames.AutoSize = true;
            this.radInheritedMemberFullNames.Location = new System.Drawing.Point(18, 53);
            this.radInheritedMemberFullNames.Name = "radInheritedMemberFullNames";
            this.radInheritedMemberFullNames.Size = new System.Drawing.Size(113, 17);
            this.radInheritedMemberFullNames.TabIndex = 0;
            this.radInheritedMemberFullNames.TabStop = true;
            this.radInheritedMemberFullNames.Text = "Show class names";
            this.radInheritedMemberFullNames.UseVisualStyleBackColor = true;
            // 
            // radSeperateBaseClass
            // 
            this.radSeperateBaseClass.AutoSize = true;
            this.radSeperateBaseClass.Location = new System.Drawing.Point(18, 30);
            this.radSeperateBaseClass.Name = "radSeperateBaseClass";
            this.radSeperateBaseClass.Size = new System.Drawing.Size(92, 17);
            this.radSeperateBaseClass.TabIndex = 0;
            this.radSeperateBaseClass.TabStop = true;
            this.radSeperateBaseClass.Text = "List separately";
            this.radSeperateBaseClass.UseVisualStyleBackColor = true;
            // 
            // tabAnalysis
            // 
            this.tabAnalysis.Controls.Add(this.groupBox2);
            this.tabAnalysis.Controls.Add(this.chkDocumentedFramework);
            this.tabAnalysis.Controls.Add(this.chkIncludeFramework);
            this.tabAnalysis.Location = new System.Drawing.Point(4, 22);
            this.tabAnalysis.Name = "tabAnalysis";
            this.tabAnalysis.Padding = new System.Windows.Forms.Padding(3);
            this.tabAnalysis.Size = new System.Drawing.Size(364, 247);
            this.tabAnalysis.TabIndex = 1;
            this.tabAnalysis.Text = "Analysis";
            this.tabAnalysis.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.radXmlDocOnly);
            this.groupBox2.Controls.Add(this.radXmlDocPrefer);
            this.groupBox2.Controls.Add(this.radXmlDocCombine);
            this.groupBox2.Controls.Add(this.radXmlDocNever);
            this.groupBox2.Location = new System.Drawing.Point(17, 89);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(301, 130);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "XML Documentation";
            // 
            // radXmlDocOnly
            // 
            this.radXmlDocOnly.AutoSize = true;
            this.radXmlDocOnly.Location = new System.Drawing.Point(23, 98);
            this.radXmlDocOnly.Name = "radXmlDocOnly";
            this.radXmlDocOnly.Size = new System.Drawing.Size(168, 17);
            this.radXmlDocOnly.TabIndex = 0;
            this.radXmlDocOnly.TabStop = true;
            this.radXmlDocOnly.Text = "Use exclusively, no IL analysis";
            this.radXmlDocOnly.UseVisualStyleBackColor = true;
            // 
            // radXmlDocPrefer
            // 
            this.radXmlDocPrefer.AutoSize = true;
            this.radXmlDocPrefer.Location = new System.Drawing.Point(23, 75);
            this.radXmlDocPrefer.Name = "radXmlDocPrefer";
            this.radXmlDocPrefer.Size = new System.Drawing.Size(220, 17);
            this.radXmlDocPrefer.TabIndex = 0;
            this.radXmlDocPrefer.TabStop = true;
            this.radXmlDocPrefer.Text = "Use if available, otherwise use IL analysis";
            this.radXmlDocPrefer.UseVisualStyleBackColor = true;
            // 
            // radXmlDocCombine
            // 
            this.radXmlDocCombine.AutoSize = true;
            this.radXmlDocCombine.Location = new System.Drawing.Point(23, 52);
            this.radXmlDocCombine.Name = "radXmlDocCombine";
            this.radXmlDocCombine.Size = new System.Drawing.Size(140, 17);
            this.radXmlDocCombine.TabIndex = 0;
            this.radXmlDocCombine.TabStop = true;
            this.radXmlDocCombine.Text = "Combine with IL analysis";
            this.radXmlDocCombine.UseVisualStyleBackColor = true;
            // 
            // radXmlDocNever
            // 
            this.radXmlDocNever.AutoSize = true;
            this.radXmlDocNever.Location = new System.Drawing.Point(23, 29);
            this.radXmlDocNever.Name = "radXmlDocNever";
            this.radXmlDocNever.Size = new System.Drawing.Size(155, 17);
            this.radXmlDocNever.TabIndex = 0;
            this.radXmlDocNever.TabStop = true;
            this.radXmlDocNever.Text = "Ignore - only use IL analysis";
            this.radXmlDocNever.UseVisualStyleBackColor = true;
            // 
            // chkDocumentedFramework
            // 
            this.chkDocumentedFramework.AutoSize = true;
            this.chkDocumentedFramework.Location = new System.Drawing.Point(40, 53);
            this.chkDocumentedFramework.Name = "chkDocumentedFramework";
            this.chkDocumentedFramework.Size = new System.Drawing.Size(163, 17);
            this.chkDocumentedFramework.TabIndex = 0;
            this.chkDocumentedFramework.Text = "Documented exceptions only";
            this.chkDocumentedFramework.UseVisualStyleBackColor = true;
            // 
            // chkIncludeFramework
            // 
            this.chkIncludeFramework.AutoSize = true;
            this.chkIncludeFramework.Location = new System.Drawing.Point(17, 30);
            this.chkIncludeFramework.Name = "chkIncludeFramework";
            this.chkIncludeFramework.Size = new System.Drawing.Size(251, 17);
            this.chkIncludeFramework.TabIndex = 0;
            this.chkIncludeFramework.Text = "Perform analysis on .NET framework assemblies";
            this.chkIncludeFramework.UseVisualStyleBackColor = true;
            // 
            // tabSource
            // 
            this.tabSource.Controls.Add(this.chkDecompileSelection);
            this.tabSource.Controls.Add(this.chkDecompileLanguageFeatures);
            this.tabSource.Controls.Add(this.chkShowXmlDoc);
            this.tabSource.Controls.Add(this.chkShowUsing);
            this.tabSource.Controls.Add(this.chkShowSource);
            this.tabSource.Location = new System.Drawing.Point(4, 22);
            this.tabSource.Name = "tabSource";
            this.tabSource.Padding = new System.Windows.Forms.Padding(3);
            this.tabSource.Size = new System.Drawing.Size(364, 247);
            this.tabSource.TabIndex = 2;
            this.tabSource.Text = "Source code";
            this.tabSource.UseVisualStyleBackColor = true;
            // 
            // chkDecompileSelection
            // 
            this.chkDecompileSelection.AutoSize = true;
            this.chkDecompileSelection.Location = new System.Drawing.Point(30, 58);
            this.chkDecompileSelection.Name = "chkDecompileSelection";
            this.chkDecompileSelection.Size = new System.Drawing.Size(238, 17);
            this.chkDecompileSelection.TabIndex = 4;
            this.chkDecompileSelection.Text = "Automatically decompile the selected method";
            this.chkDecompileSelection.UseVisualStyleBackColor = true;
            // 
            // chkDecompileLanguageFeatures
            // 
            this.chkDecompileLanguageFeatures.AutoSize = true;
            this.chkDecompileLanguageFeatures.Location = new System.Drawing.Point(30, 127);
            this.chkDecompileLanguageFeatures.Name = "chkDecompileLanguageFeatures";
            this.chkDecompileLanguageFeatures.Size = new System.Drawing.Size(164, 17);
            this.chkDecompileLanguageFeatures.TabIndex = 4;
            this.chkDecompileLanguageFeatures.Text = "Decompile language features";
            this.chkDecompileLanguageFeatures.UseVisualStyleBackColor = true;
            // 
            // chkShowXmlDoc
            // 
            this.chkShowXmlDoc.AutoSize = true;
            this.chkShowXmlDoc.Location = new System.Drawing.Point(30, 104);
            this.chkShowXmlDoc.Name = "chkShowXmlDoc";
            this.chkShowXmlDoc.Size = new System.Drawing.Size(151, 17);
            this.chkShowXmlDoc.TabIndex = 4;
            this.chkShowXmlDoc.Text = "Show XML documentation";
            this.chkShowXmlDoc.UseVisualStyleBackColor = true;
            // 
            // chkShowUsing
            // 
            this.chkShowUsing.AutoSize = true;
            this.chkShowUsing.Location = new System.Drawing.Point(30, 81);
            this.chkShowUsing.Name = "chkShowUsing";
            this.chkShowUsing.Size = new System.Drawing.Size(143, 17);
            this.chkShowUsing.TabIndex = 4;
            this.chkShowUsing.Text = "Show Using declarations";
            this.chkShowUsing.UseVisualStyleBackColor = true;
            // 
            // chkShowSource
            // 
            this.chkShowSource.AutoSize = true;
            this.chkShowSource.Location = new System.Drawing.Point(20, 30);
            this.chkShowSource.Name = "chkShowSource";
            this.chkShowSource.Size = new System.Drawing.Size(172, 17);
            this.chkShowSource.TabIndex = 4;
            this.chkShowSource.Text = "Show decompiled source code";
            this.chkShowSource.UseVisualStyleBackColor = true;
            this.chkShowSource.CheckedChanged += new System.EventHandler(this.chkShowSource_CheckedChanged);
            // 
            // tabUpdates
            // 
            this.tabUpdates.Controls.Add(this.lnkUpdate);
            this.tabUpdates.Controls.Add(this.btnCheckNow);
            this.tabUpdates.Controls.Add(this.lblLastResponse);
            this.tabUpdates.Controls.Add(this.lblLastCheck);
            this.tabUpdates.Controls.Add(this.label4);
            this.tabUpdates.Controls.Add(this.label2);
            this.tabUpdates.Controls.Add(this.cboUpdates);
            this.tabUpdates.Controls.Add(this.label1);
            this.tabUpdates.Location = new System.Drawing.Point(4, 22);
            this.tabUpdates.Name = "tabUpdates";
            this.tabUpdates.Padding = new System.Windows.Forms.Padding(3);
            this.tabUpdates.Size = new System.Drawing.Size(364, 247);
            this.tabUpdates.TabIndex = 3;
            this.tabUpdates.Text = "Updates";
            this.tabUpdates.UseVisualStyleBackColor = true;
            // 
            // lnkUpdate
            // 
            this.lnkUpdate.AutoSize = true;
            this.lnkUpdate.Location = new System.Drawing.Point(120, 157);
            this.lnkUpdate.Name = "lnkUpdate";
            this.lnkUpdate.Size = new System.Drawing.Size(115, 13);
            this.lnkUpdate.TabIndex = 5;
            this.lnkUpdate.TabStop = true;
            this.lnkUpdate.Text = "Download new version";
            this.lnkUpdate.Visible = false;
            this.lnkUpdate.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkUpdate_LinkClicked);
            // 
            // btnCheckNow
            // 
            this.btnCheckNow.Location = new System.Drawing.Point(205, 67);
            this.btnCheckNow.Name = "btnCheckNow";
            this.btnCheckNow.Size = new System.Drawing.Size(75, 23);
            this.btnCheckNow.TabIndex = 4;
            this.btnCheckNow.Text = "Check now";
            this.btnCheckNow.UseVisualStyleBackColor = true;
            this.btnCheckNow.Click += new System.EventHandler(this.btnCheckNow_Click);
            // 
            // lblLastResponse
            // 
            this.lblLastResponse.AutoSize = true;
            this.lblLastResponse.Location = new System.Drawing.Point(120, 127);
            this.lblLastResponse.Name = "lblLastResponse";
            this.lblLastResponse.Size = new System.Drawing.Size(24, 13);
            this.lblLastResponse.TabIndex = 3;
            this.lblLastResponse.Text = "n/a";
            // 
            // lblLastCheck
            // 
            this.lblLastCheck.AutoSize = true;
            this.lblLastCheck.Location = new System.Drawing.Point(120, 101);
            this.lblLastCheck.Name = "lblLastCheck";
            this.lblLastCheck.Size = new System.Drawing.Size(36, 13);
            this.lblLastCheck.TabIndex = 3;
            this.lblLastCheck.Text = "Never";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(20, 127);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(76, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Last response:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(20, 101);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Last checked:";
            // 
            // cboUpdates
            // 
            this.cboUpdates.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboUpdates.FormattingEnabled = true;
            this.cboUpdates.Location = new System.Drawing.Point(123, 33);
            this.cboUpdates.Name = "cboUpdates";
            this.cboUpdates.Size = new System.Drawing.Size(157, 21);
            this.cboUpdates.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(20, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(97, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Check for updates:";
            // 
            // tmrUpdate
            // 
            this.tmrUpdate.Interval = 5000;
            this.tmrUpdate.Tick += new System.EventHandler(this.tmrUpdate_Tick);
            // 
            // OptionsForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(397, 329);
            this.Controls.Add(this.settingsTabs);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(363, 367);
            this.Name = "OptionsForm";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Options";
            this.Load += new System.EventHandler(this.OptionsForm_Load);
            this.settingsTabs.ResumeLayout(false);
            this.tabDisplay.ResumeLayout(false);
            this.tabDisplay.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabAnalysis.ResumeLayout(false);
            this.tabAnalysis.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.tabSource.ResumeLayout(false);
            this.tabSource.PerformLayout();
            this.tabUpdates.ResumeLayout(false);
            this.tabUpdates.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.TabControl settingsTabs;
        private System.Windows.Forms.TabPage tabDisplay;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox chkShowSource;
        private System.Windows.Forms.RadioButton optNoDistinction;
        private System.Windows.Forms.RadioButton radInheritedMemberFullNames;
        private System.Windows.Forms.RadioButton radSeperateBaseClass;
        private System.Windows.Forms.TabPage tabAnalysis;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton radXmlDocOnly;
        private System.Windows.Forms.RadioButton radXmlDocCombine;
        private System.Windows.Forms.RadioButton radXmlDocNever;
        private System.Windows.Forms.CheckBox chkIncludeFramework;
        private System.Windows.Forms.RadioButton radXmlDocPrefer;
        private System.Windows.Forms.CheckBox chkShowWait;
        private System.Windows.Forms.CheckBox chkDocumentedFramework;
        private System.Windows.Forms.TabPage tabSource;
        private System.Windows.Forms.CheckBox chkDecompileLanguageFeatures;
        private System.Windows.Forms.CheckBox chkShowXmlDoc;
        private System.Windows.Forms.CheckBox chkShowUsing;
        private System.Windows.Forms.CheckBox chkDecompileSelection;
        private System.Windows.Forms.CheckBox chkStartScreen;
        private System.Windows.Forms.TabPage tabUpdates;
        private System.Windows.Forms.LinkLabel lnkUpdate;
        private System.Windows.Forms.Button btnCheckNow;
        private System.Windows.Forms.Label lblLastResponse;
        private System.Windows.Forms.Label lblLastCheck;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cboUpdates;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Timer tmrUpdate;
    }
}