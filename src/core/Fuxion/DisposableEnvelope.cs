namespace System
{
	public class DisposableEnvelope<T> : IDisposable
	{
		public DisposableEnvelope(T obj, Action<T>? actionOnDispose = null)
		{
			this.Action = actionOnDispose;
			this.Value = obj;
		}
		//internal event EventHandler<EventArgs<T>> BeforeDisposed;
		//public event EventHandler<EventArgs<T>> Disposed;
		//internal event EventHandler<EventArgs<T>> AfterDisposed;
		T value = default!;
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
		bool disposed = false;
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
}
