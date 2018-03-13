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
        protected TaskManagerEntry(Delegate @delegate, TaskScheduler scheduler, TaskCreationOptions options, ConcurrencyProfile concurrencyProfile)
        {
            CancellationTokenSource = new CancellationTokenSource();
			Delegate = @delegate;
            TaskScheduler = scheduler ?? TaskScheduler.Default;
            TaskCreationOptions = options;
			ConcurrencyProfile = concurrencyProfile;
        }

        private ILog log = LogManager.Create(typeof(TaskManagerEntry));
        Task _Task;

		public ConcurrencyProfile ConcurrencyProfile { get; set; }
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

		public void DoConcurrency()
		{
			TaskManager.Tasks.Read(l =>
			{
				if (ConcurrencyProfile.CancelPrevious)
				{
					foreach (var task in l.Where(e => e.Delegate == Delegate).ToList().Where(t => t != this && !t.IsCancellationRequested))
						task.Cancel();
				}
			});
		}
        public void Start()
        {
			Task.Start(TaskScheduler);
        }

        public event EventHandler CancelRequested;
        public bool IsCancellationRequested { get { return CancellationTokenSource.IsCancellationRequested; } }
        public CancellationTokenSource CancellationTokenSource { get; set; }

		public Delegate Delegate { get; set; }

		public void Cancel()
        {
            CancellationTokenSource.Cancel();
            CancelRequested?.Invoke(this, EventArgs.Empty);
        }
    }
    class ActionTaskManagerEntry : TaskManagerEntry
    {
        public ActionTaskManagerEntry(Action action, TaskScheduler scheduler, TaskCreationOptions options, ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile), Delegate @delegate = null)
            : base(@delegate ?? action, scheduler, options, concurrencyProfile)
        {
			Task = new Task(() =>
			{
				DoConcurrency();
				action();
			}, CancellationTokenSource.Token, TaskCreationOptions);
        }
        public ActionTaskManagerEntry(Action<object> action, object state, TaskScheduler scheduler, TaskCreationOptions options, ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
            : base(action, scheduler, default(TaskCreationOptions), concurrencyProfile)
        {
            Task = new Task(st=>
			{
				DoConcurrency();
				action(st);
			}, state, CancellationTokenSource.Token, TaskCreationOptions);
        }
	}
    class FuncTaskManagerEntry<TResult> : TaskManagerEntry
    {
        public FuncTaskManagerEntry(Func<TResult> func, TaskScheduler scheduler, TaskCreationOptions options, ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile), Delegate @delegate = null)
            : base(@delegate ?? func, scheduler, options, concurrencyProfile)
        {
            Task = new Task<TResult>(()=>
			{
				DoConcurrency();
				return func();
			}, CancellationTokenSource.Token, TaskCreationOptions);
        }
        public FuncTaskManagerEntry(Func<object, TResult> func, object state, TaskScheduler scheduler, TaskCreationOptions options, ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
            : base(func, scheduler, default(TaskCreationOptions), concurrencyProfile)
        {
			Task = new Task<TResult>(st =>
			{
				DoConcurrency();
				return func(st);
			}, state, CancellationTokenSource.Token, TaskCreationOptions);
        }
	}
}
