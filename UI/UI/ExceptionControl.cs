namespace ExceptionExplorer.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Windows.Forms;
    using ExceptionExplorer.ExceptionAnalysis;
    using System.ComponentModel;
    using System.Collections;
    using Decompiler;

    /// <summary>
    /// The main Exception Explorer interface.
    /// </summary>
    public partial class ExceptionControl : UserControl
    {
        /// <summary>Gets or sets the controller.</summary>
        /// <value>The controller.</value>
        public ExceptionExplorerController Controller { get; protected set; }

        private ExceptionTree exceptionTree;
        private CallStackList callStackList;

        private TitleBar callStackListTitle;
        private TitleBar exceptionTreeTitle;

        /// <summary>Initializes a new instance of the <see cref="ExceptionControl"/> class.</summary>
        public ExceptionControl()
        {
            this.InitializeComponent();

            if (!this.DesignMode)
            {
                this.exceptionTree = this.CreateExceptionTree(new ThrownException[] { });
                this.callStackList = this.exceptionTree.CallStackList;

                this.Controller = new ExceptionExplorerController(this.classTree, this.exceptionTree, this.callStackList, this.sourceViewer);
                this.Controller.CreateExceptionTree = this.CreateExceptionTree;
                this.Controller.Settings.ShowSource.Set += val => this.sourceSplit.Panel2Collapsed = !(this.sourceViewer.Enabled = val);
                

                this.Controller.Settings.AnalysisOptions.XmlDocumentation.Value = Config.XmlDocumentationUsage.Combine;

                this.exceptionTree.ObjectChanged += new EventHandler<MethodContainerEventArgs>(exceptionTree_ObjectChanged);
                this.callStackList.ObjectChanged += new EventHandler<MethodContainerEventArgs>(callStackList_ObjectChanged);
                this.sourceViewer.MethodChanged += new EventHandler(sourceViewer_MethodChanged);
                this.sourceViewer.ProgressChanged += new EventHandler<Decompiler.ProgressChangedEventArgs>(sourceViewer_ProgressChanged);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!this.DesignMode)
            {
                this.Controller.Settings.Persistence.AddControl(this.callStackList);
            }
        }

        void sourceViewer_ProgressChanged(object sender, Decompiler.ProgressChangedEventArgs e)
        {
            ((SourceViewer)sender).DisplayedTextBox.SetWaiting(!e.Complete);
        }

        /// <summary>Creates a new exception tree. Currently, only one is used.</summary>
        /// <param name="list">The list.</param>
        /// <returns>An exception tree</returns>
        public ExceptionTree CreateExceptionTree(IEnumerable<ThrownException> list)
        {
            if (this.exceptionTree != null)
            {
                return this.exceptionTree;
            }

            ExceptionTree tree = new ExceptionTree();
            CallStackList callStackList = new CallStackList();
            TitleBar treeTitle = new TitleBar();
            TitleBar callStackTitle = new TitleBar();
            SplitContainer split = new SplitContainer();

            tree.SuspendLayout();
            callStackList.SuspendLayout();
            treeTitle.SuspendLayout();
            callStackTitle.SuspendLayout();
            split.SuspendLayout();

            tree.CallStackList = callStackList;

            int titleHeight = 18;
            int width = this.splitContainerMaster.Panel1.ClientRectangle.Width;

            //treeTitle.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            treeTitle.Dock = DockStyle.Top;
            treeTitle.BackColor = System.Drawing.SystemColors.Control;
            treeTitle.Font = new System.Drawing.Font("Segoe UI", 9F);
            //treeTitle.Location = new System.Drawing.Point(0, this.exceptionTabStrip.Height);
            treeTitle.Name = "exceptionTreeTitle";
            treeTitle.Size = new System.Drawing.Size(width, titleHeight);
            treeTitle.TabIndex = 1;
            treeTitle.Text = "Exception Tree";
            treeTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            tree.BackColor = System.Drawing.SystemColors.Window;
            tree.Controller = null;
            tree.Dock = System.Windows.Forms.DockStyle.Fill;
            tree.FullRowSelect = true;
            tree.Location = new System.Drawing.Point(0, titleHeight);
            tree.Margin = new System.Windows.Forms.Padding(0);
            tree.Name = "exceptionTree";
            tree.ImageList = this.classTree.ImageList;
            tree.Size = new System.Drawing.Size(558, 154);
            tree.TabIndex = 2;

            callStackTitle.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            callStackTitle.BackColor = System.Drawing.SystemColors.Control;
            callStackTitle.Font = new System.Drawing.Font("Segoe UI", 9F);
            callStackTitle.Location = new System.Drawing.Point(0, 0);
            callStackTitle.Name = "callStackTitle";
            callStackTitle.Size = new System.Drawing.Size(width, titleHeight);
            callStackTitle.TabIndex = 1;
            callStackTitle.Text = "Call stack";
            callStackTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            callStackList.BackColor = System.Drawing.SystemColors.Window;
            callStackList.Controller = null;
            callStackList.Dock = System.Windows.Forms.DockStyle.Fill;
            callStackList.FullRowSelect = true;
            callStackList.GroupByType = false;
            callStackList.Location = new System.Drawing.Point(0, titleHeight);
            callStackList.Margin = new System.Windows.Forms.Padding(0);
            callStackList.Name = "callStackList";
            callStackList.OwnerDraw = true;
            callStackList.Size = new System.Drawing.Size(width, 154);
            callStackList.SmallImageList = this.images;
            callStackList.TabIndex = 2;
            callStackList.UseCompatibleStateImageBehavior = false;
            callStackList.View = System.Windows.Forms.View.Details;


            split.Name = "splitExceptions";
            split.Orientation = System.Windows.Forms.Orientation.Horizontal;

            //split.Location = new System.Drawing.Point(0, this.exceptionTabStrip.Size.Height);
            //split.Size = new System.Drawing.Size(100, this.exceptionTabStrip.ClientRectangle.Height);

            split.Panel1.Controls.Add(treeTitle);
            split.Panel1.Controls.Add(tree);
            split.Panel1.Padding = new System.Windows.Forms.Padding(0, titleHeight , 0, 0);

            split.Panel2.Controls.Add(callStackTitle);
            split.Panel2.Controls.Add(callStackList);
            split.Panel2.Padding = new System.Windows.Forms.Padding(0, titleHeight, 0, 0);

            //split.Size = new System.Drawing.Size(558, 298);
            split.SplitterDistance = 122;
            split.TabIndex = 5;

            split.Dock = DockStyle.Fill;

            this.splitContainerMaster.Panel2.Controls.Add(split);
            ////this.tablePanel.Controls.Add(split, 0, 1);
            ////this.exceptionTabStrip.ActiveTab = this.exceptionTabStrip.AddTab("hello", split);

            split.Margin = new System.Windows.Forms.Padding(0);
            

            tree.ResumeLayout();
            callStackList.ResumeLayout();
            treeTitle.ResumeLayout();
            callStackTitle.ResumeLayout();
            split.ResumeLayout();

            this.exceptionTreeTitle = treeTitle;
            this.callStackListTitle = callStackTitle;

            return tree;
        }
             

        /// <summary>Handles the MethodChanged event of the sourceViewer control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void sourceViewer_MethodChanged(object sender, EventArgs e)
        {
            string sig = "";
                MemberInfo mi = this.sourceViewer.MemberInfo;
            if (mi != null)
            {
                if (mi is MethodBase)
                {
                    sig = ((MethodBase)mi).GetSignature(true);
                }
                //else if (mi is PropertyInfo)
                //{
                //    sig = ((PropertyInfo)mi).Name;                    
                //}
                else
                {
                    sig = mi.DeclaringType.Name + "." + mi.Name;
                }
            }

            if (string.IsNullOrEmpty(sig))
            {
                this.sourceCodeTitle.Text = "Source code";
            }
            else
            {
                this.sourceCodeTitle.Text = string.Format("Source code for {0}", sig);
            }
        }

        /// <summary>Handles the ObjectChanged event of the callStackList control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ExceptionExplorer.UI.MethodContainerEventArgs"/> instance containing the event data.</param>
        void callStackList_ObjectChanged(object sender, MethodContainerEventArgs e)
        {
            Type ex = this.exceptionTree.SelectedException;

            if (ex != null)
            {
                this.callStackListTitle.Text = string.Format("Call-stacks for {0} in {1}", ex.Name, e.MethodContainer.ToString());
            }
            else
            {
                this.callStackListTitle.Text = "Call-stacks";
            }
        }

        /// <summary>Handles the ObjectChanged event of the exceptionTree control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ExceptionExplorer.UI.MethodContainerEventArgs"/> instance containing the event data.</param>
        void exceptionTree_ObjectChanged(object sender, MethodContainerEventArgs e)
        {
            this.exceptionTreeTitle.Text = string.Format("Unhandled exceptions in {0}", e.MethodContainer.ToString());
        }
    }
}