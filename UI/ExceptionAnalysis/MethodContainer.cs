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
    using ExceptionExplorer.Config;

    public class ChangedEventArgs : EventArgs
    {
        /// <summary>Gets a value indicating whether the item has been removed.</summary>
        /// <value>
        /// 	<c>true</c> if the item has been removed; otherwise, <c>false</c>.
        /// </value>
        public bool HasRemoved { get; private set; }

        /// <summary>Initializes a new instance of the <see cref="ChangedEventArgs"/> class.</summary>
        public ChangedEventArgs()
            : this(false)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ChangedEventArgs"/> class.</summary>
        /// <param name="hasRemoved">if set to <c>true</c> the instance has removed.</param>
        public ChangedEventArgs(bool hasRemoved)
        {
            this.HasRemoved = hasRemoved;
        }
    }

    /// <summary>
    /// Something that can contain methods
    /// </summary>
    public abstract class MethodContainer : IAnalysisObject
    {
        /// <summary>The analysis</summary>
        private MethodAnalysis analysis;

        private MemberInfo memberInfo;
        public MemberInfo MemberInfo { get { return this.memberInfo; } }

        /// <summary>Gets a value indicating whether this instance is method container.</summary>
        /// <value>
        /// 	<c>true</c> if this instance is method container; otherwise, <c>false</c>.
        /// </value>
        public bool IsMethodContainer { get { return true; } }

        /// <summary>Gets the container object.</summary>
        public IAnalysisObject Parent { get; private set; }

        /// <summary>Gets the full name.</summary>
        public virtual string FullName
        {
            get
            {
                return this.memberInfo.DeclaringType.FullName + "." + this.memberInfo.Name;
            }
        }


        /// <summary>Initializes a new instance of the <see cref="MethodContainer"/> class.</summary>
        /// <param name="memberInfo">The member info.</param>
        internal MethodContainer(MemberInfo memberInfo)
        {
            this.CalledMethods = new HashSet<Method>();
            this.UnhandledExceptions = new HashSet<ThrownException>();
            this.DocumentedThrownExceptions = new List<ThrownException>(1000);
            this.memberInfo = memberInfo;
            this.id = this.GetId();
            this.Parent = Class.GetClass(this.memberInfo.DeclaringType);
        }

        private static string[] frameworkPrefixes = new string[] { "System", "Microsoft" };

        internal static bool ShouldAnalyse(MemberInfo memberInfo, Type referencedBy)
        {
            Type declaringType = memberInfo.DeclaringType ?? memberInfo as Type;
            if (declaringType == null)
            {
                // huh? just classes can have no declaring type...
                return false;
            }

            bool wantFramework = ExceptionFinder.Instance.Settings.IncludeFramework.Value;
            bool sameClassOnly = ExceptionFinder.Instance.Settings.SameClass.Value;
            bool sameAssemblyOnly = ExceptionFinder.Instance.Settings.SameAssembly.Value;

            if (!wantFramework || sameAssemblyOnly || sameClassOnly)
            {
                bool isFramework = false;

                if (declaringType.Namespace != null)
                {
                    isFramework = CheckIfFramework(declaringType);
                }

                if (isFramework)
                {
                    if (!wantFramework || ExceptionFinder.Instance.Settings.UseFrameworkDocumented.Value)
                    {
                        return false;
                    }
                }
                else
                {
                    if (sameClassOnly && (declaringType.FullName != referencedBy.FullName))
                    {
                        return false;
                    }

                    if (sameAssemblyOnly && (declaringType.Assembly.FullName != referencedBy.Assembly.FullName))
                    {
                        return false;
                    }
                }
            }

            if (ExceptionFinder.Instance.Settings.IgnoreEventMethods.Value)
            {
                if (memberInfo is MethodBase && ((MethodBase)memberInfo).IsEvent())
                {
                    return false;
                }
            }

            return true;
        }

        private static bool CheckIfFramework(Type declaringType)
        {
            if (string.IsNullOrEmpty(declaringType.Namespace))
            {
                return false;
            }

            foreach (string match in frameworkPrefixes)
            {
                if (declaringType.Namespace.StartsWith(match))
                {
                    return true;
                }
            }

            return false;
        }

        private bool? isFramework;
        public bool IsFramework
        {
            get
            {
                if (this.isFramework == null)
                {
                    if (this.memberInfo.DeclaringType == null)
                    {
                        if (this.memberInfo is Type)
                        {
                            this.isFramework = CheckIfFramework((Type)this.memberInfo);
                        }
                    }
                    else
                    {
                        this.isFramework = CheckIfFramework(this.memberInfo.DeclaringType);
                    }
                }

                return (bool)this.isFramework;
            }
        }


        /// <summary>Checks if the method should be analysed.</summary>
        /// <param name="declaringType">The type that declared this method.</param>
        /// <param name="methodBase">The called method.</param>
        /// <returns>true if the method should be analysed.</returns>
        public virtual bool ShouldAnalyse(Type referencedBy)
        {
            return ShouldAnalyse(this.memberInfo, referencedBy);
        }

        private bool _complete;
        /// <summary>
        /// Gets or sets a value indicating whether the analysis of this <see cref="MethodContainer"/> is complete.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the analysis is complete; otherwise, <c>false</c>.
        /// </value>
        public virtual bool Complete
        {
            get
            {
                return this._complete;
            }
            internal set
            {
                this._complete = value;
                if (this._complete)
                {
                    this.CalledMethods.TrimExcess();
                    this.UnhandledExceptions.TrimExcess();
                    this.DocumentedThrownExceptions.TrimExcess();
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="MethodContainer"/> is currently being analysed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if analysing; otherwise, <c>false</c>.
        /// </value>
        public bool Analysing { get; internal set; }

        /// <summary>
        /// Gets or sets the methods called by this method.
        /// </summary>
        /// <value>
        /// The called methods.
        /// </value>
        public HashSet<Method> CalledMethods { get; protected set; }

        /// <summary>
        /// Gets or sets the unhandled exceptions for this method.
        /// </summary>
        /// <value>
        /// The unhandled exceptions.
        /// </value>
        public HashSet<ThrownException> UnhandledExceptions { get; protected set; }

        public List<ThrownException> DocumentedThrownExceptions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="MethodContainer"/> is ignored.
        /// </summary>
        /// <value>
        ///   <c>true</c> if ignored; otherwise, <c>false</c>.
        /// </value>
        public bool Ignore { get; protected set; }

        /// <summary>
        /// Gets the method analysis.
        /// </summary>
        public MethodAnalysis Analysis
        {
            get
            {
                return this.analysis ?? (this.analysis = new MethodAnalysis(this));
            }
        }

        public event EventHandler<ChangedEventArgs> Changed;

        /// <summary>Called when this instance changes.</summary>
        internal void OnChanged()
        {
            this.OnChanged(false);
        }

        /// <summary>Called when this instance changes.</summary>
        /// <param name="removed">if set to <c>true</c> if the instance is (about to be) removed.</param>
        protected void OnChanged(bool removed)
        {
            if (this.Changed != null)
            {
                this.Changed(this, new ChangedEventArgs(removed));
            }
        }

        public virtual void Reset()
        {
            this.analysis = null;
            this.CalledMethods.Clear();
            this.DocumentedThrownExceptions.Clear();
            this.UnhandledExceptions.Clear();
            this.Complete = false;
            //this.OnChanged();
        }

        private int id;

        /// <summary>Gets the ID of this instance</summary>
        /// <returns>The id</returns>
        public int GetId()
        {
            return Method.GetId(this.memberInfo);
        }

        public override int GetHashCode()
        {
            return this.id;
        }

        /// <summary>Finds the shortest callstack to the provided method.</summary>
        /// <param name="method">The method to search for.</param>
        /// <returns>The shortest call-stack from this method to the specified method.</returns>
        public abstract CallStack FindStack(Method method);

        /// <summary>Returns a <see cref="FrameworkMember.String"/> that represents this instance.</summary>
        /// <returns>A <see cref="FrameworkMember.String"/> that represents this instance.</returns>
        public abstract override string ToString();

        /// <summary>Gets the name, suitable for a treeview.</summary>
        public virtual string Name
        {
            get { return this.ToString(); }
        }

        protected bool ShowFullname(IAnalysisObject referer)
        {
            bool fullname = Options.Current.MemberFullname.Value;

            if (!fullname)
            {
                MethodContainer parent;
                if ((referer != null) && referer.IsMethodContainer)
                {
                    parent = (MethodContainer)referer;
                }
                else
                {
                    parent = this.Parent as MethodContainer;
                }

                if (parent == null)
                {
                    // no parent - guess it's not an inherited member
                    return fullname;
                }

                if (parent is Class)
                {
                    if (Options.Current.InheritedMemberFullname)
                    {
                        if (this.MemberInfo.DeclaringType != parent.MemberInfo)
                        {
                            fullname = true;
                        }
                    }
                }
                else if (this.MemberInfo.DeclaringType != parent.MemberInfo.DeclaringType)
                {
                    if (this.MemberInfo.DeclaringType != parent.MemberInfo)
                    {
                        fullname = true;
                    }
                }
            }

            return fullname;
        }

        public virtual string GetDisplayName(IAnalysisObject referer)
        {
            throw new NotImplementedException();
            //if (this is Method)
            //{
            //    return ((Method)this).MethodBase.GetSignature(this.ShowFullname);
            //}
            //else if (this is Class)
            //{

            //}
            //return Method.GetDisplayName;
        }
    }

}
