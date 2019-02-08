using Fuxion.Shell.Messages;
using Fuxion.Shell.ViewModels;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Input;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Docking;

namespace Fuxion.Shell.Views
{
	public partial class ShellWindow : ReactiveWindow<ShellViewModel>
	{
		public ShellWindow(ShellViewModel viewModel, MenuManager menuManager, DockingManager dockingManager)
		{
			InitializeComponent();

			Docking.MouseDown += (_, e) =>
			{
				if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Pressed && e.OriginalSource is FrameworkElement element)
				{
					if (element.ParentOfType<RadPane>() is RadPane pane)
						MessageBus.Current.ClosePanel(pane);
					else if (element.ParentOfType<PaneHeader>() is PaneHeader paneHeader)
						MessageBus.Current.ClosePanel(paneHeader.SelectedPane);
				}
			};

			ViewModel = viewModel;

			menuManager.PopulateMenu(Menu);
			dockingManager.AttachDocking(Docking);
		}
	}
}
