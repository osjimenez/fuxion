namespace Fuxion.Threading.Tasks;

internal class ActionTaskManagerEntry : TaskManagerEntry
{
	public ActionTaskManagerEntry(Action action, TaskScheduler? scheduler, TaskCreationOptions options, ConcurrencyProfile concurrencyProfile = default, Delegate? @delegate = null) : base(@delegate ?? action, scheduler, options, concurrencyProfile) => Task = new Task(() =>
	{
		DoConcurrency();
		action();
	}, CancellationTokenSource.Token, TaskCreationOptions);
	public ActionTaskManagerEntry(Action<object> action, object state, TaskScheduler? scheduler, TaskCreationOptions options, ConcurrencyProfile concurrencyProfile = default, Delegate? @delegate = null) : base(@delegate ?? action, scheduler, default(TaskCreationOptions), concurrencyProfile)
		=> Task = new Task(st =>
	{
		DoConcurrency();
		action(st ?? throw new ArgumentNullException($"'{nameof(state)}' argument acannot be null"));
	}, state, CancellationTokenSource.Token, TaskCreationOptions);
}