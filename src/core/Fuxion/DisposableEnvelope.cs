namespace System;

public class DisposableEnvelope<T> : IDisposable, IAsyncDisposable where T : notnull
{
	public DisposableEnvelope(T obj, Action<T>? actionOnDispose = null)
	{
		Action = actionOnDispose;
		value = obj;
	}
	public DisposableEnvelope(T obj, Func<T, ValueTask>? functionOnDispose = null)
	{
		Function = functionOnDispose;
		value = obj;
	}
	bool disposed;

	T value;
	public T Value
	{
		get => value;
		set
		{
			if (disposed) throw new ObjectDisposedException(nameof(Value));
			this.value = value;
		}
	}
	protected Action<T>? Action { get; set; }
	protected Func<T, ValueTask>? Function { get; set; }
	
	void IDisposable.Dispose() => OnDispose();
	ValueTask IAsyncDisposable.DisposeAsync() => OnDisposeAsync();
	protected virtual void OnDispose()
	{
		disposed = true;
		Action?.Invoke(Value);
		Function?.Invoke(Value).AsTask().Wait();
	}
	protected virtual async ValueTask OnDisposeAsync()
	{
		disposed = true;
		Action?.Invoke(Value);
		if(Function is not null)
			await Function.Invoke(Value);
	}
}