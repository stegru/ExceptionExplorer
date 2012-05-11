// -----------------------------------------------------------------------
// <copyright file="WorkSchedulers.cs" company="">
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

    public static class TaskExtensions
    {
        public static TaskState GetState(this Task task)
        {
            return task.AsyncState as TaskState;
        }
    }

    public class TaskState
    {
        public Action<CancellationToken> Action { get; private set; }
        public CancellationToken CancellationToken { get; private set; }
        public CancellationTokenSource CancellationSource { get; private set; }

        public TaskState(Action<CancellationToken> action, CancellationTokenSource cancelTokenSource)
        {
            this.Action = action;
            this.CancellationSource = cancelTokenSource;
            this.CancellationToken = this.CancellationSource.Token;
        }
    }

    public static class Work
    {
        public class Starter
        {
            public TaskFactory Factory { get; private set; }
            public TaskScheduler Scheduler { get; private set; }

            public Starter(TaskScheduler scheduler)
            {
                this.Scheduler = scheduler;
                this.Factory = new TaskFactory(this.Scheduler);
            }

            public Task Start(Action<CancellationToken> action)
            {
                CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
                return this.Factory.StartNew(this.StartAction, new TaskState(action, cancelTokenSource), cancelTokenSource.Token);
            }

            private void StartAction(object state)
            {
                TaskState a = state as TaskState;
                a.Action(a.CancellationToken);
            }

        }

        public static Starter UI { get; private set; }


        static Work()
        {
            UI = new Starter(new StackTaskScheduler());
        }

    }
}
