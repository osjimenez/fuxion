using Fuxion.Application;
using Ordinem.Tasks.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ordinem.Tasks.Projection.EventHandlers
{
	public class ToDoTaskRenamedToProjectionEventHandler : IEventHandler<ToDoTaskRenamedEvent>
	{
		public ToDoTaskRenamedToProjectionEventHandler(TasksDbContext context)
		{
			this.context = context;
		}

		readonly TasksDbContext context;
		public async Task HandleAsync(ToDoTaskRenamedEvent @event)
		{
			var toDoTask = context.ToDoTasks.Find(@event.AggregateId);
			toDoTask.Name = @event.NewName;
			await context.SaveChangesAsync();
		}
	}
}
