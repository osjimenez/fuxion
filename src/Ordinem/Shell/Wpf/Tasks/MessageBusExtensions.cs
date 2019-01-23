using Fuxion.Shell;
using Fuxion.Shell.Messages;
using Ordinem.Shell.Wpf.Tasks.ViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;
using Fuxion.Reflection;

namespace Ordinem.Shell.Wpf.Tasks
{
	public static class MessageBusExtensions
	{
		public static void OpenToDoTaskList(this IMessageBus me)
		{
			me.OpenPanel(TasksViewNames.ToDoTaskList);
		}
		public static void OpenToDoTask(this IMessageBus me, Guid toDoTaskId)
		{
			me.OpenPanel(TasksViewNames.ToDoDetail(toDoTaskId), ("Id", toDoTaskId));
		}
	}
}
