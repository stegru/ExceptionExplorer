namespace ExceptionExplorer.Analysis
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Linq;
    using System.Threading;
    using ExceptionExplorer.Analysis;

    /// <summary>
    /// Contains information about an analysed method.
    /// </summary>
    public class Method : MethodContainer
    {
        /// <summary>Initializes a new instance of the <see cref="Method"/> class.</summary>
        /// <param name="methodBase">The method base.</param>
        public Method(MethodBase methodBase)
            : base()
        {
            this.MethodBase = methodBase;
            this.MethodBody = this.MethodBase.GetMethodBody();
            this.Calls = new Dictionary<Method, List<int>>();
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

        /// <summary>Gets the display.</summary>
        /// <param name="methodBase">The method base.</param>
        /// <returns>The display name.</returns>
        public static string GetDisplayName(MethodBase methodBase)
        {
            return methodBase.GetSignature(true);
        }

        /// <summary>Gets the id for a method.</summary>
        /// <param name="methodBase">The method base.</param>
        /// <returns>A string unique to this method</returns>
        public static string GetId(MethodBase methodBase)
        {
            //return methodBase.GetHashCode().ToString();
            return methodBase.Module.Name + ">" + methodBase.DeclaringType.FullName + ">" + methodBase.ToString();
        }

        /// <summary>Adds the exception.</summary>
        /// <param name="offset">The offset of this method.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="thrownHere">if set to <c>true</c> if the exception was thrown by this method.</param>
        public void AddException(int offset, ThrownException exception, bool thrownHere)
        {
            if (!typeof(Exception).IsAssignableFrom(exception.Exception))
            {
            }
            bool got = this.UnhandledExceptions.Contains(exception);

            if (got && !thrownHere)
            {
                return;
            }

            if (this.MethodBody != null)
            {
                // see if any exception handler covers this instruction
                foreach (ExceptionHandlingClause handler in this.ExceptionHandlersAtOffset(offset))
                {
                    // is it for this exception?
                    if (handler.CatchType.IsAssignableFrom(exception.Exception))
                    {
                        return;
                    }
                }
            }
            // not handled; add it to the list(s)
            if (!got)
            {
                this.UnhandledExceptions.Add(exception);
            }

            if (thrownHere)
            {
                this.ThrownExceptions.Add(exception);
            }
        }

        /// <summary>Adds some exceptions to this method.</summary>
        /// <param name="offset">The offset.</param>
        /// <param name="exceptions">The exceptions.</param>
        public void AddExceptions(int offset, IEnumerable<ThrownException> exceptions)
        {
            this.AddExceptions(offset, exceptions, false);
        }

        /// <summary>Adds some exceptions to this method.</summary>
        /// <param name="offset">The offset of this method.</param>
        /// <param name="exceptions">The exceptions.</param>
        /// <param name="thrownHere">if set to <c>true</c> if the exception was thrown by this method.</param>
        public void AddExceptions(int offset, IEnumerable<ThrownException> exceptions, bool thrownHere)
        {
            foreach (ThrownException e in exceptions)
            {
                this.AddException(offset, e, thrownHere);
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
            return -1;
        }

        /// <summary>Gets the id for this method.</summary>
        /// <returns>A string unique to this method</returns>
        public override string GetId()
        {
            return GetId(this.MethodBase);
        }

        /// <summary>Returns a <see cref="System.String"/> that represents this instance.</summary>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        public override string ToString()
        {
            return GetDisplayName(this.MethodBase);
        }

        protected override void LoadChildContainers(CancellationToken cancelToken)
        {
            
        }
    }

}