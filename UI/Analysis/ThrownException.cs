// -----------------------------------------------------------------------
// <copyright file="ThrownException.cs" company="Microsoft">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace ExceptionExplorer.Analysis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using ExceptionExplorer.Analysis;

    /// <summary>
    /// A thrown exception
    /// </summary>
    public class ThrownException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ThrownException"/> class.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="exception">The exception.</param>
        public ThrownException(Method method, int offset, Type exception)
        {
            this.Method = method;
            this.Offset = offset;
            this.Exception = exception;
        }

        /// <summary>Initializes a new instance of the <see cref="ThrownException"/> class.</summary>
        /// <param name="method">The method.</param>
        /// <param name="fromXmlDoc">if set to <c>true</c> [from XML doc].</param>
        /// <param name="exception">The exception.</param>
        public ThrownException(Method method, bool fromXmlDoc, Type exception)
        {
            this.Method = method;
            this.IsXmlDoc = fromXmlDoc;
            this.Exception = exception;
        }

        /// <summary>Gets or sets the exception type.</summary>
        /// <value>The exception.</value>
        public Type Exception { get; protected set; }

        /// <summary>Gets or sets the method it was thrown in.</summary>
        /// <value>The method.</value>
        public Method Method { get; protected set; }

        /// <summary>Gets or sets the offset of the method is was thrown.</summary>
        /// <value>The offset.</value>
        public int Offset { get; protected set; }

        /// <summary>Gets or sets a value indicating whether this exception is from XML documentcation.</summary>
        /// <value><c>true</c> if this exception is from XML documentation; otherwise, <c>false</c>.</value>
        public bool IsXmlDoc { get; protected set; }

        /// <summary>Returns a <see cref="System.String"/> that represents this instance.</summary>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        public override string ToString()
        {
            return this.Exception.FullName;
        }
    }
}
