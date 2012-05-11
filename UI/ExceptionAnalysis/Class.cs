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

    public class Class : MethodContainer
    {
        /// <summary>Gets the ID of the type</summary>
        /// <param name="type">The type.</param>
        /// <returns>The id</returns>
        public static int GetId(MemberInfo type)
        {
            return Method.GetId(type);
        }

        public override int GetHashCode()
        {
            return GetId(this.ClassType);
        }

        /// <summary>Prevents a default instance of the <see cref="Class"/> class from being created.</summary>
        private Class()
            : base(null)
        {
            // never called
            throw new NotImplementedException();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Class"/> class.
        /// </summary>
        /// <param name="exceptionFinder">The exception finder.</param>
        /// <param name="classType">Type of the class.</param>
        private Class(Type classType)
            : base(classType)
        {
            this.ClassType = classType;
            ExceptionFinder.Instance.AddClass(this);
        }

        public static Class GetClass(Type classType)
        {
            if (classType == null)
            {
                return null;
            }
            return ExceptionFinder.Instance.TryGetClass(classType) ?? new Class(classType);

        }

        /// <summary>
        /// Finds the shortest callstack to the provided method.
        /// </summary>
        /// <param name="method">The method to search for.</param>
        /// <returns>
        /// The shortest call-stack from this method to the specified method.
        /// </returns>
        public override CallStack FindStack(Method method)
        {
            CallStack shortest = null;
            foreach (Method m in this.CalledMethods)
            {
                CallStack cs = m.FindStack(method);
                if ((cs != null) && (cs.Count > 0) && ((shortest == null) || (cs.Count < shortest.Count)))
                {
                    shortest = cs;
                }
            }

            return shortest;
        }

        /// <summary>Gets the class's type.</summary>
        /// <value>The type of the class.</value>
        public Type ClassType { get; private set; }

        /// <summary>Gets the full name.</summary>
        public override string FullName
        {
            get
            {
                return this.ClassType.FullName;
            }
        }

        /// <summary>Returns a <see cref="System.String"/> that represents this instance.</summary>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        public override string ToString()
        {
            return this.FullName;
        }

        public override string GetDisplayName(IAnalysisObject referer)
        {
            return this.ShowFullname(referer) ? this.ClassType.FullName : this.ClassType.Name;
        }
    }
}
