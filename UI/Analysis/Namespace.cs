// -----------------------------------------------------------------------
// <copyright file="Namespace.cs" company="">
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

    /// <summary>
    /// A namespace in an assembly
    /// </summary>
    public class Namespace : MethodContainer
    {
        private List<Type> types;

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        public Type[] GetTypes()
        {
            return this.types.ToArray();
        }

        public Namespace()
        {
            this.types = new List<Type>();
        }

        public override string GetId()
        {
            return this.Name;
        }

        public override string ToString()
        {
            return this.Name;
        }

        /// <summary>
        /// Gets the namespaces in an assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns></returns>
        public static Namespace[] GetAssemblyNamespaces(Assembly assembly)
        {
            Dictionary<string, Namespace> namespaces = new Dictionary<string, Namespace>();

            foreach (Type type in assembly.GetTypes())
            {
                string name = type.Namespace;
                if (string.IsNullOrEmpty(name))
                {
                    name = "-";
                }

                Namespace ns;
                if (!namespaces.TryGetValue(name, out ns))
                {
                    ns = new Namespace()
                    {
                        Name = name
                    };
                    namespaces.Add(name, ns);
                }

                ns.types.Add(type);

            }

            return namespaces.Values.ToArray();
        }

        protected override void LoadChildContainers(System.Threading.CancellationToken cancelToken)
        {
            foreach (Type type in this.GetTypes().OrderBy(t => t.Name))
            {
                if (!type.IsNested)
                {
                    this.CalledMethods.Add(this.ExceptionFinder.GetClass(type));
                }
            }
        }
    }

    /// <summary>
    /// Extension methods for Namespace
    /// </summary>
    internal static class NamespaceExtensions
    {
        /// <summary>
        /// Gets the namespaces in an assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns></returns>
        public static Namespace[] GetNamespaces(this Assembly assembly)
        {
            return Namespace.GetAssemblyNamespaces(assembly);
        }
    }

}
