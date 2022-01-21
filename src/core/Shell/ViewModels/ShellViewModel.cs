namespace Fuxion.Shell.ViewModels;

using ReactiveUI;
using System.Reactive.Disposables;

public abstract class ShellViewModel : ReactiveObject, IActivatableViewModel, ICompositeDisposable
{
	public ShellViewModel(ILogger<ShellViewModel> logger)
	{
		Activator = new ViewModelActivator();

		Logger = logger;
		Logger.LogTrace($"ShellViewModel '{GetType().Name}' CREATED");

		this.WhenActivated(d =>
		{
			HandleActivation();
			Disposable.Create(() => HandleDeactivation())
				.DisposeWith(d);
		});
	}
	public ViewModelActivator Activator { get; }
	protected ILogger Logger { get; }
#if DEBUG
	~ShellViewModel() => Logger.LogTrace($"ShellViewModel '{GetType().Name}' FINALIZED");
#endif
	protected virtual void HandleActivation() { }
	protected virtual void HandleDeactivation() { }

	CompositeDisposable ICompositeDisposable.CompositeDisposable { get; } = new();
	bool ICompositeDisposable.Disposed { get; set; }
}