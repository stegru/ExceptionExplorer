﻿// -----------------------------------------------------------------------
// <copyright file="ThrownException.cs" company="Microsoft">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace ExceptionExplorer.ExceptionAnalysis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using ExceptionExplorer.ExceptionAnalysis;

    public class ThrownExceptionComparer : IEqualityComparer<ThrownException>
    {
        public bool Equals(ThrownException x, ThrownException y)
        {
            return x == y;
        }

        public int GetHashCode(ThrownException obj)
        {
            return obj.GetHashCode();
        }
    }
    

    /// <summary>
    /// A thrown exception
    /// </summary>
    public class ThrownException
    {
        private static List<ThrownException> allExceptions = new List<ThrownException>(1000);

        public static ThrownException GetException(int id)
        {
            return allExceptions[id];
        }

        private int id;

        public override int GetHashCode()
        {
            return this.id;
        }

        /// <summary>Prevents a default instance of the <see cref="ThrownException"/> class from being created.</summary>
        /// <param name="method">The method.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="exception">The exception.</param>
        private ThrownException(Method method, Type exception)
        {
            this.Method = method;
            this.Exception = exception;
            this.id = allExceptions.Count;
            allExceptions.Add(this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThrownException"/> class.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="exception">The exception.</param>
        public ThrownException(Method method, int offset, Type exception)
            :this(method, exception)
        {
            this.Offset = offset;
        }

        /// <summary>Initializes a new instance of the <see cref="ThrownException"/> class.</summary>
        /// <param name="method">The method.</param>
        /// <param name="fromXmlDoc">if set to <c>true</c> [from XML doc].</param>
        /// <param name="exception">The exception.</param>
        public ThrownException(Method method, bool fromXmlDoc, Type exception)
            :this(method, exception)
        {
            this.IsXmlDoc = fromXmlDoc;
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
