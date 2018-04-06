using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Fuxion.Factories;

namespace Fuxion.Windows.Threading
{
    public interface IDispatchable
    {
		bool UseDispatcher { get; set; }
	}
	public interface IDispatcherProvider
	{
		Dispatcher GetDispatcher();
	}
	public class InstanceDispatcherProvider:IDispatcherProvider
	{
		public InstanceDispatcherProvider(Dispatcher dispatcher) =>
			this.dispatcher = dispatcher;
		Dispatcher dispatcher;
		Dispatcher IDispatcherProvider.GetDispatcher() => dispatcher;
	}
	public static class IDispatchableExtensions
	{
		private static Task InvokeActionDelegate(IDispatchable me, Delegate method, params object[] args)
		{
			Dispatcher dis = null;
			try
			{
				dis = Factory.Get<IDispatcherProvider>().GetDispatcher();
			}
			catch { }

			if(!me.UseDispatcher || dis == null || dis.CheckAccess())
				return Task.FromResult(method.DynamicInvoke(args));
			else if(!dis.HasShutdownStarted)
				return dis.InvokeAsync(() => method.DynamicInvoke(args)).Task;
			return Task.CompletedTask;
		}
		private static Task<TResult> InvokeFuncDelegate<TResult>(IDispatchable me, Delegate method, params object[] args)
		{
			Dispatcher dis = null;
			try
			{
				dis = Factory.Get<IDispatcherProvider>().GetDispatcher();
			}
			catch { }

			if (!me.UseDispatcher || dis == null || dis.CheckAccess())
				return Task.FromResult((TResult)method.DynamicInvoke(args));
			else if (!dis.HasShutdownStarted)
				return dis.InvokeAsync(() => (TResult)method.DynamicInvoke(args)).Task;
			return Task.FromResult(default(TResult));
		}

		public static Task Invoke(this IDispatchable me, Action action) => InvokeActionDelegate(me, action);
		public static Task Invoke<T>(this IDispatchable me, Action<T> action, T param) => InvokeActionDelegate(me, action, param);
		public static Task Invoke<T1, T2>(this IDispatchable me, Action<T1, T2> action, T1 param1, T2 param2) => InvokeActionDelegate(me, action, param1, param2);
		public static Task Invoke<T1, T2, T3>(this IDispatchable me, Action<T1, T2, T3> action, T1 param1, T2 param2, T3 param3) => InvokeActionDelegate(me, action, param1, param2, param3);
		public static Task Invoke<T1, T2, T3, T4>(this IDispatchable me, Action<T1, T2, T3, T4> action, T1 param1, T2 param2, T3 param3, T4 param4) => InvokeActionDelegate(me, action, param1, param2, param3, param4);
		public static Task Invoke<T1, T2, T3, T4, T5>(this IDispatchable me, Action<T1, T2, T3, T4, T5> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5) => InvokeActionDelegate(me, action, param1, param2, param3, param4, param5);
		public static Task Invoke<T1, T2, T3, T4, T5, T6>(this IDispatchable me, Action<T1, T2, T3, T4, T5, T6> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6) => InvokeActionDelegate(me, action, param1, param2, param3, param4, param5, param6);
		public static Task<TResult> Invoke<TResult>(this IDispatchable me, Func<TResult> func) => InvokeFuncDelegate<TResult>(me, func);
		public static Task<TResult> Invoke<T, TResult>(this IDispatchable me, Func<T, TResult> func, T param) => InvokeFuncDelegate<TResult>(me, func, param);
		public static Task<TResult> Invoke<T1, T2, TResult>(this IDispatchable me, Func<T1, T2, TResult> func, T1 param1, T2 param2) => InvokeFuncDelegate<TResult>(me, func, param1, param2);
		public static Task<TResult> Invoke<T1, T2, T3, TResult>(this IDispatchable me, Func<T1, T2, T3, TResult> func, T1 param1, T2 param2, T3 param3) => InvokeFuncDelegate<TResult>(me, func, param1, param2, param3);
		public static Task<TResult> Invoke<T1, T2, T3, T4, TResult>(this IDispatchable me, Func<T1, T2, T3, T4, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4) => InvokeFuncDelegate<TResult>(me, func, param1, param2, param3, param4);
		public static Task<TResult> Invoke<T1, T2, T3, T4, T5, TResult>(this IDispatchable me, Func<T1, T2, T3, T4, T5, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5) => InvokeFuncDelegate<TResult>(me, func, param1, param2, param3, param4, param5);
		public static Task<TResult> Invoke<T1, T2, T3, T4, T5, T6, TResult>(this IDispatchable me, Func<T1, T2, T3, T4, T5, T6, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6) => InvokeFuncDelegate<TResult>(me, func, param1, param2, param3, param4, param5, param6);
	}
}
