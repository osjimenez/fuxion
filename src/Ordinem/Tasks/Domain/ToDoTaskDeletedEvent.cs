using Fuxion.Domain;
using Fuxion.Reflection;
using System;

namespace Ordinem.Tasks.Domain
{
	[TypeKey("Ordinem.Tasks.Domain." + nameof(ToDoTaskDeletedEvent))]
	public class ToDoTaskDeletedEvent : Event
	{
		public ToDoTaskDeletedEvent(Guid toDoTaskId) : base(toDoTaskId) { }
	}
}
