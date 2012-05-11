// -----------------------------------------------------------------------
// <copyright file="Class.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace ExceptionExplorer.Analysis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;
    using System.Reflection;

    public class Class : MethodContainer
    {
        /// <summary>Gets the ID of the type</summary>
        /// <param name="type">The type.</param>
        /// <returns>The id</returns>
        public static string GetId(Type type)
        {
            return type.Module.FullyQualifiedName + ">" + type.FullName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Class"/> class.
        /// </summary>
        /// <param name="exceptionFinder">The exception finder.</param>
        /// <param name="classType">Type of the class.</param>
        public Class(ExceptionFinder exceptionFinder, Type classType)
        {
            this.ClassType = classType;
            this.ExceptionFinder = exceptionFinder;
        }

        /// <summary>Gets the class's type.</summary>
        /// <value>The type of the class.</value>
        public Type ClassType { get; private set; }

        /// <summary>Gets the exception finder.</summary>
        public ExceptionFinder ExceptionFinder { get; private set; }

        /// <summary>Gets the ID of this instance</summary>
        /// <returns>The id</returns>
        public override string GetId()
        {
            return Class.GetId(this.ClassType);
        }

        /// <summary>Returns a <see cref="System.String"/> that represents this instance.</summary>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        public override string ToString()
        {
            return this.ClassType.FullName;
        }

        /// <summary>Loads the child containers.</summary>
        /// <param name="cancelToken"></param>
        protected override void LoadChildContainers(System.Threading.CancellationToken cancelToken)
        {
            const BindingFlags Binding = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy;

            Class cls = this;
            Type type = cls.ClassType;

            // add the nested classes
            foreach (Type t in type.GetNestedTypes(Binding))
            {
                this.CalledMethods.Add(this.ExceptionFinder.GetClass(t));                
            }

            HashSet<string> done = new HashSet<string>();

            // handle properties first
            PropertyInfo[] properties = type.GetProperties(Binding);

            foreach (PropertyInfo prop in properties)
            {
                if (!(prop.CanRead || prop.CanWrite))
                {
                    continue;
                }

                string name = (prop.DeclaringType == type) ? prop.Name : String.Format("{0}.{1}", prop.DeclaringType, prop.Name);
                this.CalledMethods.Add(new Property(prop));
            }

            // the rest of the methods
            MethodInfo[] methods = type.GetMethods(Binding);
            foreach (MethodInfo method in methods)
            {
                if (method.IsProperty() || method.IsEvent())
                {
                    continue;
                }

                this.CalledMethods.Add(new Method(method));
            }
        }
    }
}
