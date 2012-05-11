// -----------------------------------------------------------------------
// <copyright file="IHasSynchronizeInvoke.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace ExceptionExplorer.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.ComponentModel;

    /// <summary>
    /// Something that has an ISynchronizeInvoke member
    /// </summary>
    public interface IHasSynchronizeInvoke
    {
        ISynchronizeInvoke SynchronizeInvoke { get; }
    }
}
