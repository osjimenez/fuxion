using Fuxion.Domain;
using Fuxion.Reflection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ordinem.Tasks.Domain
{
	[TypeKey("Ordinem.Tasks.Application." + nameof(CreateToDoTaskCommand))]
	public class CreateToDoTaskCommand : Command
	{
		public CreateToDoTaskCommand(Guid id, string toDoTaskName)
		{
			Id = id;
			ToDoTaskName = toDoTaskName;
		}
		public Guid Id { get; set; } = Guid.NewGuid();
		public string ToDoTaskName { get; set; }
	}
}
