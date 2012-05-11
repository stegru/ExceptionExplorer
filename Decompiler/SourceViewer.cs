namespace Decompiler
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Windows.Forms;
    using Decompiler.Extensions;
    using ICSharpCode.Decompiler;
    using ICSharpCode.Decompiler.ILAst;
    using ICSharpCode.NRefactory;
    using ICSharpCode.NRefactory.CSharp;
    using Mono.Cecil;
    using System.Text.RegularExpressions;
    using System.Globalization;
    using System.Threading;


    public class ProgressChangedEventArgs : EventArgs
    {
        public bool Complete { get; private set; }

        public ProgressChangedEventArgs(bool complete)
        {
            this.Complete = complete;
        }
    }

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SourceViewer : Control
    {

        public bool ShowXmlDoc { get; set; }
        public bool ShowUsing { get; set; }
        public bool DecompileLanguageFeatures { get; set; }

        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        public void OnProgressChanged(bool complete)
        {
            if (this.ProgressChanged != null)
            {
                this.ProgressChanged(this, new ProgressChangedEventArgs(complete));
            }
        }

        private static Func<AssemblyName, Assembly, Assembly> assemblyResolveCallback;

        public static void SetAssemblyResolveCallback(Func<AssemblyName, Assembly, Assembly> callback)
        {
            DefaultAssemblyResolver resolver = GlobalAssemblyResolver.Instance as DefaultAssemblyResolver;
            if (resolver != null)
            {
                assemblyResolveCallback = callback;
                resolver.ResolveFailure += new AssemblyResolveEventHandler(DefaultAssemblyResolver_ResolveFailure);
            }
        }

        private static AssemblyDefinition DefaultAssemblyResolver_ResolveFailure(object sender, AssemblyNameReference reference)
        {
            if (assemblyResolveCallback != null)
            {
                Assembly asm = assemblyResolveCallback(new AssemblyName(reference.FullName), null);
                return ModuleDefinition.ReadModule(asm.Location).Assembly;
            }

            return null;
        }


        private const int tabSize = 4;

        public enum MethodType
        {
            None = 0,
            Call = 1,
            Set = 2,
            Get = 3,
            Ctor = 4
        }

        [Flags]
        public enum HighlightType
        {
            None = 0,
            Method = 1,
            Exception = 2,
            DocumentedException = 4
        }

        public enum HighlightDisplay
        {
            None = 0,
            Highlight = 1,
            Icon = 2
        }

        public class HighlightItem
        {
            public int Offset { get; set; }
            public MethodBase Method { get; private set; }
            public MethodType MethodType { get; private set; }
            public HighlightType HighlightType { get; set; }
            public Type ExceptionType { get; set; }
            public HighlightDisplay Display { get; set; }
            public string Tooltip { get; set; }

            public HighlightItem()
            {
            }

            public HighlightItem(MethodBase method)
                : this()
            {
                this.Method = method;
                this.HighlightType = SourceViewer.HighlightType.Method;
                this.MethodType = this.GetMethodType();
            }

            public HighlightItem(Type exception)
                : this()
            {
                this.ExceptionType = exception;
                this.HighlightType = SourceViewer.HighlightType.Exception;
            }

            private MethodType GetMethodType()
            {
                if ((this.HighlightType == HighlightType.Method) && (this.Method != null))
                {
                    if (this.Method.IsSpecialName && this.Method.IsHideBySig)
                    {
                        if (this.Method.Name.StartsWith("get_"))
                        {
                            return MethodType.Get;
                        }
                        else if (this.Method.Name.StartsWith("set_"))
                        {
                            return MethodType.Set;
                        }
                    }

                    if (this.Method.IsConstructor)
                    {
                        return MethodType.Ctor;
                    }

                    return MethodType.Call;
                }

                return SourceViewer.MethodType.None;
            }
        }

        private class Link
        {
            public int Position { get; private set; }
            public int Length { get; private set; }
            public string Text { get; private set; }

            public Link(int position, int length, string text)
            {
                this.Position = position;
                this.Length = length;
                this.Text = text;
            }
        }

        private List<Link> links;

        private bool first = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceViewer"/> class.
        /// </summary>
        public SourceViewer()
            : base()
        {
            this.TextBox = this.CreateTextBox();
            this.DisplayedTextBox = this.TextBox;

            this.links = new List<Link>();
            this.DisplayedTextBox.MouseMove += new MouseEventHandler(SourceViewer_MouseMove);
            this.DisplayedTextBox.Click += new EventHandler(SourceViewer_Click);

            this.Controls.Add(this.DisplayedTextBox);
            this.Reset();
        }

        /// <summary>Occurs when the method has changed.</summary>
        public event EventHandler MethodChanged;

        /// <summary>Gets the method base.</summary>
        public MemberInfo MemberInfo { get; private set; }

        /// <summary>Gets the rich text box.</summary>
        /// <value>The rich text box control.</value>
        protected RichTextBox TextBox { get; private set; }

        /// <summary>Gets the rich text box.</summary>
        /// <value>The rich text box control.</value>
        public RichTextBox DisplayedTextBox { get; private set; }

        protected RichTextBox CreateTextBox()
        {
            RichTextBox textBox = new RichTextBox()
            {
                Dock = DockStyle.Fill,
                Text = string.Empty,
                WordWrap = false,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                ShowSelectionMargin = true
            };

            return textBox;
        }

        /// <summary>Resets the text box.</summary>
        public void Reset()
        {
            SyntaxStyle defaultStyle = SyntaxStyle.Style(SyntaxType.Default);

            this.TextBox.Clear();
            this.TextBox.Font = defaultStyle.Font;

            if (defaultStyle.Background == Color.Transparent)
            {
                this.TextBox.BackColor = SystemColors.Window;
            }
            else
            {
                this.TextBox.BackColor = defaultStyle.Background;
            }

            this.TextBox.ForeColor = defaultStyle.Foreground;
            this.TextBox.SelectAll();
            this.TextBox.SelectionBackColor = this.TextBox.BackColor;
            this.TextBox.SelectionColor = this.TextBox.ForeColor;

            this.links.Clear();
        }

        private List<HighlightItem> lastHighlightItems;


        public void Reload(CancellationToken cancelToken)
        {
            this.SetSource(this.MemberInfo, this.lastHighlightItems, cancelToken);
        }

        /// <summary>Sets the source code.</summary>
        /// <param name="methodBase">The method to decompile.</param>
        public void SetSource(MethodBase methodBase, CancellationToken cancelToken)
        {
            this.SetSource(methodBase, null, cancelToken);
        }

        public void SetSource(MemberInfo memberInfo, IEnumerable<HighlightItem> highlightItems, CancellationToken cancelToken)
        {
            if (memberInfo == null)
            {
                return;
            }

            this.MemberInfo = memberInfo;
            this.lastHighlightItems = (highlightItems ?? new HighlightItem[0]).ToList();

            if (!this.Enabled)
            {
                return;
            }

            try
            {
                this.OnProgressChanged(false);
                lock (this)
                {
                    if (this.MemberInfo != memberInfo)
                    {
                        // it changed while waiting in the lock
                        return;
                    }

                    this.InvokeIfRequired(() =>
                    {
                        this.Reset();

                        if (this.MethodChanged != null)
                        {
                            this.MethodChanged(this, new EventArgs());
                        }
                    });

                    this.TextBox = this.CreateTextBox();
                    this.Reset();

                    Highlighter highlighter = new Highlighter(this, highlightItems);

                    DecompilerSettings settings = new DecompilerSettings()
                    {
                        UsingDeclarations = this.ShowUsing,
                        ShowXmlDocumentation = this.ShowXmlDoc,
                        UseDebugSymbols = true
                    };

                    settings.AnonymousMethods =
                        settings.ExpressionTrees =
                        settings.YieldReturn =
                        settings.AutomaticProperties =
                        settings.AutomaticEvents =
                        settings.UsingStatement =
                        settings.ForEachStatement =
                        settings.LockStatement =
                        settings.SwitchStatementOnString =
                        settings.QueryExpressions =
                        settings.ObjectOrCollectionInitializers = this.DecompileLanguageFeatures;

                    // vb.net stuff:
                    settings.IntroduceIncrementAndDecrement = settings.AlwaysGenerateExceptionVariableForCatchBlocks = this.DecompileLanguageFeatures;

                    if (memberInfo is PropertyInfo)
                    {
                        ((PropertyInfo)memberInfo).DecompileProperty(highlighter, settings);
                    }
                    else
                    {
                        ((MethodBase)memberInfo).DecompileMethod(highlighter, settings);
                    }

                    highlighter.Complete();
                    int pos = this.TextBox.SelectionStart;

                    string rtf = this.TextBox.Rtf;

                    this.TextBox = this.DisplayedTextBox;

                    this.InvokeIfRequired(() =>
                    {
                        if (first)
                        {
                            this.DisplayedTextBox.ScrollBars = RichTextBoxScrollBars.ForcedBoth;
                            this.DisplayedTextBox.ScrollBars = RichTextBoxScrollBars.Both;
                            this.first = false;
                        }
                        this.Reset();
                        this.DisplayedTextBox.Rtf = rtf;
                        this.DisplayedTextBox.Select(pos, 0);
                        this.DisplayedTextBox.ScrollToCaret();
                    });
                }
            }
            finally
            {
                this.OnProgressChanged(true);
            }
        }

        private void SetSelectionStyle(SyntaxType syntaxType)
        {
            // set the color
            SyntaxStyle style = SyntaxStyle.Style(syntaxType);

            this.TextBox.SelectionColor = style.Foreground;

            Color bg = style.Background;
            if (bg == Color.Transparent)
            {
                if (style.Type != SyntaxType.Default)
                {
                    bg = SyntaxStyle.Style(SyntaxType.Default).Background;
                }

                if (bg == Color.Transparent)
                {
                    bg = SystemColors.Window;
                }
            }

            this.TextBox.SelectionBackColor = bg;



            if (style.FontStyle != FontStyle.Regular)
            {
                this.TextBox.SelectionFont = new Font(this.TextBox.SelectionFont, style.FontStyle);
            }
        }

        private void AddSyntaxText(string text, SyntaxType syntaxType)
        {
            this.TextBox.Select(this.TextBox.TextLength, 0);
            this.SetSelectionStyle(syntaxType);
            this.TextBox.AppendText(text);
        }

        private void AddSyntaxText(string text)
        {
            this.AddSyntaxText(text, SyntaxType.Default);
        }

        private void AddLink(Link link)
        {
            this.links.Add(link);
        }

        void SourceViewer_Click(object sender, EventArgs e)
        {
        }

        void SourceViewer_MouseMove(object sender, MouseEventArgs e)
        {
            return;
            int pos = this.TextBox.GetCharIndexFromPosition(new Point(e.X, e.Y));
            bool over = false;

            foreach (Link link in this.links)
            {
                if ((pos >= link.Position) && (pos <= link.Position + link.Length))
                {
                    Trace.Write(link.Text);
                    over = true;
                    break;
                }
            }

            Cursor cursor = over ? Cursors.Hand : Cursors.Default;

            if (this.TextBox.Cursor != cursor)
            {
                this.TextBox.Cursor = cursor;
            }
        }

        private class Highlighter : IOutputFormatter
        {
            private RichTextBox textBox;
            private SourceViewer sourceViewer;
            private int indent;
            private bool indented;
            private int line = 1;
            private int column = 1;

            private bool blankLine = false;

            private SyntaxType currentType;
            private StringBuilder output;

            private const string xmlDocTagPattern = @"
                (?<tag><
	                (?:
		                [^""'>]
	                |
		                (?:
			                ""(?:[^""\\]|\\.)*""
		                |
			                '(?:[^'\\]|\\.)*'
		                )
	                )*
                >)?
                (?<text><[^>]*|[^<]*)?
                ";

            private const string xmlDocAttrPattern = @"
	            (=
		            (?<attr>
			            ""(?:[^""\\]|\\.)*""
		            |
			            '(?:[^'\\]|\\.)*'
		            |
			            [^\s>]+
		            )
	            )?
	            (?<text>[^=]+)?
                ";

            private readonly Regex xmlDocTagRegex = new Regex(xmlDocTagPattern, RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);
            private readonly Regex xmlDocAttrRegex = new Regex(xmlDocAttrPattern, RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

            public int[] HighlightOffsets { get; set; }

            private List<HighlightItem> highlightItems = new List<HighlightItem>();

            private Dictionary<int, int> lineIndents = new Dictionary<int, int>();
            private Dictionary<int, List<SourceCodeMapping>> HighlightMapping;


            public Highlighter(SourceViewer sourceViewer, IEnumerable<HighlightItem> highlightItems)
                : this(sourceViewer)
            {
                this.highlightItems = highlightItems.ToList();

                List<int> offs = new List<int>(this.highlightItems.Count);
                this.HighlightMapping = new Dictionary<int, List<SourceCodeMapping>>(offs.Count);

                foreach (HighlightItem item in this.highlightItems)
                {
                    offs.Add(item.Offset);
                    this.HighlightMapping[item.Offset] = new List<SourceCodeMapping>();
                    this.HighlightOffsets = offs.ToArray();
                }

            }

            public Highlighter(SourceViewer sourceViewer)
            {
                this.sourceViewer = sourceViewer;
                this.textBox = this.sourceViewer.TextBox;
                this.output = new StringBuilder();
                this.currentType = SyntaxType.Default;
            }

            public void Complete()
            {
                if (this.HighlightMapping != null)
                {
                    List<int> lines = new List<int>();

                    foreach (List<SourceCodeMapping> mappingList in this.HighlightMapping.Values)
                    {
                        if (mappingList.Count > 0)
                        {
                            // find the most accurate (shortest) range
                            var x = mappingList.OrderByDescending(m => m.ILInstructionOffset.To - m.ILInstructionOffset.From);
                            var range = x.First();

                            //int start = this.textBox.GetFirstCharIndexFromLine(range.StartLocation.Line - 1) + range.StartLocation.Column - 1;
                            //int end = this.textBox.GetFirstCharIndexFromLine(range.EndLocation.Line - 1) + range.EndLocation.Column - 1;
                            int start = range.StartLocation.Column;
                            int end = range.EndLocation.Column;

                            // select it
                            this.textBox.Select(start, end - start);

                            SyntaxType syntax = this.textBox.SelectedText.StartsWith("throw") ?
                                    SyntaxType.ThrowHighlight :
                                    SyntaxType.CallHighlight;

                            this.sourceViewer.SetSelectionStyle(syntax);

                            lines.Add(range.StartLocation.Line);
                        }
                    }

                    if (lines.Count > 0)
                    {
                        int height = this.textBox.GetLineFromCharIndex(this.textBox.GetCharIndexFromPosition(new Point(0, this.textBox.ClientSize.Height)));
                        int line = Math.Max(0, lines[0] - height / 2 - 2);
                        int n = Math.Max(0, this.textBox.GetFirstCharIndexFromLine(line) - 1);

                        this.textBox.Select(n, 0);
                        this.textBox.ScrollToCaret();
                    }
                    else
                    {
                        this.textBox.Select(0, 0);
                    }

                }
            }

            #region Method highlighting
            private Stack<AstNode> nodes = new Stack<AstNode>();
            private Stack<TextLocation> startLocations = new Stack<TextLocation>();
            private Stack<MemberMapping> memberMapping = new Stack<MemberMapping>();
            private MemberMapping currentMapping;

            private bool CheckNode(AstNode node, HighlightItem highlightItem)
            {
                if (highlightItem.HighlightType.HasFlag(HighlightType.Exception))
                {
                    if (node.Parent is ThrowStatement)
                    {
                    }
                    return node is ThrowStatement;
                }
                else if (highlightItem.HighlightType.HasFlag(HighlightType.Method))
                {

                    switch (highlightItem.MethodType)
                    {
                        case MethodType.Call:
                            return node is InvocationExpression;
                        case MethodType.Get:
                            if (node is MemberReferenceExpression)
                            {
                                MemberReferenceExpression mre = (MemberReferenceExpression)node;
                                return (mre.MemberName == highlightItem.Method.Name.Substring(4));
                            }

                            return false;

                        case MethodType.Set:
                            return node is AssignmentExpression;
                        case MethodType.Ctor:
                            return node is ObjectCreateExpression;
                    }
                }
                return false;
            }

            public void StartNode(AstNode node)
            {
                this.nodes.Push(node);

                if (this.highlightItems.Count > 0)
                {
                    this.startLocations.Push(this.CurrentLocation);

                    MemberMapping mapping = node.Annotation<MemberMapping>();
                    if (mapping != null)
                    {
                        this.memberMapping.Push(this.currentMapping);
                        this.currentMapping = mapping;
                    }
                }
            }

            public void EndNode(AstNode node)
            {
                this.nodes.Pop();

                if (this.highlightItems.Count > 0)
                {
                    TextLocation startLocation = this.startLocations.Pop();

                    if (this.currentMapping != null)
                    {
                        var ranges = node.Annotation<List<ICSharpCode.Decompiler.ILAst.ILRange>>();
                        if ((ranges != null) && (ranges.Count > 0))
                        {
                            foreach (var range in ranges)
                            {
                                SourceCodeMapping scm = null;

                                //foreach (int offset in this.HighlightOffsets)
                                foreach (HighlightItem item in this.highlightItems)
                                {
                                    if ((item.Offset >= range.From) && (item.Offset <= range.To))
                                    {
                                        if (this.CheckNode(node, item))
                                        {
                                            if (scm == null)
                                            {
                                                int adjust = 0;
                                                if (node is ThrowStatement)
                                                {
                                                    adjust = -(this.indent * tabSize + 2);
                                                }
                                                scm = new SourceCodeMapping()
                                                {
                                                    ILInstructionOffset = range,
                                                    StartLocation = startLocation,
                                                    EndLocation = new TextLocation(this.CurrentLocation.Line, this.CurrentLocation.Column + adjust),
                                                    MemberMapping = this.currentMapping
                                                };
                                            }

                                            this.HighlightMapping[item.Offset].Add(scm);
                                        }
                                    }
                                }

                                this.currentMapping.MemberCodeMappings.Add(scm);
                            }
                        }
                    }

                    if (node.Annotation<MemberMapping>() != null)
                    {
                        this.currentMapping = this.memberMapping.Pop();
                    }
                }
            }

            #endregion

            #region Identifiers

            private MemberReference GetDefinition(AstNode node)
            {
                if (!(node is FieldDeclaration ||
                      node is ConstructorDeclaration ||
                      node is DestructorDeclaration ||
                      node is EventDeclaration ||
                      node is DelegateDeclaration ||
                      node is OperatorDeclaration ||
                      node is MemberDeclaration ||
                      node is TypeDeclaration))
                {
                    return node.Annotation<MemberReference>();
                }

                var fieldDef = node.Parent.Annotation<FieldDefinition>();
                if (fieldDef != null)
                {
                    return node.Parent.Annotation<MemberReference>();
                }

                return null;
            }

            private MemberReference GetMemberReference(AstNode node)
            {
                MemberReference memberRef = node.Annotation<MemberReference>();

                if ((memberRef == null) && (node.Role == AstNode.Roles.TargetExpression) &&
                    ((node.Parent is InvocationExpression) || (node.Parent is ObjectCreateExpression)))
                {
                    return node.Parent.Annotation<MemberReference>();
                }

                return memberRef;
            }

            private object GetLocalReference(AstNode node)
            {
                ILVariable variable = node.Annotation<ILVariable>();
                if (variable != null)
                {
                    if (variable.OriginalParameter != null)
                    {
                        return variable.OriginalParameter;
                    }
                    return variable;
                }

                return null;
            }

            private void GotDefinition(string text, MemberReference definition, bool isLocal)
            {
            }

            private void GotReference(string text, object definition, bool isLocal)
            {
                MemberReference memberRef = definition as MemberReference;
                if (memberRef != null)
                {
                }
            }

            #endregion

            #region Output

            #region Text
            private int _currentPos;
            private int CurrentPos
            {
                get
                {
                    int n = this._currentPos;
                    if (!this.indented)
                    {
                        n += this.indent * tabSize;
                    }

                    return n + this.output.Length;
                }
            }

            private TextLocation CurrentLocation
            {
                get
                {
                    return new TextLocation(this.line, this.CurrentPos);
                }
            }

            private void SetStyle(SyntaxType syntaxType)
            {
                if (syntaxType != this.currentType)
                {
                    this.CommitText();
                    this.currentType = syntaxType;
                }
            }

            private void CommitText()
            {
                if (this.output.Length > 0)
                {
                    this.textBox.InvokeIfRequired(() =>
                    {
                        if (!this.indented)
                        {
                            // indent this line
                            string indentString = string.Concat(Enumerable.Repeat("    ", this.indent));
                            this.sourceViewer.AddSyntaxText(indentString);
                            this.indented = true;
                            this.lineIndents[this.CurrentLocation.Line] = indentString.Length;
                        }

                        this.sourceViewer.AddSyntaxText(this.output.ToString(), this.currentType);
                        this.output.Clear();

                        this._currentPos = this.textBox.TextLength;
                    });
                }
            }

            private void Write(dynamic text)
            {
                this.output.Append(text);
            }

            private void WriteLine()
            {
                this.WriteLine("");
            }

            private void WriteLine(dynamic text)
            {
                this.output.AppendLine(text);
                this.CommitText();
                this.indented = false;
                this.line++;
            }

            public void Indent()
            {
                this.indent++;
            }

            public void Unindent()
            {
                this.indent--;
            }

            public void NewLine()
            {
                //if (!this.blankLine)
                {
                    AstNode last = this.nodes.Peek() as AstNode;

                    if (last != null)
                    {
                        //this.Write(string.Format("{0}<", last.GetType().Name));
                        if (last is DoWhileStatement)
                        {
                            this.blankLine = true;
                        }
                    }
                    AstNode next = this.nodes.Peek().NextSibling as AstNode;

                    if (next != null)
                    {
                        if (last != null)
                        {
                            this.blankLine = this.blankLine || (last is UsingDeclaration && !(next is UsingDeclaration));
                        }

                        if (next.NextSibling != null)
                        {
                            this.blankLine = false;
                        }
                        else
                        {
                            this.blankLine = this.blankLine ||
                                             next is ForeachStatement || next is ForStatement || next is IfElseStatement ||
                                             next is WhileStatement || next is DoWhileStatement || next is SwitchStatement ||
                                             next is LockStatement || next is MemberDeclaration;
                        }
                        //this.Write(string.Format(">{0}", next.GetType().Name));
                    }
                }

                this.WriteLine();

                if (this.blankLine)
                {
                    this.WriteLine();
                    this.blankLine = false;
                }
            }

            public void OpenBrace(BraceStyle style)
            {
                this.WriteLine();
                this.WriteLine("{");
                this.Indent();
            }

            public void CloseBrace(BraceStyle style)
            {
                this.Unindent();

                AstNode node = this.nodes.Peek();

                // add a new line after a block, unless there's a remaining 'else', or it's at the end of a block
                this.blankLine = true;

                IfElseStatement ifelse = node.Parent as IfElseStatement;
                if (ifelse != null) //&& (ifelse.TrueStatement == node))
                {
                    // blank line if there's no false block, or this is the false block
                    this.blankLine = (ifelse.FalseStatement == Statement.Null) || (ifelse.FalseStatement == node);
                }

                // no blank line before the 'while' in a do/while
                DoWhileStatement doWhile = node.Parent as DoWhileStatement;
                if (doWhile != null)
                {
                    this.blankLine = false;
                }

                // blank line only after the last block of a try/catch/finally
                TryCatchStatement tryCatch = node.Parent as TryCatchStatement;

                if (tryCatch != null)
                {
                    this.blankLine = false;
                    if (node == tryCatch.FinallyBlock)
                    {
                        // finally is always the last statement
                        this.blankLine = true;
                    }
                    //else if (tryCatch.FinallyBlock.IsNull)
                    //{
                    //    // there's no finally, so the last catch is the last block
                    //    if (node == tryCatch.LastChild)
                    //    {
                    //        this.blankLine = true;
                    //    }
                    //}
                }

                // no blank line if it's the last block (but not a switch case block)
                if ((node.Parent.NextSibling == null) && !((node is SwitchStatement) && (node.NextSibling != null)))
                {
                    this.blankLine = false;
                }

                this.Write("}");
                //this.blankLine = true;
            }

            public void Space()
            {
                this.Write(' ');
            }

            #endregion

            public void WriteComment(CommentType commentType, string content)
            {
                switch (commentType)
                {
                    case CommentType.SingleLine:
                        this.SetStyle(SyntaxType.Comment);
                        this.Write("//");
                        this.WriteLine(content);
                        break;

                    case CommentType.MultiLine:
                        this.SetStyle(SyntaxType.Comment);
                        this.Write("/*");
                        this.Write(content);
                        this.Write("*/");
                        break;

                    case CommentType.Documentation:
                        this.SetStyle(SyntaxType.XmlDocTag);
                        this.Write("///");

                        MatchCollection matches = this.xmlDocTagRegex.Matches(content);


                        List<string> exceptionNames = new List<string>();
                        List<string> highlightExceptionNames = new List<string>();

                        foreach (HighlightItem item in this.highlightItems)
                        {
                            if (item.HighlightType.HasFlag(HighlightType.DocumentedException) && content.Contains("exception"))
                            {
                                string[] names = new string[] {
                                        // T:Namespace.Name
                                        string.Format(CultureInfo.InvariantCulture, "\"T:{0}\"", item.ExceptionType.FullName),
                                        // T:Name
                                        string.Format(CultureInfo.InvariantCulture, "\"T:{0}\"", item.ExceptionType.Name),
                                        // Namespace.Name
                                        string.Format(CultureInfo.InvariantCulture, "\"{0}\"", item.ExceptionType.FullName),
                                        // Name
                                        string.Format(CultureInfo.InvariantCulture, "\"{0}\"", item.ExceptionType.Name),
                                    };

                                exceptionNames.AddRange(names);

                                if (item.Display.HasFlag(HighlightDisplay.Highlight))
                                {
                                    highlightExceptionNames.AddRange(names);
                                }
                            }
                        }

                        foreach (Match match in matches)
                        {
                            // tag content
                            Group tag = match.Groups["tag"];
                            // text after tag
                            Group text = match.Groups["text"];

                            if (tag.Success && tag.Length > 0)
                            {
                                // see if there are any attributes in the tag
                                MatchCollection attrs = null;

                                if (tag.Value.Contains("="))
                                {
                                    attrs = this.xmlDocAttrRegex.Matches(tag.Value);
                                }

                                if ((attrs != null) && (attrs.Count > 0))
                                {
                                    foreach (Match m in attrs)
                                    {
                                        Group attr = m.Groups["attr"];
                                        Group text2 = m.Groups["text"];
                                        if (attr.Success && attr.Length > 0)
                                        {
                                            this.SetStyle(SyntaxType.XmlDocTag);
                                            this.Write("=");

                                            // see if the attribute value is the exception to highlight
                                            if ((exceptionNames != null) && highlightExceptionNames.Contains(attr.Value))
                                            {
                                                this.SetStyle(SyntaxType.ThrowHighlight);
                                            }
                                            else
                                            {
                                                this.SetStyle(SyntaxType.XmlDocAttribute);
                                            }

                                            this.Write(attr.Value);
                                        }

                                        if (text2.Success && text2.Length > 0)
                                        {
                                            this.SetStyle(SyntaxType.XmlDocTag);
                                            this.Write(text2.Value);
                                        }
                                    }
                                }
                                else
                                {
                                    this.SetStyle(SyntaxType.XmlDocTag);
                                    this.Write(tag.Value);
                                }
                            }
                            if (text.Success && text.Length > 0)
                            {
                                this.SetStyle(SyntaxType.XmlDocComment);
                                this.Write(text.Value);
                            }
                        }

                        this.WriteLine();
                        break;

                    default:
                        this.Write(content);
                        break;
                }

                this.SetStyle(SyntaxType.Default);
            }

            public void WriteIdentifier(string identifier)
            {
                AstNode node = this.nodes.Peek();

                MemberReference def = this.GetDefinition(node);
                Link link = null;
                if (def != null)
                {
                    this.GotDefinition(identifier, def, false);
                    link = new Link(this.CurrentPos, identifier.Length, identifier);
                }
                else
                {
                    object memberRef = this.GetMemberReference(node);
                    if (memberRef != null)
                    {
                        this.GotReference(identifier, memberRef, false);
                    }
                    else
                    {
                        memberRef = this.GetLocalReference(node);
                        if (memberRef != null)
                        {
                            this.GotReference(identifier, memberRef, true);
                        }
                    }
                }

                if (link != null)
                {
                    this.sourceViewer.InvokeIfRequired(() =>
                    {
                        this.sourceViewer.AddLink(link);
                    });
                }

                SyntaxType st = SyntaxType.Default;

                if ((node.Role == AstNode.Roles.Type) || (node.Role == AstNode.Roles.TypeArgument))
                {
                    st = SyntaxType.Type;
                }

                this.SetStyle(st);
                this.Write(identifier);
                this.SetStyle(SyntaxType.Default);
            }

            public void WriteKeyword(string keyword)
            {
                this.SetStyle(SyntaxType.Keyword);
                this.Write(keyword);
                this.SetStyle(SyntaxType.Default);
            }

            public void WritePreProcessorDirective(PreProcessorDirectiveType type, string argument)
            {
                this.Write("#");
                this.Write(type.ToString().ToLowerInvariant());
                if (!string.IsNullOrEmpty(argument))
                {
                    this.Write(' ');
                    this.Write(argument);
                }
                this.WriteLine();
            }

            public void WriteToken(string token)
            {
                AstNode node = this.nodes.Peek();

                SyntaxType st = SyntaxType.Default;

                if (node is PrimitiveExpression)
                {
                    // string and char literals
                    PrimitiveExpression pe = (PrimitiveExpression)node;
                    if (pe.Value is string)
                    {
                        st = SyntaxType.String;
                    }
                    else if (pe.Value is char)
                    {
                        st = SyntaxType.Char;
                    }
                }

                //Console.WriteLine("T {0}:\t{1} / {2}", token, nodes.Peek().GetType().Name, nodes.Peek().Role.ToString());// .GetType().Name);
                this.SetStyle(st);
                this.Write(token);
                this.SetStyle(SyntaxType.Default);
            }
            #endregion
        }
    }

}