using System.Reactive.Disposables;
using ReactiveUI;

namespace Fuxion.Telerik_.Wpf.Views;

public class PanelView<TViewModel> : ReactiveUserControl<TViewModel>, IPanelView, ICompositeDisposable where TViewModel : class, IPanel
{
	public PanelView(ILogger<PanelView<TViewModel>> logger, TViewModel viewModel)
	{
		ViewModel = viewModel;
		Logger = logger;
		Logger.LogTrace($"PanelView '{GetType().Name}' CREATED");
	}
	protected ILogger Logger { get; }
	CompositeDisposable ICompositeDisposable.CompositeDisposable { get; } = new();
	bool ICompositeDisposable.Disposed { get; set; }
	public IPanel Panel => ViewModel ?? throw new InvalidProgramException($"'{nameof(ViewModel)}' cannot be null");
#if DEBUG
	~PanelView() => Logger.LogTrace($"PanelView '{GetType().Name}' FINALIZED");
#endif
}