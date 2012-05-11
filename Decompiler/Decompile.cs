namespace Decompiler.Extensions
{
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using ICSharpCode.Decompiler;
    using ICSharpCode.Decompiler.Ast;
    using ICSharpCode.Decompiler.Ast.Transforms;
    using ICSharpCode.NRefactory.CSharp;
    using Mono.Cecil;
    using Cil = Mono.Cecil.Cil;
    using ICSharpCode.ILSpy;
    using System;

    /// <summary>
    /// Location of source code.
    /// </summary>
    public struct SourceLocation
    {
        /// <summary>An empty instance.</summary>
        public static SourceLocation Empty = new SourceLocation();

        /// <summary>Gets or sets the end column.</summary>
        /// <value>The end column.</value>
        public int EndColumn { get; set; }

        /// <summary>Gets or sets the end line.</summary>
        /// <value>The end line.</value>
        public int EndLine { get; set; }

        /// <summary>Gets or sets the file.</summary>
        /// <value>The filename.</value>
        public string File { get; set; }

        /// <summary>Gets or sets the start column.</summary>
        /// <value>The start column.</value>
        public int StartColumn { get; set; }

        /// <summary>Gets or sets the start line.</summary>
        /// <value>The start line.</value>
        public int StartLine { get; set; }
    }

    /// <summary>
    /// Extension methods related to decompilation.
    /// </summary>
    public static class Decompile
    {
        /// <summary>
        /// Invokes the action, via Invoke() if required.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="action">The action.</param>
        public static bool InvokeIfRequired(this System.Windows.Forms.Control control, System.Action action)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(action);
                return true;
            }
            else
            {
                action();
                return false;
            }
        }

        /// <summary>
        /// Invokes the action, via Invoke() if required.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="action">The action.</param>
        public static bool InvokeOnlyIfRequired(this System.Windows.Forms.Control control, System.Action action)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(action);
                return true;
            }
            else
            {
                return false;
            }
        }



        /// <summary>Gets the source code file, and line number, of a method.</summary>
        /// <param name="method">The method.</param>
        /// <param name="offset">The offset.</param>
        /// <returns>The method source location.  SourceLocation.Empty if not available.</returns>
        public static SourceLocation GetSourceLocation(this MethodBase method, int offset)
        {
            string file = method.DeclaringType.Assembly.Location;

            ModuleDefinition module = null;
            try
            {
                module = ModuleDefinition.ReadModule(method.Module.FullyQualifiedName, new Mono.Cecil.ReaderParameters() { ReadSymbols = true });
            }
            catch (FileNotFoundException)
            {
                // can't get the pdb (or module)
                return SourceLocation.Empty;
            }

            TypeDefinition td = (TypeDefinition)module.LookupToken(method.DeclaringType.MetadataToken);
            MethodDefinition methodDef = (MethodDefinition)td.Module.LookupToken(method.MetadataToken);

            if (methodDef.HasBody)
            {
                Mono.Cecil.Pdb.PdbReaderProvider readerProvider = new Mono.Cecil.Pdb.PdbReaderProvider();
                Cil.ISymbolReader reader = readerProvider.GetSymbolReader(module, file);

                AssemblyDefinition ad = AssemblyDefinition.ReadAssembly(file, new ReaderParameters { ReadSymbols = true });
                ad.MainModule.ReadSymbols(reader);
                reader.Read(methodDef.Body, os => GetInstruction(methodDef.Body.Instructions, os));

                Cil.Instruction ins = GetInstruction(methodDef.Body.Instructions, offset);

                Cil.SequencePoint sp = ins.SequencePoint;

                // look at the previous ones until it has a location
                while ((sp == null) && (ins != null))
                {
                    ins = ins.Previous;
                    sp = ins.SequencePoint;
                }

                if (sp != null)
                {
                    return new SourceLocation()
                    {
                        File = sp.Document.Url,
                        StartColumn = sp.StartColumn,
                        StartLine = sp.StartLine,
                        EndColumn = sp.EndColumn,
                        EndLine = sp.EndLine
                    };
                }
            }

            return SourceLocation.Empty;
        }

        /// <summary>Decompiles the method.</summary>
        /// <param name="method">The method.</param>
        /// <returns>The method's source code.</returns>
        public static string DecompileMethod(this MethodBase method)
        {
            return method.GetMethodDefinition().DecompileMethod();
        }

        internal static bool DecompileMethod(this MethodBase method, IOutputFormatter outputFormatter, DecompilerSettings settings)
        {
            return Decompile.DecompileMember(method.GetMethodDefinition(), null, outputFormatter, settings);
        }

        internal static bool DecompileProperty(this PropertyInfo property, IOutputFormatter outputFormatter, DecompilerSettings settings)
        {
            return Decompile.DecompileMember(property.GetPropertyDefinition(), null, outputFormatter, settings);
        }

        /// <summary>Decompiles the method.</summary>
        /// <param name="methodDef">The method def.</param>
        /// <returns>The method's source code.</returns>
        internal static string DecompileMethod(this MethodDefinition methodDef)
        {
            StringWriter writer = new StringWriter(new StringBuilder());
            PlainTextOutput output = new PlainTextOutput(writer);
            Decompile.DecompileMember(methodDef, output, null, null);
            return output.ToString();
        }

        /// <summary>Decompiles the method.</summary>
        /// <param name="methodDef">The method def.</param>
        /// <param name="textOutput">The text output.</param>
        /// <returns>true if successful.</returns>
        internal static bool DecompileMember(MemberReference member, ITextOutput textOutput, IOutputFormatter outputFormatter, DecompilerSettings settings)
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ilspy");
            ((DefaultAssemblyResolver)GlobalAssemblyResolver.Instance).AddSearchDirectory(path);

            MethodDefinition methodDef = member as MethodDefinition;
            PropertyDefinition propDef = null;

            if (methodDef == null)
            {
                propDef = member as PropertyDefinition;
                if (propDef == null)
                {
                    return false;
                }
            }

            TypeDefinition declaringType = ((IMemberDefinition)member).DeclaringType;

            try
            {
                MyAstBuilder builder = new MyAstBuilder(
                    new DecompilerContext(declaringType.Module)
                    {
                        CurrentType = declaringType,
                        Settings = settings,
                    });

                IAstTransform additionalTransform = null;
                bool transformFieldInitialisers = true;

                if ((methodDef != null) && transformFieldInitialisers && methodDef.IsConstructor && !methodDef.IsStatic && !declaringType.IsValueType)
                {
                    foreach (FieldDefinition field in methodDef.DeclaringType.Fields)
                    {
                        if (!field.IsStatic)
                        {
                            builder.AddField(field);
                        }
                    }

                    foreach (MethodDefinition md in declaringType.Methods)
                    {
                        if (md.IsConstructor && !md.IsStatic)
                        {
                            builder.AddMethod(md);
                        }
                    }

                    additionalTransform = new SelectCtorTransform(methodDef);
                }
                else
                {
                    if (methodDef != null)
                    {
                        builder.AddMethod(methodDef);
                    }

                    if (propDef != null)
                    {
                        builder.AddProperty(propDef);
                    }
                }

                builder.RunTransformations();
                if (additionalTransform != null)
                {
                    additionalTransform.Run(builder.CompilationUnit);
                }

                if (settings.ShowXmlDocumentation)
                {
                    //textOutput.WriteLine(methodDef.GetXmlDoc());
                    AddXmlDocTransform.Run(builder.CompilationUnit);
                }

                if (textOutput != null)
                {
                    builder.GenerateCode(textOutput);
                }
                else
                {
                    builder.GenerateCode(outputFormatter);
                }

                return true;
            }
            catch (AssemblyResolutionException e)
            {
                string error = string.Format(" Unable to resolve assembly: {0}", e.AssemblyReference.FullName);
                if (textOutput != null)
                {
                    textOutput.WriteLine("//" + error);
                }
                else
                {
                    outputFormatter.WriteComment(CommentType.SingleLine, error);
                }
            }

            return false;
        }

        internal class MyAstBuilder : AstBuilder
        {
            bool transformationsHaveRun;

            public new void RunTransformations()
            {
                this.transformationsHaveRun = true;
                base.RunTransformations();
            }

            public MyAstBuilder(DecompilerContext context)
                : base(context)
            {
            }

            public void GenerateCode(IOutputFormatter outputFormatter)
            {
                if (!transformationsHaveRun)
                    RunTransformations();

                base.CompilationUnit.AcceptVisitor(new InsertParenthesesVisitor { InsertParenthesesForReadability = true }, null);
                //var outputFormatter = new TextOutputFormatter(output);
                var formattingPolicy = new CSharpFormattingOptions();
                // disable whitespace in front of parentheses:
                formattingPolicy.SpaceBeforeMethodCallParentheses = false;
                formattingPolicy.SpaceBeforeMethodDeclarationParentheses = false;
                formattingPolicy.SpaceBeforeConstructorDeclarationParentheses = false;
                formattingPolicy.SpaceBeforeDelegateDeclarationParentheses = false;
                base.CompilationUnit.AcceptVisitor(new CSharpOutputVisitor(outputFormatter, formattingPolicy), null);
            }

        }

        private static ModuleDefinition GetModuleDefinition(this MemberInfo memberInfo)
        {
            try
            {
                return ModuleDefinition.ReadModule(memberInfo.Module.FullyQualifiedName, new Mono.Cecil.ReaderParameters() { ReadSymbols = false });
            }
            catch (FileNotFoundException)
            {
                // can't get the pdb (or module)
                return null;
            }
        }

        /// <summary>Gets the MethodDefinition for a method.</summary>
        /// <param name="method">The method.</param>
        /// <returns>The get method definition.</returns>
        internal static MethodDefinition GetMethodDefinition(this MethodBase method)
        {
            ModuleDefinition module = method.GetModuleDefinition();
            if (method != null)
            {
                TypeDefinition td = (TypeDefinition)module.LookupToken(method.DeclaringType.MetadataToken);
                return (MethodDefinition)td.Module.LookupToken(method.MetadataToken);
            }
            return null;
        }

        internal static PropertyDefinition GetPropertyDefinition(this PropertyInfo propertyInfo)
        {
            ModuleDefinition module = propertyInfo.GetModuleDefinition();
            TypeDefinition td = (TypeDefinition)module.LookupToken(propertyInfo.DeclaringType.MetadataToken);
            return td.Properties.FirstOrDefault(pd => pd.MetadataToken.ToInt32() == propertyInfo.MetadataToken);
        }

        /// <summary>Instruction resolver, used by GetSourceLocation.</summary>
        /// <param name="instructions">The instructions.</param>
        /// <param name="offset">The offset.</param>
        /// <returns>The get instruction.</returns>
        private static Cil.Instruction GetInstruction(Mono.Collections.Generic.Collection<Cil.Instruction> instructions, int offset)
        {
            var size = instructions.Count;
            var items = instructions;
            if (offset < 0 || offset > items[size - 1].Offset)
            {
                return null;
            }

            int min = 0;
            int max = size - 1;
            while (min <= max)
            {
                int mid = min + ((max - min) / 2);
                var instruction = items[mid];
                var instruction_offset = instruction.Offset;

                if (offset == instruction_offset)
                {
                    return instruction;
                }

                if (offset < instruction_offset)
                {
                    max = mid - 1;
                }
                else
                {
                    min = mid + 1;
                }
            }

            return null;
        }
    }
}