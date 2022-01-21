namespace Fuxion.Shell;

using System.Reactive.Disposables;
using System.Threading.Tasks;
public interface ICompositeDisposable : IDisposable
{
	CompositeDisposable CompositeDisposable { get; }
	bool Disposed { get; set; }
	void IDisposable.Dispose() => Dispose(true);
#if !DEBUG
			GC.SuppressFinalize(this);
#endif

	protected virtual void Dispose(bool disposing)
	{
		if (!Disposed)
		{
			if (disposing)
			{
				// Dispose managed
				this.CompositeDisposable.Dispose();
			}
			//Dispose unmanaged
		}
		Disposed = true;
	}
}
public static class ICompositeDisposableExtensions
{
	public static void WhileNotDisposed(this ICompositeDisposable me, Action<CompositeDisposable> block) => block(me.CompositeDisposable);
	public static void DisposeWith(this Task<IDisposable> me, CompositeDisposable compositeDisposable)
			=> me.ContinueWith(t => t.Result.DisposeWith(compositeDisposable));
}