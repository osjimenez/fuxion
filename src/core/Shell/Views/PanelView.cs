namespace Fuxion.Shell.Views;

using ReactiveUI;
using System.Reactive.Disposables;

public class PanelView<TViewModel> : ReactiveUserControl<TViewModel>, IPanelView, ICompositeDisposable where TViewModel : class, IPanel
{
	public PanelView(ILogger<PanelView<TViewModel>> logger, TViewModel viewModel)
	{
		ViewModel = viewModel;
		Logger = logger;
		Logger.LogTrace($"PanelView '{GetType().Name}' CREATED");
	}
	protected ILogger Logger { get; }
#if DEBUG
	~PanelView() => Logger.LogTrace($"PanelView '{GetType().Name}' FINALIZED");
#endif
	public IPanel Panel => ViewModel ?? throw new InvalidProgramException($"'{nameof(ViewModel)}' cannot be null");

	CompositeDisposable ICompositeDisposable.CompositeDisposable { get; } = new();
	bool ICompositeDisposable.Disposed { get; set; }
}