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

    public class Property : MethodContainer
    {
        public Method Get { get; private set; }
        public Method Set { get; private set; }
        public PropertyInfo PropertyInfo { get; private set; }
        private bool _complete;

        public Property(PropertyInfo propertyInfo)
            : base(propertyInfo)
        {
            this.PropertyInfo = propertyInfo;
            this._complete = false;
            this.Get = Method.GetMethod(this.PropertyInfo.GetGetMethod(true));
            this.Set = Method.GetMethod(this.PropertyInfo.GetSetMethod(true));

            if (this.Get != null)
            {
                this.Get.Changed += new EventHandler<ChangedEventArgs>(Method_Changed);
            }

            if (this.Set != null)
            {
                this.Set.Changed += new EventHandler<ChangedEventArgs>(Method_Changed);
            }
        }

        void Method_Changed(object sender, EventArgs e)
        {
            this.OnChanged();
        }

        public override string GetDisplayName(IAnalysisObject referer)
        {
            return this.ShowFullname(referer) ? string.Format("{0}.{1}", this.PropertyInfo.DeclaringType.Name, this.PropertyInfo.Name) : this.PropertyInfo.Name;
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
            Method[] methods = new Method[] {
                this.Get,
                this.Set
            };

            foreach (Method m in methods)
            {
                if (m != null)
                {
                    if (!m.Complete && (cancelToken != null))
                    {
                        ExceptionFinder.Instance.ReadMethod(m, cancelToken);
                    }

                    this.CalledMethods.UnionWith(m.CalledMethods);
                    this.UnhandledExceptions.UnionWith(m.UnhandledExceptions);
                    this.DocumentedThrownExceptions.AddRange(m.DocumentedThrownExceptions);
                }
            }

            this._complete = ((this.Get == null) || this.Get.Complete)
                          && ((this.Set == null) || this.Set.Complete);
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
    }

}
