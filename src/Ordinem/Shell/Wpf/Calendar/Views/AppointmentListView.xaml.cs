using DynamicData;
using Fuxion.Reflection;
using Fuxion.Shell;
using Ordinem.Shell.Wpf.Calendar.ViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Telerik.Windows.Controls;

namespace Ordinem.Shell.Wpf.Calendar.Views
{
	[TypeKey("Ordinem.Shell.Wpf.Calendar.Views." + nameof(AppointmentListView))]
	public partial class AppointmentListView : ReactiveUserControl<AppointmentListViewModel>, IPanelView
	{
		public AppointmentListView(AppointmentListViewModel viewModel)
		{
			InitializeComponent();

			ViewModel = viewModel;

			this.WhenActivated(d =>
			{
				this.OneWayBind(ViewModel,
					x => x.ToDoTasks,
					x => x.GridView.ItemsSource)
				.DisposeWith(d);
				this.Bind(ViewModel,
					x => x.SelectedToDoTask,
					x => x.GridView.SelectedItem)
				.DisposeWith(d);

				this.OneWayBind(ViewModel,
					x => x.AllToDoTasks,
					x => x.DataPager.Source)
				.DisposeWith(d);
				//this.OneWayBind(ViewModel,
				//	x => x.PageRequest.Size,
				//	x => x.DataPager.PageSize)
				//.DisposeWith(d);
				this.WhenAnyValue(x => x.DataPager.PageIndex)
					.Select(x => new PageRequest(x + 1, DataPager.PageSize))
					.BindTo(this, x => x.ViewModel.PageRequest);
				//this.OneWayBind(ViewModel,
				//	x => x.PageRequest.Page,
				//	x => x.DataPager.PageIndex,null,)
				//.DisposeWith(d);

				// CREATE
				this.BindCommand(
					this.ViewModel,
					x => x.CreateCommand,
					x => x.AddButton)
				.DisposeWith(d);
				this.ViewModel
					.CreateInteraction
					.RegisterHandler(inter =>
					{
						RadWindow.Prompt(new DialogParameters
						{
							Header = "Creación de tarea",
							Content = "Nombre:",
							Closed = new EventHandler<WindowClosedEventArgs>((s, e) => {
								if (e.DialogResult ?? false)
									inter.SetOutput(e.PromptResult);
								else
									inter.SetOutput(null);
							}),
							DialogStartupLocation = WindowStartupLocation.CenterOwner,
							Owner = Application.Current.MainWindow
						});
					});

				// EDIT
				this.BindCommand(
					this.ViewModel,
					x => x.EditCommand,
					x => x.EditButton,
					this.WhenAnyValue(x => x.GridView.SelectedItem))
				.DisposeWith(d);
				this.BindCommand(
					this.ViewModel,
					x => x.EditCommand,
					x => x.GridView,
					this.WhenAnyValue(x => x.GridView.SelectedItem), nameof(GridView.MouseDoubleClick))
				.DisposeWith(d);

				// DELETE
				this.BindCommand(
					this.ViewModel,
					x => x.DeleteCommand,
					x => x.DeleteButton,
					this.WhenAnyValue(x => x.GridView.SelectedItem))
				.DisposeWith(d);

				// REFRESH
				this.BindCommand(
					this.ViewModel,
					x => x.RefreshCommand,
					x => x.RefreshButton)
				.DisposeWith(d);
			});
		}
		~AppointmentListView() => Debug.WriteLine($"||||||||||||||||||||||||||||||||| => {this.GetType().Name} DESTROYED");

		public IPanel Panel => ViewModel as IPanel;
	}
}
