using System.Reactive.Disposables;
using ReactiveUI;

namespace Fuxion.Shell.ViewModels;

public abstract class ShellViewModel : ReactiveObject, IActivatableViewModel, ICompositeDisposable
{
	public ShellViewModel(ILogger<ShellViewModel> logger)
	{
		Activator = new();
		Logger    = logger;
		Logger.LogTrace($"ShellViewModel '{GetType().Name}' CREATED");
		this.WhenActivated(d =>
		{
			HandleActivation();
			Disposable.Create(() => HandleDeactivation()).DisposeWith(d);
		});
	}
	protected ILogger                        Logger              { get; }
	public    ViewModelActivator             Activator           { get; }
	CompositeDisposable ICompositeDisposable.CompositeDisposable { get; } = new();
	bool ICompositeDisposable.               Disposed            { get; set; }
#if DEBUG
	~ShellViewModel() => Logger.LogTrace($"ShellViewModel '{GetType().Name}' FINALIZED");
#endif
	protected virtual void HandleActivation()   { }
	protected virtual void HandleDeactivation() { }
}