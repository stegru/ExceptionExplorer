namespace ExceptionExplorer.ExceptionAnalysis
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Linq;
    using System.Threading;
    using System.ComponentModel;
    using ExceptionExplorer.UI;
    using System.Text;
    using System.IO;

    /// <summary>
    /// Contains information about an analysed method.
    /// </summary>
    public class Method : MethodContainer
    {
        /// <summary>Initializes a new instance of the <see cref="Method"/> class.</summary>
        /// <param name="methodBase">The method base.</param>
        protected Method(MethodBase methodBase)
            : base(methodBase)
        {
            this.MethodBase = methodBase;
            try
            {
                this.MethodBody = this.MethodBase.GetMethodBody();
            }
            catch (FileLoadException)
            {
                this.MethodBody = null;
            }

            this.Calls = new Dictionary<Method, List<int>>();
            ExceptionFinder.Instance.AddMethod(this);
        }

        /// <summary>Gets the method class for the MethodBase (or creates a new one).</summary>
        /// <param name="methodBase">The method base.</param>
        /// <returns></returns>
        public static Method GetMethod(MethodBase methodBase)
        {
            if (methodBase == null)
            {
                return null;
            }

            Method method = ExceptionFinder.Instance.TryGetMethod(methodBase);
            return method ?? new Method(methodBase);
        }

        public override string GetDisplayName(IAnalysisObject referer)
        {
            return ((Method)this).MethodBase.GetSignature(this.ShowFullname(referer));
        }

        public override string FullName
        {
            get
            {
                return this.MethodBase.DeclaringType.FullName + "." + this.MethodBase.GetSignature();
            }
        }

        public Dictionary<Method, List<int>> Calls { get; protected set; }

        /// <summary>Gets or sets the method base.</summary>
        /// <value>The method base.</value>
        public MethodBase MethodBase { get; protected set; }

        /// <summary>Gets or sets the method body.</summary>
        /// <value>The method body.</value>
        public MethodBody MethodBody { get; protected set; }

        public override void Reset()
        {
            base.Reset();

            this.Calls.Clear();
        }

        /// <summary>Gets the id for a method.</summary>
        /// <param name="methodBase">The method base.</param>
        /// <returns>A string unique to this method</returns>
        public static int GetId(MemberInfo memberInfo)
        {
            return memberInfo.Module.GetHashCode() ^ memberInfo.MetadataToken;
            //return methodBase.Module.Name + ">" + methodBase.DeclaringType.FullName + ">" + methodBase.ToString();
        }

        /// <summary>Adds the exception.</summary>
        /// <param name="offset">The offset of this method.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="thrownHere">if set to <c>true</c> if the exception was thrown by this method.</param>
        public void AddException(int offset, ThrownException exception)
        {
            // !! This method is the most frequently called non-trivial method !!

            if (this.MethodBody != null)
            {
                // see if any exception handler covers this instruction
                foreach (ExceptionHandlingClause handler in this.MethodBody.ExceptionHandlingClauses)
                {
                    if ((handler.Flags != ExceptionHandlingClauseOptions.Fault) && (handler.Flags != ExceptionHandlingClauseOptions.Finally))
                    {
                        if ((offset >= handler.TryOffset) && (offset < handler.TryOffset + handler.TryLength))
                        {
                            // is it for this exception?
                            if (this.CheckSubclass(handler.CatchType, exception.Exception))
                            {
                                return;
                            }
                        }
                    }
                }
            }
            else
            {
            }

            this.UnhandledExceptions.Add(exception);
        }



        /// <summary>
        /// Checks if subClass is a sub class of, or the same as, baseClass.
        /// Nothing wrong with Type.IsSubClass (or Type.IsAssignableFrom), but this is slightly faster.
        /// </summary>
        /// <param name="baseClass">The base class.</param>
        /// <param name="subClass">The sub class.</param>
        /// <returns>true if subClass derives from baseClass, or is the same.</returns>
        private bool CheckSubclass(Type baseClass, Type subClass)
        {
            Type t = subClass;
            do
            {
                if (t == baseClass)
                {
                    return true;
                }
                t = t.BaseType;
            }
            while (t != null);

            return false;
        }

        /// <summary>Adds some exceptions to this method.</summary>
        /// <param name="offset">The offset.</param>
        /// <param name="exceptions">The exceptions.</param>
        public void AddExceptions(int offset, IEnumerable<ThrownException> exceptions)
        {
            foreach (ThrownException e in exceptions)
            {
                this.AddException(offset, e);
            }
        }

        /// <summary>
        /// Returns the exception handlers that apply to the given offset.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <returns>Exception handlers that apply to the given offset.</returns>
        public IEnumerable<ExceptionHandlingClause> ExceptionHandlersAtOffset(int offset)
        {
            // see if any exception handler covers this instruction
            foreach (ExceptionHandlingClause handler in this.MethodBody.ExceptionHandlingClauses)
            {
                if ((offset >= handler.TryOffset) && (offset < handler.TryOffset + handler.TryLength))
                {
                    if ((handler.Flags != ExceptionHandlingClauseOptions.Fault) && (handler.Flags != ExceptionHandlingClauseOptions.Finally))
                    {
                        yield return handler;
                    }
                }
            }
        }

        /// <summary>Finds the shortest callstack to the provided method.</summary>
        /// <param name="method">The method to search for.</param>
        /// <returns>The shortest call-stack from this method to the specified method.</returns>
        public override CallStack FindStack(Method method)
        {
            if (method == null)
            {
                return null;
            }

            HashSet<Method> visited = new HashSet<Method>();
            CallStack shortest = new CallStack();
            this.FindStack(method, new CallStack(), shortest, visited);
            visited.Clear();

            return shortest;
        }

        /// <summary>Finds the stack.</summary>
        /// <param name="method">The method.</param>
        /// <param name="stack">The stack.</param>
        /// <param name="shortest">The shortest.</param>
        /// <param name="visited">The visited.</param>
        /// <returns></returns>
        private int FindStack(Method method, CallStack stack, CallStack shortest, HashSet<Method> visited)
        {
            stack.Push(this);

            try
            {
                visited.Add(this);

                if ((shortest.Count > 0) && (stack.Count >= shortest.Count))
                {
                    // the shortest stack found is less than the current one - don't search any further
                    return -1;
                }

                if (this == method)
                {
                    // copy the current stack
                    shortest.Clear();
                    foreach (Method m in stack)
                    {
                        shortest.Push(m);
                    }
                    return 0;
                }

                // go through the unhandled exceptions for each called method, and see if the method that the exception
                // matches the one being found.
                int best = -1;
                foreach (Method m in this.CalledMethods)
                {
                    if (visited.Contains(m))
                    {
                        continue;
                    }

                    // see if this method has an uncaught exception that was thrown in the target method
                    var xx = m.UnhandledExceptions.FirstOrDefault(ex => ex.Method == method);
                    var yy = m.UnhandledExceptions.Where(ex => ex.Method == method);

                    bool found = (m == method) ||
                        m.UnhandledExceptions.FirstOrDefault(ex => ex.Method == method) != null;

                    if (found)
                    {
                        int depth = m.FindStack(method, stack, shortest, visited);

                        if (depth == 0)
                        {
                            // it's this one, no need to search the rest
                            return depth + 1;
                        }
                        else if ((depth < best) || (best == -1))
                        {
                            best = depth;
                        }
                    }
                }
                return best;
            }
            finally
            {
                stack.Pop();
            }
        }

        /// <summary>Returns a <see cref="System.String"/> that represents this instance.</summary>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        public override string ToString()
        {
            return this.MethodBase.GetSignature(true);
        }
    }
}