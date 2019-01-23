using Fuxion.Application;
using Ordinem.Tasks.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ordinem.Tasks.Application
{
	public class RenameToDoTaskCommandHandler : ICommandHandler<RenameToDoTaskCommand>
	{
		public RenameToDoTaskCommandHandler(IRepository<ToDoTask> repository)
		{
			this.repository = repository;
		}
		readonly IRepository<ToDoTask> repository;
		public async Task HandleAsync(RenameToDoTaskCommand command)
		{
			var toDoTask = await repository.GetAsync(command.Id);
			toDoTask.Rename(command.NewName);
			await repository.CommitAsync();
		}
	}
}
