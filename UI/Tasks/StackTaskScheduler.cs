// -----------------------------------------------------------------------
// <copyright file="UIWorkers.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace ExceptionExplorer.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Threading;

    public class StackTaskScheduler : TaskScheduler
    {
        private Stack<Task> workers;

        /// <summary>Gets the current task.</summary>
        protected Task Current
        {
            get
            {
                lock (workers)
                {
                    return this.workers.Count == 0 ? null : this.workers.Peek();
                }
            }
        }

        public StackTaskScheduler()
        {
            this.workers = new Stack<Task>();
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            lock (workers)
            {
                return this.workers.Cast<Task>();
            }
        }

        /// <summary>
        /// Queues a <see cref="T:System.Threading.Tasks.Task"/> to the scheduler.
        /// The currently running task is cancelled, and this task is started. What was the current task is then restarted.
        /// </summary>
        /// <param name="task">The <see cref="T:System.Threading.Tasks.Task"/> to be queued.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="task"/> argument is null.</exception>
        protected override void QueueTask(Task task)
        {
            Task current = this.Current;

            if (current != null)
            {
                current.GetState().CancellationSource.Cancel();
            }

            lock (this.workers)
            {
                this.workers.Push(task);
            }

            this.RunTask(current, task, true);
            
        }

        /// <summary>Runs the task.</summary>
        /// <param name="previousTask">The previous task.</param>
        /// <param name="task">The task.</param>
        /// <param name="newThread">if set to <c>true</c>, run it in a new thread.</param>
        private void RunTask(Task previousTask, Task task, bool newThread)
        {
            ThreadStart ts = () =>
            {
                // wait for the previous task to complete.
                if ((previousTask != null) && !previousTask.IsCompleted)
                {
                    try
                    {
                        previousTask.Wait(task.GetState().CancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                    catch (AggregateException)
                    {
                    }

                    if (task.IsCanceled)
                    {
                        return;
                    }
                }

                base.TryExecuteTask(task);

                this.TaskEnded(task);

            };

            if (newThread)
            {
                new Thread(ts).Start();
            }
            else
            {
                ts();
            }
        }

        /// <summary>Called when the task has ended. Starts the next one.</summary>
        /// <param name="task">The task.</param>
        private void TaskEnded(Task task)
        {
            bool run = false;

            lock (this.workers)
            {
                if (this.workers.Count > 0)
                {
                    this.workers.Pop();
                    if (this.workers.Count > 0)
                    {
                        run = true;
                    }
                }
            }

            if (run)
            {
                this.RunTask(task, this.Current, false);
            }
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return this.TryExecuteTask(task);
        }
    }
}
