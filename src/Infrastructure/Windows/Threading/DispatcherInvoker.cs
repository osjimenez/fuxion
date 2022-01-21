namespace Fuxion.Windows.Threading;

using System.Windows.Threading;

public class DispatcherInvoker : IInvoker
{
	public DispatcherInvoker(Dispatcher? dispatcher = null) =>
		this.dispatcher = dispatcher ?? Dispatcher.CurrentDispatcher;

	private readonly Dispatcher dispatcher;
	public Task InvokeActionDelegate(IInvokable invokable, Delegate method, params object?[] args)
	{
		if (!invokable.UseInvoker || dispatcher == null || dispatcher.CheckAccess())
			return Task.FromResult(method.DynamicInvoke(args));
		else if (!dispatcher.HasShutdownStarted)
			return dispatcher.InvokeAsync(() => method.DynamicInvoke(args)).Task;
		return Task.CompletedTask;
	}
	public Task<TResult> InvokeFuncDelegate<TResult>(IInvokable invokable, Delegate method, params object?[] args)
	{
		if (!invokable.UseInvoker || dispatcher == null || dispatcher.CheckAccess())
		{
			var r = method.DynamicInvoke(args);
			return r is null
				? throw new InvalidOperationException()// Task.FromResult(default(TResult))
				: Task.FromResult((TResult)r);
		}
		else if (!dispatcher.HasShutdownStarted)
			return dispatcher.InvokeAsync(() =>
			{
				var r = method.DynamicInvoke(args);
				return r == null
					? throw new InvalidOperationException()//default
					: (TResult)r;
			}).Task;
		throw new InvalidOperationException();
		//return Task.FromResult(default(TResult));
	}
}