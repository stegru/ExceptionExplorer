// -----------------------------------------------------------------------
// <copyright file="AssemblyMethodContainer.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace ExceptionExplorer.Analysis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Reflection;
    using System.Windows.Forms;

    /// <summary>
    /// Assembly
    /// </summary>
    public class AssemblyMethodContainer : MethodContainer
    {
        /// <summary>Gets or sets the assembly.</summary>
        /// <value>The assembly.</value>
        public Assembly Assembly { get; protected set; }

        /// <summary>Initializes a new instance of the <see cref="AssemblyMethodContainer"/> class.</summary>
        /// <param name="assembly">The assembly.</param>
        public AssemblyMethodContainer(ExceptionFinder exceptionFinder, Assembly assembly)
            : base()
        {
            this.Assembly = assembly;

            this.ExceptionFinder = exceptionFinder;
        }

        /// <summary>Gets the ID of this instance</summary>
        /// <returns>The id</returns>
        public override string GetId()
        {
            return this.Assembly.FullName;
        }

        /// <summary>Returns a <see cref="System.String"/> that represents this instance.</summary>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        public override string ToString()
        {
            return this.Assembly.FullName;
        }

        /// <summary>Loads the child containers.</summary>
        protected override void LoadChildContainers(System.Threading.CancellationToken cancelToken)
        {
            // add the types for each namespace
            foreach (Namespace ns in this.Assembly.GetNamespaces().OrderBy(n => n.Name))
            {
                ns.ExceptionFinder = this.ExceptionFinder;
                this.CalledMethods.Add(ns);
            }
        }
    }
}
