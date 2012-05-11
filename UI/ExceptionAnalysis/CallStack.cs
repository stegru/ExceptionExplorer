// -----------------------------------------------------------------------
// <copyright file="MethodContainer.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

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

    /// <summary>
    /// A call-stack
    /// </summary>
    public class CallStack : Stack<Method>
    {
        /// <summary>Initializes a new instance of the <see cref="CallStack"/> class.</summary>
        public CallStack() : base() { }

        /// <summary>Initializes a new instance of the <see cref="CallStack"/> class.</summary>
        /// <param name="stack">The stack.</param>
        public CallStack(CallStack stack)
            : base(stack)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="CallStack"/> class.</summary>
        /// <param name="collection">The collection.</param>
        public CallStack(IEnumerable<Method> collection)
            : base(collection)
        {
        }

    }

}
