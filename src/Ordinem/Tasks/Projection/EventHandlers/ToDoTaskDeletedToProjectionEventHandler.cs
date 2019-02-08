using Fuxion.Application;
using Ordinem.Tasks.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ordinem.Tasks.Projection.EventHandlers
{
	public class ToDoTaskDeletedToProjectionEventHandler : IEventHandler<ToDoTaskDeletedEvent>
	{
		public ToDoTaskDeletedToProjectionEventHandler(TasksDbContext context)
		{
			this.context = context;
		}
		TasksDbContext context;
		public async Task HandleAsync(ToDoTaskDeletedEvent @event)
		{
			var toDoTask = await context.ToDoTasks.FindAsync(@event.AggregateId);
			context.Remove(toDoTask);
			await context.SaveChangesAsync();
		}
	}
}
