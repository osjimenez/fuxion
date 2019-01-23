using AutoMapper;
using Fuxion.Collections.Generic;
using Fuxion.Shell;
using Microsoft.Extensions.DependencyInjection;
using Ordinem.Shell.Wpf.Tasks.AutoMapper;
using Ordinem.Shell.Wpf.Tasks.ViewModels;
using Ordinem.Shell.Wpf.Tasks.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ordinem.Shell.Wpf.Tasks
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
			serviceProvider.GetRequiredService<Cache>().Add(new GenericEqualityComparer<ToDoTaskDvo>((t1, t2) => t1.Name == t2.Name, t => t.GetHashCode()));
		}
	}
}
