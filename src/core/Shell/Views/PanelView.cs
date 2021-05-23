using Microsoft.Extensions.Logging;
using ReactiveUI;
using System;

namespace Fuxion.Shell.Views
{
	public class PanelView<TViewModel> : ReactiveUserControl<TViewModel>, IPanelView where TViewModel : class, IPanel
	{
		public PanelView(ILogger<PanelView<TViewModel>> logger, TViewModel viewModel)
		{
			ViewModel = viewModel;
			Logger = logger;
			Logger.LogTrace($"PanelView '{GetType().Name}' CREATED");
		}
		protected ILogger Logger { get; }
		~PanelView() => Logger.LogTrace($"PanelView '{GetType().Name}' FINALIZED");
		public IPanel Panel => ViewModel ?? throw new InvalidProgramException($"'{nameof(ViewModel)}' cannot be null");
	}
}
