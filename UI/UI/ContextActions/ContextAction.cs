namespace ExceptionExplorer.UI.ContextActions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;


    public class ContextAction
    {
        internal List<Control> Controls;
        public NodeType NodeType { get; set; }
        public ToolStripMenuItem MenuItem { get; set; }
        public Action<NodeInfo> Callback { get; set; }
        public string Text { get; set; }
        public Func<NodeInfo, bool> Predicate { get; set; }
        public bool NoSelection { get; set; }
        public bool FormattedText { get; set; }

        public ContextAction(string text)
        {
            this.Text = text;
            this.Controls = new List<Control>();
        }

        public ContextAction(string text, NodeType nodeType)
            : this(text)
        {
            this.NodeType = nodeType;
        }

        public ContextAction(string text, NodeType nodeType, Action<NodeInfo> callback)
            : this(text, nodeType)
        {
            this.Callback = callback;
        }

    }
}
