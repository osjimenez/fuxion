using Fuxion.Shell;
using Ordinem.Shell.Wpf.Tasks.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using Fuxion.Reflection;

namespace Ordinem.Shell.Wpf.Tasks
{
	public static class TasksViewNames
	{
		public static PanelName ToDoTaskList => PanelName.Parse(typeof(ToDoTaskListViewModel).GetTypeKey());
		public static PanelName ToDoDetail(Guid? toDoTaskId = null) => new PanelName(typeof(ToDoTaskDetailViewModel).GetTypeKey(), toDoTaskId?.ToString() ?? Guid.NewGuid().ToString());
	}
}
