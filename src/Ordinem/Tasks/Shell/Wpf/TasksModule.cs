using AutoMapper;
using Fuxion.Collections.Generic;
using Fuxion.Shell;
using Microsoft.Extensions.DependencyInjection;
using Ordinem.Tasks.Shell.Wpf.AutoMapper;
using Ordinem.Tasks.Shell.Wpf.ViewModels;
using Ordinem.Tasks.Shell.Wpf.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ordinem.Tasks.Shell.Wpf
{
	public class TasksModule : IModule
	{
		public void Register(IServiceCollection services)
		{
			services.AddSingleton<TasksProxy>();

			services.AddAutoMapperProfile<ToDoTaskProfile>();

			services.AddMenu("Tasks", () => MessageBus.Current.OpenToDoTaskList());

			services.AddPanel<ToDoTaskListView, ToDoTaskListViewModel>(defaultPosition: PanelPosition.DockedLeft, removeOnHide: false);
			services.AddPanel<ToDoTaskDetailView, ToDoTaskDetailViewModel>();
		}
		public void Initialize(IServiceProvider serviceProvider)
		{
			serviceProvider.GetRequiredService<Cache>().Add(new GenericEqualityComparer<ToDoTaskDvo>((t1, t2) => t1?.Name == t2?.Name, t => t.GetHashCode()));
		}
	}
}
