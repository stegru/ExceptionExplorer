namespace ExceptionExplorer.Analysis
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Analyses the results of an ExceptionFinder result.
    /// Common analysis operations are performed here, so the results are cached
    /// </summary>
    public class MethodAnalysis
    {
        /// <summary>All exceptions</summary>
        private IEnumerable<ThrownException> allExceptions;

        /// <summary>The exception dictionary</summary>
        private ThrownExceptionDictionary exceptionDictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodAnalysis"/> class.
        /// </summary>
        /// <param name="method">The method.</param>
        public MethodAnalysis(MethodContainer method)
        {
            this.Method = method;
        }

        /// <summary>Gets all exceptions.</summary>
        public IEnumerable<ThrownException> AllExceptions
        {
            get
            {
                return this.allExceptions ?? (this.allExceptions = this.Method.ThrownExceptions.Concat(this.Method.UnhandledExceptions).Distinct());
            }
        }

        /// <summary>Gets the exception dictionary.</summary>
        public ThrownExceptionDictionary ExceptionDictionary
        {
            get
            {
                return this.exceptionDictionary ?? (this.exceptionDictionary = new ThrownExceptionDictionary(this.AllExceptions));
            }
        }

        /// <summary>Gets or sets the method.</summary>
        /// <value>The method.</value>
        public MethodContainer Method { get; protected set; }

        public CallStack GetCallstack(Method method)
        {
            return this.Method.FindStack(method);
        }

    }
}