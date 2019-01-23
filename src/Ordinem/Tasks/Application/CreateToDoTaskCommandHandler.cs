using Fuxion.Application;
using Ordinem.Tasks.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ordinem.Tasks.Application
{
	public class CreateToDoTaskCommandHandler : ICommandHandler<CreateToDoTaskCommand>
	{
		public CreateToDoTaskCommandHandler(IRepository<ToDoTask> repository, ToDoTaskFactory factory)
		{
			this.repository = repository;
			this.factory = factory;
		}

		private readonly IRepository<ToDoTask> repository;
		private readonly ToDoTaskFactory factory;
		public async Task HandleAsync(CreateToDoTaskCommand command)
		{
			var toDoTask = factory.Create(command.Id, command.ToDoTaskName);
			await repository.AddAsync(toDoTask);
			await repository.CommitAsync();
		}
	}
}
