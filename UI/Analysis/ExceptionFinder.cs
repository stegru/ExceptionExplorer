namespace ExceptionExplorer.Analysis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Threading;
    using ExceptionExplorer.Config;
    using ExceptionExplorer.IL;

    public class MethodCompleteEventArgs : EventArgs
    {
        public Method Method { get; private set; }

        public MethodCompleteEventArgs(Method method)
        {
            this.Method = method;
        }
    }

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
        private Dictionary<string, Method> visitedMethods;
        private Dictionary<string, Class> classes;

        public AnalysisOptions Settings { get; private set; }

        public event EventHandler<MethodCompleteEventArgs> MethodComplete;
        public event EventHandler<ClassCompleteEventArgs> ClassComplete;

        public delegate IEnumerable<Type> DocumentedExceptionsDelegate(MemberInfo member);
        public DocumentedExceptionsDelegate DocumentedExceptionsCallback;

        static ExceptionFinder()
        {
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += new ResolveEventHandler(CurrentDomain_ReflectionOnlyAssemblyResolve);
        }

        static Assembly CurrentDomain_ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
        {
            return Assembly.ReflectionOnlyLoad(args.Name);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionFinder"/> class.
        /// </summary>
        public ExceptionFinder()
            : this(Options.Current.AnalysisOptions)
        {
        }

        public ExceptionFinder(AnalysisOptions settings)
        {
            this.Settings = settings;

            this.visitedMethods = new Dictionary<string, Method>();
            this.classes = new Dictionary<string, Class>();

            this.Settings.Changed += new EventHandler<SettingChangedEventArgs>(Settings_Changed);
        }

        private void Settings_Changed(object sender, SettingChangedEventArgs e)
        {
            this.ResetAllMethods();
        }

        /// <summary>
        /// Called when a method analysis is completed.
        /// </summary>
        /// <param name="method">The method.</param>
        protected void OnMethodComplete(Method method)
        {
            if (this.MethodComplete != null)
            {
                this.MethodComplete(this, new MethodCompleteEventArgs(method));
            }
        }

        protected void OnClassComplete(Class cls)
        {
            if (this.ClassComplete != null)
            {
                this.ClassComplete(this.ClassComplete, new ClassCompleteEventArgs(cls));
            }
        }

        private IList<ThrownException> GetDocumentedExceptions(Method method)
        {
            // get the property that the method is for
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

        public Class GetClass(Type type)
        {
            string id = Class.GetId(type);
            Class cls;

            lock (this.classes)
            {
                if (!this.classes.TryGetValue(id, out cls))
                {
                    cls = new Class(this, type);

                    try
                    {
                        this.classes.Add(id, cls);
                    }
                    catch (NullReferenceException)
                    {
                        // don't know why this happens
                    }
                }
            }
            return cls;
        }

        public Class ReadClass(Class cls, CancellationToken cancelToken, bool includeNested)
        {
            cls.Analysing = true;

            IEnumerable<Method> methods = this.ReadClass(cls.ClassType, includeNested);

            bool complete = true;

            foreach (Method m in methods)
            {
                if (m.Complete)
                {
                    cls.CalledMethods.Add(m);

                    cls.DocumentedThrownExceptions.AddRange(m.DocumentedThrownExceptions);
                    cls.ThrownExceptions.AddRange(m.ThrownExceptions);
                    cls.UnhandledExceptions.AddRange(m.UnhandledExceptions);
                }
                else
                {
                    complete = false;
                }
            }

            cls.Analysing = false;
            cls.Complete = true;

            this.OnClassComplete(cls);

            return cls;
        }

        private IEnumerable<Method> ReadClass(Type type, bool includeNested)
        {
            const BindingFlags Binding = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy;
            List<Method> methods = new List<Method>();

            try
            {
                foreach (MethodBase mb in type.GetMethods(Binding))
                {

                    if (this.CheckMethod(type, mb))
                    {
                        this.CancellationToken.ThrowIfCancellationRequested();
                        methods.Add(this.ReadMethod(mb, this.CancellationToken));
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

            return methods;
        }

        private object ReadMethodLock = new object();

        /// <summary>
        /// Analyses the method for unhandled exceptions.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns>A <see cref="Method"/> class for the specified method.</returns>
        public Method ReadMethod(MethodBase method, CancellationToken cancelToken)
        {
            if (method == null)
            {
                return null;
            }

            Stack<Type> stack = new Stack<Type>();
            stack.Push(null);

            this.CancellationToken = cancelToken;

            lock (this.ReadMethodLock)
            {
                return this.ReadMethod(method, stack);
            }
        }

        internal void ResetAllMethods()
        {
            this.Cancel();

            foreach (Method mc in this.visitedMethods.Values)
            {
                mc.Reset();
            }
            this.visitedMethods.Clear();
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

        /// <summary>
        /// Analyses the method for unhandled exceptions.
        /// </summary>
        /// <param name="methodBase">The method base.</param>
        /// <param name="locals2">The locals2.</param>
        /// <param name="stack">The stack.</param>
        /// <returns>The analysed method</returns>
        private Method ReadMethod(MethodBase methodBase, Stack<Type> stack)
        {
            Method method = null;
            if (this.GotMethod(methodBase))
            {
                // has it already been analysed?
                method = this.visitedMethods[Method.GetId(methodBase)];
                if (method.Complete)
                {
                    return method;
                }
            }
            else
            {
                method = new Method(methodBase);
                this.AddMethod(method);
            }

            // XML Documentation usage
            if ((this.Settings.XmlDocumentation.Value != XmlDocumentationUsage.Never) && !method.Analysing)
            {
                XmlDocumentationUsage docUsage = this.Settings.XmlDocumentation.Value;

                if (!method.Analysing)
                {
                    IList<ThrownException> documented = this.GetDocumentedExceptions(method);

                    bool continueAnalysis = (docUsage != XmlDocumentationUsage.Only);

                    if (documented.Count > 0)
                    {
                        method.AddExceptions(-1, documented, true);

                        if (docUsage == XmlDocumentationUsage.Prefer)
                        {
                            continueAnalysis = false;
                        }
                    }

                    if (!continueAnalysis)
                    {
                        method.Analysing = false;
                        method.Complete = true;
                        return method;
                    }
                }
            }

            if (method.MethodBody == null)
            {
                return method;
            }

            ILCode code = null;

            try
            {
                method.Analysing = true;

                Type[] locals = new Type[method.MethodBody.LocalVariables.Count];
                int i = 0;
                foreach (LocalVariableInfo lvi in method.MethodBody.LocalVariables)
                {
                    locals[i++] = lvi.LocalType;
                }
                Type[] declaredLocals = locals.ToArray();

                Instruction instruction;
                code = new ILCode(methodBase);

                List<Instruction> instructions = code.GetAllInstructions().ToList();

                for (int instructionNumber = 0; instructionNumber < instructions.Count; instructionNumber++)
                {
                    instruction = instructions[instructionNumber];

                    if (instruction.Value == OpCodeValue.Nop)
                    {
                        continue;
                    }

                    if (this.CancellationToken.IsCancellationRequested)
                    {
                        return method;
                    }

                    switch (instruction.OpCode.OperandType)
                    {
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

                        case OperandType.InlineMethod:

                            HandleMethodCall(stack, method, instruction);
                            break;

                        default:

                            Action<int> storeLocal = (index) =>
                            {
                                Type t = stack.Peek();
                                try
                                {
                                    if ((t != null) && (locals[index] != null) && t.IsClass && !t.IsPointer &&
                                        !(declaredLocals[index].IsAssignableFrom(t) || t.IsAssignableFrom(declaredLocals[index])))
                                    {
                                        method.ToString();
                                    }

                                    stack.Pop();
                                    locals[index] = t;
                                }
                                catch (Exception)
                                {
                                }
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

                                ParameterInfo[] paras = method.MethodBase.GetParameters();
                                if (index < paras.Length)
                                {
                                    stack.Push(paras[index].ParameterType);
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

                                    if (!typeof(Exception).IsAssignableFrom(stack.Peek()))
                                    {
                                        var ops = instructions.Select(f => f.OpCode).ToArray();
                                        new String('x', 1).ToString();
                                    }

                                    ThrownException te = new ThrownException(method, instruction.Offset, stack.Pop());
                                    method.AddException(instruction.Offset, te, true);
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
                                        method.AddException(instruction.Offset, te2, true);
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
            }

            method.Complete = true;
            this.OnMethodComplete(method);
            return method;
        }

        private void HandleMethodCall(Stack<Type> stack, Method method, Instruction instruction)
        {
            MethodBase instructionMethod = instruction.ResolveMethod();

            if ((instruction.Value == OpCodeValue.Call) || (instruction.Value == OpCodeValue.Callvirt) || (instruction.Value == OpCodeValue.Calli))
            {
                bool canCall = CheckMethod(method.MethodBase.DeclaringType, instructionMethod);

                if (canCall)
                {
                    Method calledMethod = null;

                    bool gotMethod = this.GotMethod(instructionMethod);
                    bool needsCalling = true;

                    if (gotMethod)
                    {
                        // already called
                        calledMethod = this.visitedMethods[Method.GetId(instructionMethod)];

                        // has is been analysed completely?
                        needsCalling = !calledMethod.Complete && !calledMethod.Analysing;
                    }

                    if (needsCalling)
                    {
                        calledMethod = this.ReadMethod(instructionMethod, stack);
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

            // put the method return onto the stack
            if (instructionMethod is MethodInfo)
            {
                stack.Push(((MethodInfo)instructionMethod).ReturnParameter.ParameterType);
            }
            else if (instructionMethod is ConstructorInfo)
            {
                stack.Push(((ConstructorInfo)instructionMethod).DeclaringType);
            }
        }

        /// <summary>Checks if the method should be analysed.</summary>
        /// <param name="declaringType">The type that declared this method.</param>
        /// <param name="instructionMethod">The called method.</param>
        /// <returns>true if the method should be analysed.</returns>
        private bool CheckMethod(Type declaringType, MethodBase instructionMethod)
        {
            if (this.Settings.SameClass.Value)
            {
                if (instructionMethod.DeclaringType.FullName != declaringType.FullName)
                {
                    return false;
                }
            }

            if (this.Settings.SameAssembly.Value)
            {
                if (instructionMethod.DeclaringType.Assembly.FullName != declaringType.Assembly.FullName)
                {
                    return false;
                }
            }

            if (this.Settings.IgnoreFramework.Value && (instructionMethod.DeclaringType.Namespace != null))
            {
                string[] framework = new string[] { "System", "Microsoft." };
                foreach (string match in framework)
                {
                    if (instructionMethod.DeclaringType.Namespace.StartsWith(match))
                    {
                        return false;
                    }
                }
            }

            if (this.Settings.IgnoreEventMethods.Value)
            {
                if (instructionMethod.IsEvent())
                {
                    return false;
                }
            }

            return true;
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