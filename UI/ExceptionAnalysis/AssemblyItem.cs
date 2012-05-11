// -----------------------------------------------------------------------
// <copyright file="AssemblyMethodContainer.cs" company="">
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
    using System.IO;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class AssemblyItem : IAnalysisObject
    {
        private Dictionary<string, Namespace> namespaces = new Dictionary<string, Namespace>();
        private Type[] types;


        /// <summary>Gets the assembly.</summary>
        public Assembly Assembly { get; private set; }

        /// <summary>Gets the full name.</summary>
        public string FullName { get { return this.Assembly.FullName; } }

        /// <summary>Gets a value indicating whether this instance is method container.</summary>
        /// <value>
        /// 	<c>true</c> if this instance is method container; otherwise, <c>false</c>.
        /// </value>
        public bool IsMethodContainer
        {
            get { return false; }
        }

        /// <summary>Gets the namespaces.</summary>
        public IEnumerable<Namespace> Namespaces
        {
            get
            {
                return this.namespaces.Values;
            }
        }


        /// <summary>Gets the container object.</summary>
        public IAnalysisObject Parent
        {
            get { return null; }
        }

        /// <summary>Initializes a new instance of the <see cref="AssemblyItem"/> class.</summary>
        /// <param name="assembly">The assembly.</param>
        public AssemblyItem(Assembly assembly)
        {
            // load the assembly
            this.Assembly = assembly;
            this.types = Type.EmptyTypes;

            try
            {
                this.types = this.Assembly.GetTypes() ?? Type.EmptyTypes;
            }
            catch (IOException)
            {
            }
            catch (ReflectionTypeLoadException)
            {
            }

            this.GetAssemblyNamespaces();
        }

        /// <summary>
        /// Gets the namespaces in the assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns></returns>
        private void GetAssemblyNamespaces()
        {
            // the assembly doesn't actually specify what namespaces are in it, so get all the classes and
            // see what namespaces they are from.
            foreach (Type type in this.types)
            {
                string name = type.Namespace;
                if (string.IsNullOrEmpty(name))
                {
                    name = "-";
                }

                Namespace ns;
                if (!this.namespaces.TryGetValue(name, out ns))
                {
                    ns = new Namespace(this, name);
                    this.namespaces.Add(name, ns);
                }

                ns.AddType(type);
            }

        }

        public string GetDisplayName(IAnalysisObject referer)
        {
            return this.Assembly.FullName;
        }
    }
}
