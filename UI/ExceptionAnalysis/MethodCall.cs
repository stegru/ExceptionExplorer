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
    /// A method call
    /// </summary>
    public class MethodCall
    {
        /// <summary>Initializes a new instance of the <see cref="MethodCall"/> class.</summary>
        /// <param name="offset">The offset.</param>
        /// <param name="method">The method.</param>
        public MethodCall(int offset, Method method)
        {
            this.Method = method;
            this.Offset = offset;
        }

        /// <summary>Gets or sets the method.</summary>
        /// <value>The method.</value>
        public Method Method { get; protected set; }

        /// <summary>Gets or sets the offset the call occurs.</summary>
        /// <value>The offset.</value>
        public int Offset { get; protected set; }
    }

}
