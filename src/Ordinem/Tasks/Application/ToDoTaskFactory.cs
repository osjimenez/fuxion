using Fuxion.Application;
using Ordinem.Tasks.Domain;
using System;

namespace Ordinem.Tasks.Application
{
	public class ToDoTaskFactory : Factory<ToDoTask>
	{
		public ToDoTaskFactory(IServiceProvider sp) : base(sp) { }
		public ToDoTask Create(Guid id, string name)
		{
			var toDoTask = Create(id);
			toDoTask.Create(name);
			return toDoTask;
		}
	}
}
