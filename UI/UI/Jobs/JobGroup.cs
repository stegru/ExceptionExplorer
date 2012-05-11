// -----------------------------------------------------------------------
// <copyright file="JobGroup.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace ExceptionExplorer.UI.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;

    /// <summary>
    /// Type of queueing
    /// </summary>
    public enum JobGroupType
    {

        /// <summary>Queue - one after the other.</summary>
        FIFO = 0,
        /// <summary>Stack - last one added is ran next.</summary>
        Lifo = 1 << 0,
        /// <summary>No queue, current is cancelled on new job.</summary>
        Replace = 1 << 1,
        /// <summary>Stack - last one added is ran instantly (current is cancelled).</summary>
        LifoReplace = Lifo | Replace,

    }

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class JobGroup
    {
        public JobGroupType Type { get; private set; }
        public string Name { get; private set; }

        private static ThreadLocal<JobGroup> _current = new ThreadLocal<JobGroup>();
        
        /// <summary>Gets the job group for the current thread.</summary>
        public static JobGroup Current
        {
            get
            {
                return _current.IsValueCreated ? _current.Value : null;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JobGroup"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="name">The name.</param>
        public JobGroup(JobGroupType type, string name)
        {
            this.Type = type;
            this.Name = name;
        }

        private Thread thread;
        /// <summary>Gets the thread.</summary>
        public Thread Thread { get { return this.thread; } }

        /// <summary>
        /// Gets a value indicating whether the thread is running.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the thread is running; otherwise, <c>false</c>.
        /// </value>
        private bool threadRunning
        {
            get
            {
                return this.thread != null;
            }
        }

        /// <summary>
        /// The jobs. The first is always the current job.
        /// </summary>
        private LinkedList<Job> jobs = new LinkedList<Job>();


        /// <summary>News the job.</summary>
        /// <param name="control">The control.</param>
        /// <param name="work">The work.</param>
        /// <returns></returns>
        public Job NewJob(Control control, Action<CancellationToken> work)
        {
            return this.NewJob(control, work, null);
        }

        /// <summary>News the job.</summary>
        /// <param name="control">The control.</param>
        /// <param name="work">The work.</param>
        /// <param name="callback">The callback.</param>
        /// <returns></returns>
        public Job NewJob(Control control, Action<CancellationToken> work, Action callback)
        {
            return new Job(this, control, work, callback);
        }

        /// <summary>Adds the specified job.</summary>
        /// <param name="job">The job.</param>
        public void Add(Job job)
        {
            lock (this.jobs)
            {
                if (this.Type == JobGroupType.FIFO)
                {
                    this.jobs.AddLast(job);
                }
                else
                {
                    if (this.Type.IsSet(JobGroupType.Replace))
                    {
                        if ((this.jobs.First != null) && this.jobs.First.Value.State == JobStates.Active)
                        {
                            this.jobs.First.Value.Stop();
                        }

                        if (this.Type == JobGroupType.Replace)
                        {
                            this.jobs.Clear();
                        }
                    }

                    this.jobs.AddFirst(job);
                }
            }

            this.StartThread();
        }

        /// <summary>Cancels all jobs.</summary>
        /// <param name="wait">if set to <c>true</c>, wait for the running job to stop.</param>
        public void CancelAll(bool wait)
        {
            LinkedListNode<Job> first = this.jobs.First;

            if (first != null)
            {
                lock (this.jobs)
                {
                    this.jobs.Clear();
                    first.Value.Stop();
                }
                if (wait)
                {
                    first.Value.Wait();
                }
            }
        }

        /// <summary>Get the next job.</summary>
        /// <returns>The next job.</returns>
        private Job GetNext()
        {
            lock (this.jobs)
            {
                LinkedListNode<Job> node = this.jobs.First;

                for (; node != null; node = node.Next)
                {
                    Job j = node.Value;

                    if (j.State == JobStates.Active)
                    {
                        continue;
                    }

                    if (j.State == JobStates.Complete)
                    {
                        LinkedListNode<Job> previous = node.Previous;
                        this.jobs.Remove(node);
                        continue;
                    }

                    return node.Value;
                }
            }

            return null;
        }

        /// <summary>Starts the thread.</summary>
        private void StartThread()
        {
            if (this.threadRunning)
            {
                return;
            }

            this.thread = new Thread(this.ThreadWorker)
            {
                Name = "job: " + this.Name,
                IsBackground = true,
            };

            this.thread.Start(null);
        }

        /// <summary>The thread worker.</summary>
        /// <param name="obj">The obj.</param>
        private void ThreadWorker(object obj)
        {
            try
            {
                _current.Value = this;
                do
                {
                    Job j = this.GetNext();

                    if (j == null)
                    {
                        return;
                    }

                    j.DoWork();

                    if (j.State == JobStates.Complete)
                    {
                        lock (this.jobs)
                        {
                            jobs.Remove(j);
                        }
                    }

                } while (true);
            }
            finally
            {
                this.thread = null;
            }
        }
    }
}
