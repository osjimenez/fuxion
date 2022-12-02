namespace Fuxion.Threading.Tasks;

class FuncTaskManagerEntry<TResult> : TaskManagerEntry
{
	public FuncTaskManagerEntry(Func<TResult> func, TaskScheduler? scheduler, TaskCreationOptions options, ConcurrencyProfile concurrencyProfile = default, Delegate? @delegate = null) : base(@delegate ?? func, scheduler, options, concurrencyProfile) =>
		Task = new Task<TResult>(() =>
		{
			DoConcurrency();
			var res = func();
			return res;
		}, CancellationTokenSource.Token, TaskCreationOptions);
	public FuncTaskManagerEntry(Func<object, TResult> func, object state, TaskScheduler? scheduler, TaskCreationOptions options, ConcurrencyProfile concurrencyProfile = default, Delegate? @delegate = null) : base(@delegate ?? func, scheduler, default, concurrencyProfile) =>
		Task = new Task<TResult>(st =>
		{
			DoConcurrency();
			var res = func(st ?? throw new ArgumentNullException($"'{nameof(state)}' argument acannot be null"));
			return res;
		}, state, CancellationTokenSource.Token, TaskCreationOptions);
}