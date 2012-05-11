namespace ExceptionExplorer.ExceptionAnalysis
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Threading;
    using ExceptionExplorer.Config;
    using ExceptionExplorer.IL;

    public class ClassCompleteEventArgs : EventArgs
    {
        public Class Class { get; private set; }

        public ClassCompleteEventArgs(Class cls)
        {
            this.Class = cls;
        }
    }

    /// <summary>
    /// Analyses the exceptions that methods can throw.
    /// </summary>
    public class ExceptionFinder : ICancellable
    {
        /// <summary>The visited methods</summary>
        private Dictionary<int, Method> visitedMethods;

        /// <summary>The classes</summary>
        private Dictionary<int, Class> classes;

        /// <summary>Gets the settings.</summary>
        public AnalysisOptions Settings { get; private set; }

        /// <summary>Occurs when the analysis of a class is complete.</summary>
        public event EventHandler<ClassCompleteEventArgs> ClassComplete;

        /// <summary>Gets or sets the synchronize invoke.</summary>
        /// <value>The synchronize invoke.</value>
        internal ISynchronizeInvoke SynchronizeInvoke { get; set; }

        /// <summary>The changed methods.</summary>
        private HashSet<Method> ChangedMethods = new HashSet<Method>();

        /// <summary>
        /// Gets the documented exceptions.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <returns></returns>
        public delegate IEnumerable<Type> DocumentedExceptionsDelegate(MemberInfo member);

        /// <summary>The documented exceptions callback</summary>
        public DocumentedExceptionsDelegate DocumentedExceptionsCallback;

        /// <summary>
        /// Gets a value indicating whether analysis is current being performed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if analysis is being performed; otherwise, <c>false</c>.
        /// </value>
        public bool InAnalysis { get; private set; }

        /// <summary>The instance.</summary>
        private static ExceptionFinder _instance;

        /// <summary>Gets the single instance of this class.</summary>
        public static ExceptionFinder Instance
        {
            get
            {
                return _instance ?? (_instance = new ExceptionFinder());
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionFinder"/> class.
        /// </summary>
        public ExceptionFinder()
            : this(Options.Current.AnalysisOptions)
        {
            AssemblyLoader.Init();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionFinder"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public ExceptionFinder(AnalysisOptions settings)
        {
            this.Settings = settings;

            this.visitedMethods = new Dictionary<int, Method>();
            this.classes = new Dictionary<int, Class>();

            this.Settings.Changed += new EventHandler<SettingChangedEventArgs>(Settings_Changed);
        }

        /// <summary>
        /// Handles the Changed event of the Settings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ExceptionExplorer.Config.SettingChangedEventArgs"/> instance containing the event data.</param>
        private void Settings_Changed(object sender, SettingChangedEventArgs e)
        {
            this.ResetAllMethods();
        }

        /// <summary>
        /// Called when some methods have changed.
        /// </summary>
        protected void OnMethodsChanged()
        {
            if (this.inReadClass && (this.ChangedMethods.Count <= 50))
            {
                return;
            }

            Action action = () =>
            {
                foreach (Method m in this.ChangedMethods)
                {
                    this.CancellationToken.ThrowIfCancellationRequested();
                    m.OnChanged();
                }
                this.ChangedMethods.Clear();
            };

            if ((this.SynchronizeInvoke != null) && this.SynchronizeInvoke.InvokeRequired)
            {
                this.SynchronizeInvoke.Invoke(action, null);
            }
            else
            {
                action();
            }
        }

        /// <summary>Called when class analysis is complete./// </summary>
        /// <param name="cls">The CLS.</param>
        protected void OnClassComplete(Class cls)
        {
            if (this.ClassComplete != null)
            {
                this.ClassComplete(this.ClassComplete, new ClassCompleteEventArgs(cls));
            }
        }

        private IList<ThrownException> GetDocumentedExceptions(Method method)
        {
            // get the property that the method is for (if any)
            MemberInfo member = (MemberInfo)method.MethodBase.GetMethodProperty() ?? method.MethodBase;

            List<ThrownException> list = new List<ThrownException>();
            if (this.DocumentedExceptionsCallback != null)
            {
                IEnumerable<Type> types = this.DocumentedExceptionsCallback(member);
                foreach (Type t in types)
                {
                    list.Add(new ThrownException(method, true, t));
                }
            }

            return list;
        }

        /// <summary>Reads the class.</summary>
        /// <param name="cls">The CLS.</param>
        /// <param name="cancelToken">The cancel token.</param>
        /// <param name="includeNested">if set to <c>true</c> [include nested].</param>
        /// <returns>A Class.</returns>
        public Class ReadClass(Class cls, CancellationToken cancelToken, bool includeNested)
        {
            cls.Analysing = true;
            this.CancellationToken = cancelToken;

            IEnumerable<Method> methods = this.ReadClass(cls.ClassType, includeNested);

            bool complete = true;

            foreach (Method m in methods)
            {
                if (m.Complete)
                {
                    cls.CalledMethods.Add(m);

                    cls.DocumentedThrownExceptions.AddRange(m.DocumentedThrownExceptions);
                    cls.UnhandledExceptions.UnionWith(m.UnhandledExceptions);
                }
                else
                {
                    complete = false;
                }
            }

            cls.Analysing = false;

            cls.Complete = complete;
            if (cls.Complete)
            {
                this.OnClassComplete(cls);
            }

            return cls;
        }

        private bool inReadClass;

        /// <summary>Reads the class.</summary>
        /// <param name="type">The type.</param>
        /// <param name="includeNested">if set to <c>true</c> [include nested].</param>
        /// <returns>The methods in the class.</returns>
        private IEnumerable<Method> ReadClass(Type type, bool includeNested)
        {
            List<Method> methods = new List<Method>();
            const BindingFlags Binding = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy;

            try
            {
                this.inReadClass = true;

                foreach (MethodBase mb in type.GetAllMethods())
                {
                    Method method = Method.GetMethod(mb);
                    if (!method.Ignore)
                    {
                        this.CancellationToken.ThrowIfCancellationRequested();
                        this.ReadMethod(method, this.CancellationToken);
                        methods.Add(method);
                    }
                }

                if (includeNested)
                {
                    foreach (Type t in type.GetNestedTypes(Binding))
                    {
                        this.CancellationToken.ThrowIfCancellationRequested();
                        methods.AddRange(this.ReadClass(t, true));
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                this.inReadClass = false;
            }

            this.OnMethodsChanged();

            return methods;
        }

        private object ReadMethodLock = new object();

        /// <summary>
        /// Analyses the method for unhandled exceptions.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns>A <see cref="Method"/> class for the specified method.</returns>
        public void ReadMethod(MethodBase methodBase, CancellationToken cancelToken)
        {
            this.ReadMethod(Method.GetMethod(methodBase), cancelToken);
        }

        /// <summary>
        /// Analyses the method for unhandled exceptions.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns>A <see cref="Method"/> class for the specified method.</returns>
        public void ReadMethod(Method method, CancellationToken cancelToken)
        {
            if (method == null)
            {
                return;
            }

            Stack<MemberInfo> stack = new Stack<MemberInfo>();
            stack.Push(null);

            this.CancellationToken = cancelToken;

            lock (this.ReadMethodLock)
            {
                try
                {
                    this.InAnalysis = true;
                    this.ReadMethod(method, stack);
                    this.OnMethodsChanged();
                    return;
                }
                finally
                {
                    this.InAnalysis = false;
                }
            }
        }

        /// <summary>Resets all methods.</summary>
        internal void ResetAllMethods()
        {
            this.Cancel();

            foreach (Method mc in this.visitedMethods.Values)
            {
                mc.Reset();
            }

            foreach (Class cls in this.classes.Values)
            {
                cls.Reset();
            }

            this.visitedMethods.Clear();
            this.classes.Clear();
        }

        /// <summary>
        /// Adds the method to the cache of visited methods.
        /// </summary>
        /// <param name="method">The method.</param>
        internal void AddMethod(Method method)
        {
            this.visitedMethods.Add(method.GetId(), method);
        }

        /// <summary>
        /// Determines if the method has already been analysed.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns>true if the method has already been analysed.</returns>
        internal bool GotMethod(Method method)
        {
            return this.GotMethod(method.MethodBase);
        }

        /// <summary>
        /// Determines if the method has already been analysed.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns>true if the method has already been analysed.</returns>
        internal bool GotMethod(MethodBase method)
        {
            return this.visitedMethods.ContainsKey(Method.GetId(method));
        }

        internal Method TryGetMethod(MethodBase methodBase)
        {
            Method m = null;
            if (this.visitedMethods.TryGetValue(Method.GetId(methodBase), out m))
            {
                return m;
            }
            return null;
        }

        internal Class TryGetClass(Type type)
        {
            Class cls = null;
            if (this.classes.TryGetValue(Class.GetId(type), out cls))
            {
                return cls;
            }
            return null;
        }

        internal void AddClass(Class cls)
        {
            this.classes.Add(cls.GetId(), cls);
        }

        /// <summary>Analyses the method for unhandled exceptions.</summary>
        /// <param name="method">The method.</param>
        /// <param name="stack">The stack.</param>
        public void ReadMethod(Method method, Stack<MemberInfo> stack)
        {
            MethodBase methodBase = method.MethodBase;

            if (method.Complete)
            {
                return;
            }

            // XML Documentation usage
            if (((this.Settings.XmlDocumentation.Value != XmlDocumentationUsage.Never) || (method.IsFramework && this.Settings.UseFrameworkDocumented.Value)) && !method.Analysing)
            {
                XmlDocumentationUsage docUsage = this.Settings.XmlDocumentation.Value;

                if (!method.Analysing)
                {
                    IList<ThrownException> documented = this.GetDocumentedExceptions(method);

                    bool continueAnalysis = (docUsage != XmlDocumentationUsage.Only);

                    if (documented.Count > 0)
                    {
                        method.AddExceptions(-1, documented);

                        if (docUsage == XmlDocumentationUsage.Prefer)
                        {
                            continueAnalysis = false;
                        }
                    }

                    if (method.IsFramework && this.Settings.UseFrameworkDocumented)
                    {
                        continueAnalysis = false;
                    }

                    if (!continueAnalysis)
                    {
                        method.Analysing = false;
                        method.Complete = true;
                        return;
                    }
                }
            }

            if (method.MethodBody == null)
            {
                method.Complete = true;
                return;
            }

            ILCode code = null;

            ParameterInfo[] methodParameters = method.MethodBase.GetParameters();

            try
            {
                method.Analysing = true;

                Type[] locals = new Type[method.MethodBody.LocalVariables.Count];
                Type[] declaredLocals = new Type[locals.Length];
                int i = 0;
                foreach (LocalVariableInfo lvi in method.MethodBody.LocalVariables)
                {
                    locals[i] = lvi.LocalType;
                    declaredLocals[i++] = lvi.LocalType;
                }

                Instruction instruction;
                code = new ILCode(methodBase);

                //List<Instruction> instructions = code.GetAllInstructions().ToList();
                Instruction[] instructions = code.GetAllInstructions().ToArray();
                int instructionCount = instructions.Length;
                for (int instructionNumber = 0; instructionNumber < instructionCount; instructionNumber++)
                {
                    instruction = instructions[instructionNumber];

                    this.CancellationToken.ThrowIfCancellationRequested();

                    switch (instruction.OpCode.OperandType)
                    {
                        case OperandType.InlineNone:
                            // most popular answer, catch it here and jump to the default
                            goto default;

                        case OperandType.InlineMethod:
                            HandleMethodCall(stack, method, instruction);
                            break;

                        case OperandType.InlineField:
                            FieldInfo fi = instruction.ResolveField();

                            switch (instruction.Value)
                            {
                                case OpCodeValue.Ldflda:
                                case OpCodeValue.Ldsflda:
                                    stack.Push(fi.FieldType.MakeByRefType());
                                    break;

                                case OpCodeValue.Ldfld:
                                case OpCodeValue.Ldsfld:
                                case OpCodeValue.Stfld:
                                case OpCodeValue.Stsfld:
                                    stack.Push(fi.FieldType);
                                    break;

                                default:
                                    break;
                            }

                            break;

                        case OperandType.ShortInlineBrTarget:

                            // When program flow enters a catch block, the exception is placed on the stack.
                            // The compiler puts a leave statement at the end of every(?) try/catch block.
                            if (instruction.Value == OpCodeValue.Leave_s)
                            {
                                // is the next line a catch?
                                foreach (ExceptionHandlingClause handler in method.MethodBody.ExceptionHandlingClauses)
                                {
                                    if ((handler.HandlerOffset == instructions[instructionNumber + 1].Offset) &&
                                        (handler.Flags != ExceptionHandlingClauseOptions.Finally) && (handler.Flags != ExceptionHandlingClauseOptions.Fault))
                                    {
                                        // put the exception on the stack
                                        stack.Push(handler.CatchType);
                                    }
                                }
                            }

                            break;

                        case OperandType.InlineBrTarget:
                            break;

                        default:

                            Action<int> storeLocal = (index) =>
                            {
                                locals[index] = stack.Pop() as Type;
                            };

                            Action<int> loadArg = (index) =>
                            {
                                if (!method.MethodBase.IsStatic)
                                {
                                    if (index == 0)
                                    {
                                        stack.Push(method.MethodBase.DeclaringType);
                                        return;
                                    }

                                    index--;
                                }

                                if (index < methodParameters.Length)
                                {
                                    stack.Push(methodParameters[index].ParameterType);
                                }
                                else
                                {
                                    stack.Push(null);
                                }
                            };

                            switch (instruction.Value)
                            {
                                case OpCodeValue.Ldarg_0:
                                    loadArg(0);
                                    break;

                                case OpCodeValue.Ldarg_1:
                                    loadArg(1);
                                    break;

                                case OpCodeValue.Ldarg_2:
                                    loadArg(2);
                                    break;

                                case OpCodeValue.Ldarg_3:
                                    loadArg(3);
                                    break;

                                case OpCodeValue.Ldarg_s:
                                case OpCodeValue.Ldarg:
                                case OpCodeValue.Ldarga_s:
                                case OpCodeValue.Ldarga:
                                    {
                                        int idx = Convert.ToInt32(instructions[instructionNumber].Operand);
                                        loadArg(idx);
                                    }

                                    break;

                                case OpCodeValue.Ldloc_0:
                                    stack.Push(locals[0]);
                                    break;

                                case OpCodeValue.Ldloc_1:
                                    stack.Push(locals[1]);
                                    break;

                                case OpCodeValue.Ldloc_2:
                                    stack.Push(locals[2]);
                                    break;

                                case OpCodeValue.Ldloc_3:
                                    stack.Push(locals[3]);
                                    break;

                                case OpCodeValue.Ldloc_s:
                                    {
                                        int idx = Convert.ToInt32(instructions[instructionNumber].Operand);
                                        stack.Push(locals[idx]);
                                    }

                                    break;

                                case OpCodeValue.Stloc_0:
                                    storeLocal(0);
                                    break;

                                case OpCodeValue.Stloc_1:
                                    storeLocal(1);
                                    break;

                                case OpCodeValue.Stloc_2:
                                    storeLocal(2);
                                    break;

                                case OpCodeValue.Stloc_3:
                                    storeLocal(3);
                                    break;

                                case OpCodeValue.Stloc_s:
                                    {
                                        int idx = Convert.ToInt32(instructions[instructionNumber].Operand);
                                        storeLocal(idx);
                                    }

                                    break;

                                case OpCodeValue.Throw:
                                    if (stack.Peek() == null)
                                    {
                                        break;
                                    }

                                    ThrownException te = new ThrownException(method, instruction.Offset, stack.Pop() as Type);
                                    method.AddException(instruction.Offset, te);
                                    break;

                                case OpCodeValue.Rethrow:

                                    // get the exception handler closest to this offset (so we know what's being rethrown)
                                    ExceptionHandlingClause handler = (
                                                                       from h in method.MethodBody.ExceptionHandlingClauses
                                                                       where (instruction.Offset >= h.HandlerOffset) && (instruction.Offset <= h.HandlerOffset + h.HandlerLength) &&
                                                                             (h.Flags != ExceptionHandlingClauseOptions.Finally) && (h.Flags != ExceptionHandlingClauseOptions.Fault)
                                                                       orderby instruction.Offset
                                                                       select h).FirstOrDefault();

                                    // put the exception on the stack
                                    if (handler != null)
                                    {
                                        ThrownException te2 = new ThrownException(method, instruction.Offset, handler.CatchType);
                                        method.AddException(instruction.Offset, te2);
                                    }

                                    break;

                                ////case OpCodeValue.Ret:
                                ////    if (method.MethodBase is MethodInfo)
                                ////    {
                                ////        MethodInfo mi = (MethodInfo)method.MethodBase;
                                ////        if (mi.ReturnType == typeof(void))
                                ////        {
                                ////            ////break;
                                ////        }
                                ////    }

                                ////    goto default;

                                default:
                                    // see how many pops from the stack
                                    switch (instruction.OpCode.StackBehaviourPop)
                                    {
                                        case StackBehaviour.Pop0:
                                            break;

                                        case StackBehaviour.Pop1:
                                        case StackBehaviour.Popi:
                                        case StackBehaviour.Popref:
                                        case StackBehaviour.Varpop:
                                            stack.Pop();
                                            break;

                                        case StackBehaviour.Pop1_pop1:
                                        case StackBehaviour.Popi_pop1:
                                        case StackBehaviour.Popi_popi:
                                        case StackBehaviour.Popi_popi8:
                                        case StackBehaviour.Popi_popr4:
                                        case StackBehaviour.Popi_popr8:
                                        case StackBehaviour.Popref_pop1:
                                        case StackBehaviour.Popref_popi:
                                            stack.Pop();
                                            stack.Pop();
                                            break;

                                        case StackBehaviour.Popref_popi_pop1:
                                        case StackBehaviour.Popref_popi_popi:
                                        case StackBehaviour.Popref_popi_popi8:
                                        case StackBehaviour.Popref_popi_popr4:
                                        case StackBehaviour.Popref_popi_popr8:
                                        case StackBehaviour.Popref_popi_popref:
                                            stack.Pop();
                                            stack.Pop();
                                            stack.Pop();
                                            break;
                                    }

                                    switch (instruction.OpCode.StackBehaviourPush)
                                    {
                                        case StackBehaviour.Push0:
                                            break;

                                        case StackBehaviour.Push1:
                                        case StackBehaviour.Pushi:
                                        case StackBehaviour.Pushi8:
                                        case StackBehaviour.Pushr4:
                                        case StackBehaviour.Pushr8:
                                        case StackBehaviour.Pushref:
                                        case StackBehaviour.Varpush:
                                            stack.Push(null);
                                            break;

                                        case StackBehaviour.Push1_push1:
                                            stack.Push(null);
                                            stack.Push(null);
                                            break;
                                    }

                                    break;
                            }

                            break;
                    }// switch
                }// for
            }
            finally
            {
                if (code != null)
                {
                    code.Dispose();
                }

                method.Analysing = false;

                this.ChangedMethods.Add(method);
                if (this.ChangedMethods.Count > 50)
                {
                    this.OnMethodsChanged();
                }
            }

            method.Complete = true;
        }

        private void HandleMethodCall(Stack<MemberInfo> stack, Method method, Instruction instruction)
        {
            MethodBase instructionMethod = instruction.ResolveMethod();

            if ((instruction.Value == OpCodeValue.Call) || (instruction.Value == OpCodeValue.Callvirt) || (instruction.Value == OpCodeValue.Calli) || (instruction.Value == OpCodeValue.Newobj) || (instruction.Value == OpCodeValue.Ldftn))
            {
                Method calledMethod = Method.GetMethod(instructionMethod);
                bool canCall = calledMethod.ShouldAnalyse(method.MethodBase.DeclaringType);

                if (canCall)
                {
                    if (!calledMethod.Complete && !calledMethod.Analysing)
                    {
                        // ?? the call to this is slow
                        this.ReadMethod(calledMethod, stack);
                    }

                    // add the offset
                    List<int> offsetList;
                    if (!method.Calls.TryGetValue(calledMethod, out offsetList))
                    {
                        offsetList = new List<int>();
                        method.Calls.Add(calledMethod, offsetList);
                    }

                    offsetList.Add(instruction.Offset);

                    // add it to the list of called methods
                    if (!method.CalledMethods.Contains(calledMethod))
                    {
                        method.CalledMethods.Add(calledMethod);
                    }

                    if (calledMethod != method)
                    {
                        if (calledMethod.UnhandledExceptions.Count > 0)
                        {
                            method.AddExceptions(instruction.Offset, calledMethod.UnhandledExceptions);
                        }
                    }
                }
            }
            else
            {
            }

            // put the method return onto the stack
            if (instructionMethod is MethodInfo)
            {
                if (instruction.Value == OpCodeValue.Ldftn)
                {
                    stack.Push(instructionMethod);
                }
                else
                {
                    stack.Push(((MethodInfo)instructionMethod).ReturnParameter.ParameterType);
                }
            }
            else if (instructionMethod is ConstructorInfo)
            {
                stack.Push(((ConstructorInfo)instructionMethod).DeclaringType);
            }
        }

        private bool Cancelled { get; set; }

        public bool IsRunning { get; private set; }

        public void Cancel()
        {
            this.Cancelled = false;
            this.IsRunning = false;
            if (this.CancellationSource != null)
            {
                this.CancellationSource.Cancel();
            }
        }

        public System.Threading.CancellationToken CancellationToken { get; private set; }

        public System.Threading.CancellationTokenSource CancellationSource { get; private set; }
    }
}