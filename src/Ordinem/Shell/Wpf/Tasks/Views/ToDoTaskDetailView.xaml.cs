﻿using Fuxion.Shell;
using Ordinem.Shell.Wpf.Tasks.ViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Disposables;
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

namespace Ordinem.Shell.Wpf.Tasks.Views
{
	public partial class ToDoTaskDetailView : ReactiveUserControl<ToDoTaskDetailViewModel>, IPanelView
	{
		public ToDoTaskDetailView(ToDoTaskDetailViewModel viewModel)
		{
			InitializeComponent();

			ViewModel = viewModel;

			this.WhenActivated(d =>
			{
				this.OneWayBind(ViewModel,
					x => x.Dvo.Name,
					x => x.NameTextBlock.Text)
				.DisposeWith(d);

				this.BindCommand(ViewModel,
					x => x.RenameCommand,
					x => x.EditNameButton)
				.DisposeWith(d);

				this.ViewModel
					.RenameInteraction
					.RegisterHandler(inter =>
					{
						RadWindow.Prompt(new DialogParameters
						{
							Header = "Cambio de nombre",
							Content = "Nuevo nombre:",
							DefaultPromptResultValue = inter.Input,
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
			});
		}
		~ToDoTaskDetailView() => Debug.WriteLine($"||||||||||||||||||||||||||||||||| => {this.GetType().Name} DESTROYED");

		public IPanel Panel => ViewModel as IPanel;
	}
}
