namespace Fuxion.Threading.Tasks;

class ActionTaskManagerEntry : TaskManagerEntry
{
	public ActionTaskManagerEntry(Action action, TaskScheduler? scheduler, TaskCreationOptions options, ConcurrencyProfile concurrencyProfile = default, Delegate? @delegate = null) : base(
		@delegate ?? action, scheduler, options, concurrencyProfile) =>
		Task = new(() => {
			DoConcurrency();
			action();
		}, CancellationTokenSource.Token, TaskCreationOptions);
	public ActionTaskManagerEntry(Action<object> action,
		object state,
		TaskScheduler? scheduler,
		TaskCreationOptions options,
		ConcurrencyProfile concurrencyProfile = default,
		Delegate? @delegate = null) : base(@delegate ?? action, scheduler, default, concurrencyProfile) =>
		Task = new(st => {
			DoConcurrency();
			action(st ?? throw new ArgumentNullException($"'{nameof(state)}' argument acannot be null"));
		}, state, CancellationTokenSource.Token, TaskCreationOptions);
}