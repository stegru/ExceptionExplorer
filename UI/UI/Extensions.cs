// -----------------------------------------------------------------------
// <copyright file="Extensions.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace ExceptionExplorer.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;
    using System.ComponentModel;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Invokes the action, via Invoke() if required.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="action">The action.</param>
        public static void InvokeIfRequired(this Control control, Action action)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(action);
            }
            else
            {
                action();
            }
        }

        /// <summary>
        /// Invokes the action, via Invoke() if required.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="action">The action.</param>
        public static T InvokeIfRequired<T>(this Control control, Func<T> action)
        {
            if (control.InvokeRequired)
            {
                return (T)control.Invoke(action);
            }
            else
            {
                return action();
            }
        }

        public static bool IsSet<T>(this T enumeration, T flag) where T : struct,IConvertible
        {
            Enum e = enumeration as Enum;
            if (e == null)
            {
                return false;
            }
            return e.HasFlag(flag as Enum);
        }
    }
}
