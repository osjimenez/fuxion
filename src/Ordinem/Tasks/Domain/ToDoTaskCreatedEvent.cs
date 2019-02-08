using Fuxion.Domain;
using Fuxion.Reflection;
using System;

namespace Ordinem.Tasks.Domain
{
	[TypeKey("Ordinem.Tasks.Domain." + nameof(ToDoTaskCreatedEvent))]
	public class ToDoTaskCreatedEvent : Event
	{
		public ToDoTaskCreatedEvent(Guid toDoTaskId, string name) : base(toDoTaskId) => Name = name;
		public string Name { get; private set; }
	}
}
