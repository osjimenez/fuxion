using Fuxion.Domain;
using Fuxion.Reflection;
using System;

namespace Ordinem.Tasks.Domain
{
	[TypeKey("Ordinem.Tasks.Domain." + nameof(ToDoTaskRenamedEvent))]
	public class ToDoTaskRenamedEvent : Event
	{
		public ToDoTaskRenamedEvent(Guid toDoTaskId, string newName) : base(toDoTaskId)
		{
			NewName = newName;
		}
		public string NewName { get; set; }
	}
}
