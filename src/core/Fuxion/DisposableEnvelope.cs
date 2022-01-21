namespace System;

public class DisposableEnvelope<T> : IDisposable where T : notnull
{
	public DisposableEnvelope(T obj, Action<T>? actionOnDispose = null)
	{
		Action = actionOnDispose;
		value = obj;
	}

	//internal event EventHandler<EventArgs<T>> BeforeDisposed;
	//public event EventHandler<EventArgs<T>> Disposed;
	//internal event EventHandler<EventArgs<T>> AfterDisposed;
	private T value;
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

	private bool disposed = false;
	protected virtual void OnDispose()
	{
		disposed = true;
		Action?.Invoke(Value);
		//BeforeDisposed?.Invoke(this, new EventArgs<T>(Value));
		//Disposed?.Invoke(this, new EventArgs<T>(Value));
		//AfterDisposed?.Invoke(this, new EventArgs<T>(Value));
	}
	void IDisposable.Dispose() => OnDispose();
}