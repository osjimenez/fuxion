namespace System;

public class DisposableEnvelope<T> : IDisposable where T : notnull
{
	public DisposableEnvelope(T obj, Action<T>? actionOnDispose = null)
	{
		Action = actionOnDispose;
		value  = obj;
	}
	bool disposed;

	//internal event EventHandler<EventArgs<T>> BeforeDisposed;
	//public event EventHandler<EventArgs<T>> Disposed;
	//internal event EventHandler<EventArgs<T>> AfterDisposed;
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
	protected Action<T>? Action    { get; set; }
	void IDisposable.    Dispose() => OnDispose();
	protected virtual void OnDispose()
	{
		disposed = true;
		Action?.Invoke(Value);
		//BeforeDisposed?.Invoke(this, new EventArgs<T>(Value));
		//Disposed?.Invoke(this, new EventArgs<T>(Value));
		//AfterDisposed?.Invoke(this, new EventArgs<T>(Value));
	}
}