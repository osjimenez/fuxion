using System.Windows.Input;
using Fuxion.Telerik_.Wpf.ViewModels;
using ReactiveUI;
using Telerik.Windows.Controls;

namespace Fuxion.Telerik_.Wpf.Views;

public partial class ShellWindow
{
	public ShellWindow(ShellWindowViewModel viewModel, MenuManager menuManager, DockingManager dockingManager)
	{
		//global::Telerik.Windows.Controls.radm
		InitializeComponent();
		Docking.PreviewMouseDown += (_, e) => {
			if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Pressed && e.Source is RadPane pane)
			{
				// MessageBus.Current.ClosePanel(pane);
				// //TODO RadTabItem BUG. Reported in https://feedback.telerik.com/wpf/1411792-tabcontrol-tabitem-is-removed-when-clicking-with-mouse-middle-button-over-it
				// e.Handled = true;
			}
		};
		Docking.MouseDown += (_, e) => {
			//if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Pressed && e.OriginalSource is FrameworkElement element)
			if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Pressed && e.Source is RadPane pane)
			{
				//MessageBus.Current.ClosePanel(pane);

				//if (element.ParentOfType<RadPane>() is RadPane pane)
				//	MessageBus.Current.ClosePane(pane);
				//else if (element.ParentOfType<PaneHeader>() is PaneHeader paneHeader)
				//	MessageBus.Current.ClosePane(paneHeader.SelectedPane);
			}
		};
		DataContext = viewModel;
		// ViewModel = viewModel;
		menuManager.PopulateMenu(Menu);
		dockingManager.AttachDocking(Docking);
	}
}