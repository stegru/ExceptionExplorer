// -----------------------------------------------------------------------
// <copyright file="Job.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace ExceptionExplorer.UI.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;
    using System.Threading;

    public enum JobStates
    {
        None = 0,
        Active,
        Cancelled,
        Cancelling,
        Complete
    }

    /// <summary>
    /// A Job.
    /// </summary>
    public class Job
    {
        public static JobGroup General = new JobGroup(JobGroupType.FIFO, "General");
        public static JobGroup Analysis = new JobGroup(JobGroupType.LifoReplace, "Analysis");
        public static JobGroup ExceptionTree = new JobGroup(JobGroupType.Replace, "ExceptionTree");
        public static JobGroup SourceView = new JobGroup(JobGroupType.Replace, "SourceView");
        public static JobGroup Download = new JobGroup(JobGroupType.FIFO, "Download");

        private Action<CancellationToken> work;

        /// <summary>Gets or sets the callback, that is called when the job is complete.</summary>
        /// <value>The callback.</value>
        public Action Callback { get; set; }

        private CancellationTokenSource cancellationSource;


        private Control control;

        private object stateLock = new object();

        /// <summary>Gets or sets the job state.</summary>
        /// <value>The state.</value>
        public JobStates State { get; protected set; }

        private JobGroup jobGroup;

        /// <summary>
        /// Initializes a new instance of the <see cref="Job"/> class.
        /// </summary>
        /// <param name="jobGroup">The job group.</param>
        /// <param name="control">The control.</param>
        /// <param name="work">The work.</param>
        public Job(JobGroup jobGroup, Control control, Action<CancellationToken> work)
            : this(jobGroup, control, work, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Job"/> class.
        /// </summary>
        /// <param name="jobGroup">The job group.</param>
        /// <param name="control">The control.</param>
        /// <param name="work">The work.</param>
        /// <param name="callback">The callback.</param>
        public Job(JobGroup jobGroup, Control control, Action<CancellationToken> work, Action callback)
            : base()
        {
            this.jobGroup = jobGroup;
            this.State = JobStates.None;
            this.work = work;
            this.Callback = callback;
            this.control = control;
        }

        /// <summary>Executes this instance.</summary>
        /// <returns>This job.</returns>
        public Job Execute()
        {
            return this.Execute(null);
        }

        /// <summary>Executes this instance.</summary>
        /// <param name="param">The param.</param>
        /// <returns>This job.</returns>
        public Job Execute(object param)
        {
            this.jobGroup.Add(this);
            return this;
        }

        /// <summary>Does the work.</summary>
        public void DoWork()
        {
            this.cancellationSource = new CancellationTokenSource();

            lock (this.stateLock)
            {
                this.State = JobStates.Active;
            }

            try
            {
                this.work(this.cancellationSource.Token);
            }
            catch (OperationCanceledException)
            {
            }
            catch (ThreadAbortException)
            {
            }

            lock (this.stateLock)
            {
                if ((this.State == JobStates.Cancelling) || this.cancellationSource.IsCancellationRequested)
                {
                    this.State = JobStates.Cancelled;
                }
                else
                {
                    this.State = JobStates.Complete;
                }

                if (this.waitHandle != null)
                {
                    this.waitHandle.Set();
                }
            }
        }

        /// <summary>Stops this instance.</summary>
        public void Stop()
        {
            lock (this.stateLock)
            {
                this.State |= JobStates.Cancelling;
            }
            this.cancellationSource.Cancel();
        }

        private EventWaitHandle waitHandle;

        /// <summary>Waits for this job to end.</summary>
        public void Wait()
        {
            lock (this.stateLock)
            {
                if ((this.State != JobStates.Active) || (this.State != JobStates.Cancelling) || this.cancellationSource.IsCancellationRequested)
                {
                    return;
                }
                this.waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
            }

            this.waitHandle.WaitOne();
        }
    }
}
