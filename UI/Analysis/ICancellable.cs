﻿// -----------------------------------------------------------------------
// <copyright file="ICancellable.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace ExceptionExplorer.Analysis
{
    using System;
    using System.Linq;

    /// <summary>
    /// Implement to be cancellable
    /// </summary>
    public interface ICancellable
    {
        bool IsRunning { get; }
        void Cancel();
    }
}