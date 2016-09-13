using Fuxion.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fuxion.Threading.Tasks
{
    abstract class TaskManagerEntry : ITaskManagerEntry
    {
        protected TaskManagerEntry(TaskScheduler scheduler, TaskCreationOptions options)
        {
            CancellationTokenSource = new CancellationTokenSource();
            AutoResetEvent = new AutoResetEvent(false);
            TaskScheduler = scheduler ?? TaskScheduler.Default;
            TaskCreationOptions = options;
        }

        private ILog log = LogManager.Create(typeof(TaskManagerEntry));
        Task _Task;
        public Task Task
        {
            get { return _Task; }
            set
            {
                _Task = value;
                _Task.ContinueWith(t =>
                {
                    string toLog = "La tarea finalizó con " + t.Exception.InnerExceptions.Count + " errores.";
                    if (t.Exception.InnerExceptions.Count == 1)
                    {
                        toLog += " Error '" + t.Exception.InnerException.GetType().Name + "': " + t.Exception.InnerException.Message;
                    }
                    log.Error(toLog, t.Exception);
                }, TaskContinuationOptions.OnlyOnFaulted);
            }
        }
        public TaskScheduler TaskScheduler { get; set; }
        public TaskCreationOptions TaskCreationOptions { get; set; }
        public AutoResetEvent AutoResetEvent { get; set; }
        public bool IsSleeping { get; set; }

        public void Start()
        {
            Task.Start(TaskScheduler);
        }

        public event EventHandler Canceled;
        public bool IsCancellationRequested { get { return CancellationTokenSource.IsCancellationRequested; } }
        protected CancellationTokenSource CancellationTokenSource { get; set; }
        public void Cancel()
        {
            CancellationTokenSource.Cancel();
            Canceled?.Invoke(this, EventArgs.Empty);
        }
    }
    class ActionTaskManagerEntry : TaskManagerEntry
    {
        public ActionTaskManagerEntry(Action action, TaskScheduler scheduler, TaskCreationOptions options)
            : base(scheduler, options)
        {
            Task = new Task(action, CancellationTokenSource.Token, TaskCreationOptions);
        }
        public ActionTaskManagerEntry(Action<object> action, object state, TaskScheduler scheduler, TaskCreationOptions options)
            : base(null, default(TaskCreationOptions))
        {
            Task = new Task(action, state, CancellationTokenSource.Token, TaskCreationOptions);
        }
    }
    class FuncTaskManagerEntry<TResult> : TaskManagerEntry
    {
        public FuncTaskManagerEntry(Func<TResult> func, TaskScheduler scheduler, TaskCreationOptions options)
            : base(scheduler, options)
        {
            Task = new Task<TResult>(func, CancellationTokenSource.Token, TaskCreationOptions);
        }
        public FuncTaskManagerEntry(Func<object, TResult> func, object state, TaskScheduler scheduler, TaskCreationOptions options)
            : base(null, default(TaskCreationOptions))
        {
            Task = new Task<TResult>(func, state, CancellationTokenSource.Token, TaskCreationOptions);
        }
    }
}
