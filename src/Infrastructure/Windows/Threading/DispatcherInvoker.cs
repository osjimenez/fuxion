using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Fuxion.Windows.Threading
{
	public class DispatcherInvoker : IInvoker
	{
		public DispatcherInvoker(Dispatcher? dispatcher = null) =>
			this.dispatcher = dispatcher ?? Dispatcher.CurrentDispatcher;
		Dispatcher dispatcher;
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
				return r == null
					? Task.FromResult(default(TResult)!)
					: Task.FromResult((TResult)r);
			}
			else if (!dispatcher.HasShutdownStarted)
				return dispatcher.InvokeAsync(() =>
				{
					var r = method.DynamicInvoke(args);
					return r == null
						? default
						: (TResult)r;
				}).Task;
		
			return Task.FromResult(default(TResult)!);
		}
	}
}