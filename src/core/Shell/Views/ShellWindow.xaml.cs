using Fuxion.Shell.UIMessages;
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
				if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Pressed)
				{
					var item = (FrameworkElement)e.OriginalSource;
					if (item.ParentOfType<RadPane>() is RadPane pane)
						pane.IsHidden = true;
					else if (item.ParentOfType<PaneHeader>() is PaneHeader paneHeader)
						paneHeader.SelectedPane.IsHidden = true;

					//var pane = item.ParentOfType<RadPane>();
					//if (pane != null)
					//	(pane as RadPane).IsHidden = true;
					//else
					//{
					//	var paneHeader = item.ParentOfType<PaneHeader>();
					//	if (paneHeader != null)						{
					//		(paneHeader.SelectedPane as RadPane).IsHidden = true;
					//}
				}
			};

			ViewModel = viewModel;

			menuManager.PopulateMenu(Menu);
			dockingManager.AttachDocking(Docking);
		}
		//private void Docking_MouseDown(object sender, MouseButtonEventArgs e)
		//{
		//	if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Pressed)
		//	{
		//		var item = (FrameworkElement)e.OriginalSource;
		//		var pane = item.ParentOfType<RadPane>();
		//		if (pane != null)
		//		{
		//			(pane as RadPane).IsHidden = true;
		//		}
		//		else
		//		{
		//			var paneHeader = item.ParentOfType<PaneHeader>();
		//			if (paneHeader != null)
		//			{
		//				(paneHeader.SelectedPane as RadPane).IsHidden = true;
		//			}
		//		}
		//	}
		//}
	}
}
