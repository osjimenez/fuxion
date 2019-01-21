using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fuxion.Logging;

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
		readonly ILog log = LogManager.Create(typeof(TaskManagerEntry));
		ITaskManagerEntry _Next;
		ITaskManagerEntry _Previous;
		Task _Task;
		public ITaskManagerEntry Previous
		{
			get => _Previous;
			set
			{
				_Previous = value;
				if (value != null)
					((TaskManagerEntry) value)._Next = this;
			}
		}
		public ITaskManagerEntry Next
		{
			get => _Next;
			set
			{
				_Next = value;
				if (value != null)
					((TaskManagerEntry) value)._Previous = this;
			}
		}
		public ConcurrencyProfile ConcurrencyProfile { get; set; }
		public Task Task
		{
			get => _Task;
			set
			{
				_Task = value;
				_Task.ContinueWith(t =>
				{
					var toLog = "La tarea finalizó con " + t.Exception.InnerExceptions.Count + " errores.";
					Exception ex = t.Exception;
					if (t.Exception.InnerExceptions.Count == 1)
					{
						ex = t.Exception.InnerExceptions[0];
						toLog += " Error '" + ex.GetType().Name + "': " + ex.Message;
					}
					if (ex is TaskCanceledByConcurrencyException tccex)
						log.Debug(toLog, tccex);
					else
						log.Error(toLog, ex);
				}, TaskContinuationOptions.OnlyOnFaulted);
			}
		}
		public TaskScheduler TaskScheduler { get; set; }
		public TaskCreationOptions TaskCreationOptions { get; set; }
		public void Start() => Task.Start(TaskScheduler);
		public event EventHandler CancelRequested;
		public bool IsCancellationRequested => CancellationTokenSource.IsCancellationRequested;
		public CancellationTokenSource CancellationTokenSource { get; set; }
		public Delegate Delegate { get; set; }
		public void Cancel()
		{
			CancellationTokenSource.Cancel();
			CancelRequested?.Invoke(this, EventArgs.Empty);
		}
		public void DoConcurrency()
		{
			var allPrevious = TaskManager.Tasks.Read(l => l.Take(l.IndexOf(this)).Where(e => 
				string.IsNullOrWhiteSpace(ConcurrencyProfile.Name)
					//? e.Delegate.Method == Delegate.Method && e.Delegate.Target.GetType() == Delegate.Target.GetType()
					? ConcurrencyProfile.ByInstance
						? e.Delegate.Method == Delegate.Method && e.Delegate.Target == Delegate.Target
						: e.Delegate.Method == Delegate.Method && e.Delegate.Target.GetType() == Delegate.Target.GetType()
					: e.ConcurrencyProfile.Name == ConcurrencyProfile.Name).ToList());
			Previous = allPrevious.LastOrDefault();
			if (ConcurrencyProfile.CancelPrevious)
				foreach (var entry in allPrevious)
					entry.Cancel();
			if (ConcurrencyProfile.Sequentially)
				if (Previous != null)
					try
					{
						Previous.Task.Wait();
					}
					// If task was cancelled, nothing happens
					catch (Exception ex) when (ex is TaskCanceledException || ex is AggregateException aex && aex.Flatten().InnerException is TaskCanceledException)
					{
						Debug.WriteLine("Previous entry was canceled");
					}
			if (ConcurrencyProfile.ExecuteOnlyLast)
				if (Next != null)
					throw new TaskCanceledByConcurrencyException();
		}
	}

	class ActionTaskManagerEntry : TaskManagerEntry
	{
		public ActionTaskManagerEntry(Action action, TaskScheduler scheduler, TaskCreationOptions options, ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile), Delegate @delegate = null) : base(@delegate ?? action, scheduler, options, concurrencyProfile) => Task = new Task(() =>
		{
			DoConcurrency();
			action();
		}, CancellationTokenSource.Token, TaskCreationOptions);
		public ActionTaskManagerEntry(Action<object> action, object state, TaskScheduler scheduler, TaskCreationOptions options, ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile), Delegate @delegate = null) : base(@delegate ?? action, scheduler, default(TaskCreationOptions), concurrencyProfile) => Task = new Task(st =>
		{
			DoConcurrency();
			action(st);
		}, state, CancellationTokenSource.Token, TaskCreationOptions);
	}

	class FuncTaskManagerEntry<TResult> : TaskManagerEntry
	{
		public FuncTaskManagerEntry(Func<TResult> func, TaskScheduler scheduler, TaskCreationOptions options, ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile), Delegate @delegate = null) : base(@delegate ?? func, scheduler, options, concurrencyProfile) => Task = new Task<TResult>(() =>
		{
			DoConcurrency();
			var res = func();
			return res;
		}, CancellationTokenSource.Token, TaskCreationOptions);
		public FuncTaskManagerEntry(Func<object, TResult> func, object state, TaskScheduler scheduler, TaskCreationOptions options, ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile), Delegate @delegate = null) : base(@delegate ?? func, scheduler, default(TaskCreationOptions), concurrencyProfile) => Task = new Task<TResult>(st =>
		{
			DoConcurrency();
			var res = func(st);
			return res;
		}, state, CancellationTokenSource.Token, TaskCreationOptions);
	}

	public class TaskCanceledByConcurrencyException : TaskCanceledException { }
}