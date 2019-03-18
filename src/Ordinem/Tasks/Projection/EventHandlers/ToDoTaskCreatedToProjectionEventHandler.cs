using Fuxion.Application;
using Ordinem.Tasks.Application;
using Ordinem.Tasks.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ordinem.Tasks.Projection.EventHandlers
{
	public class ToDoTaskCreatedToProjectEventHandler : IEventHandler<ToDoTaskCreatedEvent>
	{
		public ToDoTaskCreatedToProjectEventHandler(TasksDbContext context)
		{
			this.context = context;
		}

		readonly TasksDbContext context;
		public async Task HandleAsync(ToDoTaskCreatedEvent @event)
		{
			await context.ToDoTasks.AddAsync(new ToDoTaskDpo
			{
				Id = @event.AggregateId,
				Name = @event.Name
			});
			await context.SaveChangesAsync();
		}
	}
}
