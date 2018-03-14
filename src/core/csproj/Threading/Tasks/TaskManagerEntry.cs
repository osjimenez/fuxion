using Fuxion.Logging;
using System;
using System.Collections.Concurrent;
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
		ITaskManagerEntry _Previous;
		public ITaskManagerEntry Previous
		{
			get => _Previous;
			set
			{
				_Previous = value;
				if (value != null)
					((TaskManagerEntry)value)._Next = this;
			}
		}
		ITaskManagerEntry _Next;
		public ITaskManagerEntry Next
		{
			get => _Next;
			set
			{
				_Next = value;
				if (value != null)
					((TaskManagerEntry)value)._Previous = this;
			}
		}
		public void DoConcurrency()
		{
			Printer.WriteLine("DoConcurrency");
			var allPrevious = TaskManager.Tasks.Read(l => l.Take(l.IndexOf(this)).Where(e => e.Delegate.Method == Delegate.Method && e.Delegate.Target.GetType() == Delegate.Target.GetType()).ToList());
			Previous = allPrevious.LastOrDefault();
			string GetPreviousIds() => allPrevious.Select(e => e.Task.Id).Aggregate("", (c, a) => c + "," + a, a => a.Trim(','));
			Printer.WriteLine($"I have '{allPrevious.Count}' previous '{GetPreviousIds()}'");
			if (ConcurrencyProfile.CancelPrevious)
			{
				Printer.WriteLine($"Canceling '{allPrevious.Count}' previous '{GetPreviousIds()}'");
				foreach (var entry in allPrevious)
					entry.Cancel();
			}
			if (ConcurrencyProfile.Sequentially)
			{
				Printer.WriteLine("Make sequential");
				if (Previous != null)
				{
					Printer.WriteLine($"Wait for previous entry '{Previous.Task.Id}'");
					try
					{
						Previous.Task.Wait();
					}
					catch (TaskCanceledException)
					{
						Printer.WriteLine("Previous entry was canceled");
					}
					catch (AggregateException ex) when (ex.Flatten().InnerException is TaskCanceledException)
					{
						Printer.WriteLine("Previous entry was canceled");
					}
				}
				else
				{
					Printer.WriteLine("NOT Have previous entry");
				}
			}
			if (ConcurrencyProfile.ExecuteOnlyLast)
			{
				if (Next != null)
				{
					throw new TaskCanceledByConcurrencyException();
				}
			}
		}
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
		public ActionTaskManagerEntry(Action<object> action, object state, TaskScheduler scheduler, TaskCreationOptions options, ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile), Delegate @delegate = null)
			: base(@delegate ?? action, scheduler, default(TaskCreationOptions), concurrencyProfile)
		{
			Task = new Task(st =>
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
			Task = new Task<TResult>(() =>
			{
				DoConcurrency();
				var res = func();
				return res;
			}, CancellationTokenSource.Token, TaskCreationOptions);
		}
		public FuncTaskManagerEntry(Func<object, TResult> func, object state, TaskScheduler scheduler, TaskCreationOptions options, ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile), Delegate @delegate = null)
			: base(@delegate ?? func, scheduler, default(TaskCreationOptions), concurrencyProfile)
		{
			Task = new Task<TResult>(st =>
			{
				DoConcurrency();
				var res = func(st);
				return res;
			}, state, CancellationTokenSource.Token, TaskCreationOptions);
		}
	}
	public class TaskCanceledByConcurrencyException : TaskCanceledException { }
}
