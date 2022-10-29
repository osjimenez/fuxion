using System.Diagnostics;

namespace Fuxion.Threading.Tasks;

abstract class TaskManagerEntry : ITaskManagerEntry
{
	protected TaskManagerEntry(Delegate? @delegate, TaskScheduler? scheduler, TaskCreationOptions options, ConcurrencyProfile concurrencyProfile)
	{
		CancellationTokenSource = new();
		Delegate                = @delegate;
		TaskScheduler           = scheduler ?? TaskScheduler.Default;
		TaskCreationOptions     = options;
		ConcurrencyProfile      = concurrencyProfile;
	}
	ITaskManagerEntry? _Next;
	ITaskManagerEntry? _Previous;
	Task?              _Task;
	public ILogger?    Logger { get; set; }
	public ITaskManagerEntry? Previous
	{
		get => _Previous;
		set
		{
			_Previous = value;
			if (value != null) ((TaskManagerEntry)value)._Next = this;
		}
	}
	public ITaskManagerEntry? Next
	{
		get => _Next;
		set
		{
			_Next = value;
			if (value != null) ((TaskManagerEntry)value)._Previous = this;
		}
	}
	public ConcurrencyProfile ConcurrencyProfile { get; set; }
	public Task Task
	{
		get => _Task ?? throw new InvalidProgramException();
		set
		{
			_Task = value;
			_Task.ContinueWith(t =>
			{
				var        toLog = "La tarea finalizó con " + t.Exception?.InnerExceptions.Count + " errores.";
				Exception? ex    = t.Exception;
				if (t.Exception?.InnerExceptions.Count == 1)
				{
					ex    =  t.Exception.InnerExceptions[0];
					toLog += " Error '" + ex.GetType().Name + "': " + ex.Message;
				}
				if (ex is TaskCanceledByConcurrencyException tccex)
					Logger?.LogDebug(tccex, toLog);
				else
					Logger?.LogError(ex, toLog);
			}, TaskContinuationOptions.OnlyOnFaulted);
		}
	}
	public TaskScheduler           TaskScheduler       { get; }
	public TaskCreationOptions     TaskCreationOptions { get; }
	public void                    Start()             => Task.Start(TaskScheduler);
	public event EventHandler?     CancelRequested;
	public bool                    IsCancellationRequested => CancellationTokenSource.IsCancellationRequested;
	public CancellationTokenSource CancellationTokenSource { get; }
	public Delegate?               Delegate                { get; }
	public void Cancel()
	{
		CancellationTokenSource.Cancel();
		CancelRequested?.Invoke(this, EventArgs.Empty);
	}
	public void DoConcurrency()
	{
		var allPrevious = TaskManager.Tasks.Read(l => l.Take(l.IndexOf(this)).Where(e => string.IsNullOrWhiteSpace(ConcurrencyProfile.Name)
			//? e.Delegate.Method == Delegate.Method && e.Delegate.Target.GetType() == Delegate.Target.GetType()
			? ConcurrencyProfile.ByInstance
				? e.Delegate?.Method == Delegate?.Method && e.Delegate?.Target            == Delegate?.Target
				: e.Delegate?.Method == Delegate?.Method && e.Delegate?.Target?.GetType() == Delegate?.Target?.GetType()
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