using Fuxion.Application;
using Ordinem.Tasks.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ordinem.Tasks.Application
{
	public class DeleteToDoTaskCommandHandler : ICommandHandler<DeleteToDoTaskCommand>
	{
		public DeleteToDoTaskCommandHandler(IRepository<ToDoTask> repository)
		{
			this.repository = repository;
		}
		IRepository<ToDoTask> repository;
		public async Task HandleAsync(DeleteToDoTaskCommand command)
		{
			var toDoTask = await repository.GetAsync(command.Id);
			toDoTask.Delete();
			await repository.CommitAsync();
		}
	}
}
