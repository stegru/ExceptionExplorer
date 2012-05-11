// -----------------------------------------------------------------------
// <copyright file="Property.cs" company="">
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
    using System.Threading;

    public class Property : MethodContainer
    {
        public Method Get { get; private set; }
        public Method Set { get; private set; }
        public PropertyInfo PropertyInfo { get; private set; }
        private bool _complete;

        public Property(PropertyInfo propertyInfo)
        {
            this.PropertyInfo = propertyInfo;
            this._complete = false;
        }

        public override bool Complete
        {
            get
            {
                return this._complete &&
                        (((this.Get == null) || this.Get.Complete) &&
                         ((this.Set == null) || this.Set.Complete));
            }
            internal set
            {
                base.Complete = value;
            }
        }

        public void ReadProperty(ExceptionFinder finder, CancellationToken cancelToken)
        {
            MethodInfo[] methods = new MethodInfo[] {
                this.PropertyInfo.GetGetMethod(true),
                this.PropertyInfo.GetSetMethod(true) 
            };

            this.Set = this.Get = null;

            foreach (MethodInfo mi in methods)
            {
                if (mi != null)
                {
                    Method m = finder.ReadMethod(mi, cancelToken);

                    if (m != null)
                    {
                        foreach (MethodContainer mc in this.CalledMethods)
                        {
                            this.CalledMethods.Add(mc);
                        }
                        this.ThrownExceptions.AddRange(m.ThrownExceptions);
                        this.UnhandledExceptions.AddRange(m.UnhandledExceptions);
                        this.DocumentedThrownExceptions.AddRange(m.DocumentedThrownExceptions);
                    }

                    if (mi.Name.StartsWith("get_"))
                    {
                        this.Get = m;
                    }
                    else
                    {
                        this.Set = m;
                    }
                }
                this._complete = true;
            }
        }

        public override string GetId()
        {
            return this.PropertyInfo.Module.FullyQualifiedName + ">" + this.PropertyInfo.Name;
        }

        public override CallStack FindStack(Method method)
        {
            CallStack getStack = null;
            CallStack setStack = null;

            if (this.Get != null)
            {
                getStack = this.Get.FindStack(method);
            }

            if (this.Set != null)
            {
                setStack = this.Set.FindStack(method);
            }
            else
            {
                return getStack;
            }

            if ((getStack == null) || (setStack.Count < getStack.Count))
            {
                return setStack;
            }
            else
            {
                return getStack;
            }
        }

        /// <summary>Returns a <see cref="System.String"/> that represents this instance.</summary>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        public override string ToString()
        {
            return this.PropertyInfo.Name;
        }

        protected override void LoadChildContainers(CancellationToken cancelToken)
        {
            this.ReadProperty(this.ExceptionFinder, cancelToken);
        }
    }
}
