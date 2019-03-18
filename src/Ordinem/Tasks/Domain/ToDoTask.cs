using Fuxion.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ordinem.Tasks.Domain
{
	public class ToDoTask : Aggregate
	{
		public void Create(string taskName) => ApplyEvent(new ToDoTaskCreatedEvent(Id, taskName));
		public void Rename(string newName) => ApplyEvent(new ToDoTaskRenamedEvent(Id, newName));
		public void Delete() => ApplyEvent(new ToDoTaskDeletedEvent(Id));

		internal string name;

		[AggregateEventHandler]
		void WhenCreated(ToDoTaskCreatedEvent @event)
		{
			name = @event.Name;
		}
		[AggregateEventHandler]
		void WhenRenamed(ToDoTaskRenamedEvent @event)
		{
			name = @event.NewName;
		}
		[AggregateEventHandler]
		void WhenRenamed(ToDoTaskDeletedEvent @event)
		{
		}
	}
}
