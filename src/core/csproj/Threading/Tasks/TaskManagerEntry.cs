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
        protected TaskManagerEntry(Delegate @delegate, TaskScheduler scheduler, TaskCreationOptions options, BurstMode burstMode)
        {
            CancellationTokenSource = new CancellationTokenSource();
			Delegate = @delegate;
            TaskScheduler = scheduler ?? TaskScheduler.Default;
            TaskCreationOptions = options;
			BurstMode = burstMode;
        }

        private ILog log = LogManager.Create(typeof(TaskManagerEntry));
        Task _Task;

		public BurstMode BurstMode { get; set; }
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
			switch (BurstMode)
			{
				case BurstMode.WaitForPrevious:
					break;
				case BurstMode.CancelPrevious:
					foreach (var task in TaskManager.SearchEntriesByDelegate(Delegate).Where(t => t != this && !t.IsCancellationRequested))
						task.Cancel();
					break;
				case BurstMode.Coupled:
				default:
					break;
			}
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
        public ActionTaskManagerEntry(Action action, TaskScheduler scheduler, TaskCreationOptions options, BurstMode burstMode = BurstMode.Coupled, Delegate @delegate = null)
            : base(@delegate ?? action, scheduler, options, burstMode)
        {
			Task = new Task(() =>
			{
				DoConcurrency();
				action();
			}, CancellationTokenSource.Token, TaskCreationOptions);
        }
        public ActionTaskManagerEntry(Action<object> action, object state, TaskScheduler scheduler, TaskCreationOptions options, BurstMode burstMode = BurstMode.Coupled)
            : base(action, scheduler, default(TaskCreationOptions), burstMode)
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
        public FuncTaskManagerEntry(Func<TResult> func, TaskScheduler scheduler, TaskCreationOptions options, BurstMode burstMode = BurstMode.Coupled, Delegate @delegate = null)
            : base(@delegate ?? func, scheduler, options, burstMode)
        {
            Task = new Task<TResult>(()=>
			{
				DoConcurrency();
				return func();
			}, CancellationTokenSource.Token, TaskCreationOptions);
        }
        public FuncTaskManagerEntry(Func<object, TResult> func, object state, TaskScheduler scheduler, TaskCreationOptions options, BurstMode burstMode = BurstMode.Coupled)
            : base(func, scheduler, default(TaskCreationOptions), burstMode)
        {
			Task = new Task<TResult>(st =>
			{
				DoConcurrency();
				return func(st);
			}, state, CancellationTokenSource.Token, TaskCreationOptions);
        }
	}
}
