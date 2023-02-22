using System.Windows.Input;
using Fuxion.Shell.ViewModels;
using ReactiveUI;
using Telerik.Windows.Controls;

namespace Fuxion.Shell.Views;

#nullable disable
public partial class ShellWindow
#nullable enable
{
	public ShellWindow(ShellWindowViewModel viewModel, MenuManager menuManager, DockingManager dockingManager)
	{
		InitializeComponent();
		Docking.PreviewMouseDown += (_, e) => {
			if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Pressed && e.Source is RadPane pane)
			{
				MessageBus.Current.ClosePanel(pane);
				//TODO RadTabItem BUG. Reported in https://feedback.telerik.com/wpf/1411792-tabcontrol-tabitem-is-removed-when-clicking-with-mouse-middle-button-over-it
				e.Handled = true;
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
		ViewModel = viewModel;
		menuManager.PopulateMenu(Menu);
		dockingManager.AttachDocking(Docking);
	}
}