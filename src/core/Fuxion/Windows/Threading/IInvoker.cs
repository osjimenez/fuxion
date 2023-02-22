namespace Fuxion.Windows.Threading;

public interface IInvokable
{
	bool UseInvoker { get; set; }
	//IInvoker Invoker { get; }
}

[DefaultSingletonInstance(typeof(SynchronousInvoker))]
public interface IInvoker
{
	Task InvokeActionDelegate(IInvokable invokable, Delegate method, params object?[] args);
	Task<TResult> InvokeFuncDelegate<TResult>(IInvokable invokable, Delegate method, params object?[] args);
}

public class SynchronousInvoker : IInvoker
{
	public Task InvokeActionDelegate(IInvokable invokable, Delegate method, params object?[] args) => Task.FromResult(method.DynamicInvoke(args));
	public Task<TResult> InvokeFuncDelegate<TResult>(IInvokable invokable, Delegate method, params object?[] args)
	{
		var res = method.DynamicInvoke(args);
		// NULLABLE - To test it
		if (!typeof(TResult).IsNullable() && res == null)
			throw new InvalidOperationException($"Error in '{nameof(SynchronousInvoker)}', the invocation return null and the return type '{typeof(TResult).GetSignature()}' is not nullable.");
		if (res is not TResult) throw new InvalidOperationException($"Error in '{nameof(SynchronousInvoker)}', the invocation return value cannot be casted to type '{typeof(TResult).GetSignature()}'.");
		return Task.FromResult((TResult)res);
	}
}

public static class IInvokerExtensions
{
	public static Task Invoke(this IInvokable me, Action action) => (Singleton.Get<IInvoker>() ?? new SynchronousInvoker()).InvokeActionDelegate(me, action);
	public static Task Invoke<T>(this IInvokable me, Action<T> action, T param) => (Singleton.Get<IInvoker>() ?? new SynchronousInvoker()).InvokeActionDelegate(me, action, param);
	public static Task Invoke<T1, T2>(this IInvokable me, Action<T1, T2> action, T1 param1, T2 param2) =>
		(Singleton.Get<IInvoker>() ?? new SynchronousInvoker()).InvokeActionDelegate(me, action, param1, param2);
	public static Task Invoke<T1, T2, T3>(this IInvokable me, Action<T1, T2, T3> action, T1 param1, T2 param2, T3 param3) =>
		(Singleton.Get<IInvoker>() ?? new SynchronousInvoker()).InvokeActionDelegate(me, action, param1, param2, param3);
	public static Task Invoke<T1, T2, T3, T4>(this IInvokable me, Action<T1, T2, T3, T4> action, T1 param1, T2 param2, T3 param3, T4 param4) =>
		(Singleton.Get<IInvoker>() ?? new SynchronousInvoker()).InvokeActionDelegate(me, action, param1, param2, param3, param4);
	public static Task Invoke<T1, T2, T3, T4, T5>(this IInvokable me, Action<T1, T2, T3, T4, T5> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5) =>
		(Singleton.Get<IInvoker>() ?? new SynchronousInvoker()).InvokeActionDelegate(me, action, param1, param2, param3, param4, param5);
	public static Task Invoke<T1, T2, T3, T4, T5, T6>(this IInvokable me, Action<T1, T2, T3, T4, T5, T6> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6) =>
		(Singleton.Get<IInvoker>() ?? new SynchronousInvoker()).InvokeActionDelegate(me, action, param1, param2, param3, param4, param5, param6);
	public static Task<TResult> Invoke<TResult>(this IInvokable me, Func<TResult> func) => (Singleton.Get<IInvoker>() ?? new SynchronousInvoker()).InvokeFuncDelegate<TResult>(me, func);
	public static Task<TResult> Invoke<T, TResult>(this IInvokable me, Func<T, TResult> func, T param) =>
		(Singleton.Get<IInvoker>() ?? new SynchronousInvoker()).InvokeFuncDelegate<TResult>(me, func, param);
	public static Task<TResult> Invoke<T1, T2, TResult>(this IInvokable me, Func<T1, T2, TResult> func, T1 param1, T2 param2) =>
		(Singleton.Get<IInvoker>() ?? new SynchronousInvoker()).InvokeFuncDelegate<TResult>(me, func, param1, param2);
	public static Task<TResult> Invoke<T1, T2, T3, TResult>(this IInvokable me, Func<T1, T2, T3, TResult> func, T1 param1, T2 param2, T3 param3) =>
		(Singleton.Get<IInvoker>() ?? new SynchronousInvoker()).InvokeFuncDelegate<TResult>(me, func, param1, param2, param3);
	public static Task<TResult> Invoke<T1, T2, T3, T4, TResult>(this IInvokable me, Func<T1, T2, T3, T4, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4) =>
		(Singleton.Get<IInvoker>() ?? new SynchronousInvoker()).InvokeFuncDelegate<TResult>(me, func, param1, param2, param3, param4);
	public static Task<TResult> Invoke<T1, T2, T3, T4, T5, TResult>(this IInvokable me, Func<T1, T2, T3, T4, T5, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5) =>
		(Singleton.Get<IInvoker>() ?? new SynchronousInvoker()).InvokeFuncDelegate<TResult>(me, func, param1, param2, param3, param4, param5);
	public static Task<TResult>
		Invoke<T1, T2, T3, T4, T5, T6, TResult>(this IInvokable me, Func<T1, T2, T3, T4, T5, T6, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6) =>
		(Singleton.Get<IInvoker>() ?? new SynchronousInvoker()).InvokeFuncDelegate<TResult>(me, func, param1, param2, param3, param4, param5, param6);
}