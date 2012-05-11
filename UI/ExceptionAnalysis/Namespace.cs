// -----------------------------------------------------------------------
// <copyright file="Namespace.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace ExceptionExplorer.ExceptionAnalysis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Reflection;

    /// <summary>
    /// A namespace in an assembly
    /// </summary>
    public class Namespace : IAnalysisObject
    {
        private List<Type> types;

        /// <summary>Gets or sets the name.</summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>Gets the container object.</summary>
        public IAnalysisObject Parent { get; protected set; }

        /// <summary>Gets the full name.</summary>
        public string FullName { get { return this.Name; } }


        /// <summary>Gets the types that this namespace declares.</summary>
        /// <returns>An array of types.</returns>
        public Type[] GetTypes()
        {
            return this.types.ToArray();
        }

        /// <summary>Initializes a new instance of the <see cref="Namespace"/> class.</summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="name">The name of the namespace.</param>
        public Namespace(AssemblyItem assembly, string name)
        {
            this.Parent = assembly;
            this.Name = name;
            this.types = new List<Type>();
        }

        /// <summary>Gets a value indicating whether this instance is method container.</summary>
        /// <value>
        /// 	<c>true</c> if this instance is method container; otherwise, <c>false</c>.
        /// </value>
        public bool IsMethodContainer
        {
            get { return false; }
        }

        /// <summary>Adds a class that this namespace declares.</summary>
        /// <param name="type">The type.</param>
        internal void AddType(Type type)
        {
            this.types.Add(type);
        }


        public string GetDisplayName(IAnalysisObject referer)
        {
            return this.Name;
        }
    }
}
