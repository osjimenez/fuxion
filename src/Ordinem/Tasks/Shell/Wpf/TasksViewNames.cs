using Fuxion.Shell;
using Ordinem.Tasks.Shell.Wpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using Fuxion.Reflection;

namespace Ordinem.Tasks.Shell.Wpf
{
	public static class TasksViewNames
	{
		public static PanelName ToDoTaskList => PanelName.Parse(typeof(ToDoTaskListViewModel).GetTypeKey());
		public static PanelName ToDoDetail(Guid? toDoTaskId = null) => new PanelName(typeof(ToDoTaskDetailViewModel).GetTypeKey(), toDoTaskId?.ToString() ?? Guid.NewGuid().ToString());
	}
}
