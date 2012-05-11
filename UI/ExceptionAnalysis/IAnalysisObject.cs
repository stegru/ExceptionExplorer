using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace ExceptionExplorer.ExceptionAnalysis
{
    public interface IAnalysisObject
    {
        /// <summary>Gets a value indicating whether this instance is method container.</summary>
        /// <value>
        /// 	<c>true</c> if this instance is method container; otherwise, <c>false</c>.
        /// </value>
        bool IsMethodContainer { get; }

        /// <summary>Gets the container object.</summary>
        IAnalysisObject Parent { get; }

        /// <summary>Gets the display name.</summary>
        string GetDisplayName(IAnalysisObject referer);

        /// <summary>Gets the full name.</summary>
        string FullName { get; }
    }

    public static class AnalysisObjectExtensions
    {
        public static Assembly GetAssembly(this IAnalysisObject item)
        {
            Assembly asm = null;

            if (item.IsMethodContainer)
            {
                MethodContainer mc = (MethodContainer)item;
                asm = mc.MemberInfo.Module.Assembly;
            }
            else if (item is AssemblyItem)
            {
                asm = ((AssemblyItem)item).Assembly;
            }
            else if (item is Namespace)
            {
                AssemblyItem a = ((Namespace)item).Parent as AssemblyItem;
                if (a != null)
                {
                    asm = a.Assembly;
                }
            }

            return asm;

        }

    }

}
