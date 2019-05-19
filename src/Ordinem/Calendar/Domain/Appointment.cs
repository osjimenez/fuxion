using Fuxion.Domain;
using System;

namespace Ordinem.Calendar.Domain
{
	public class Appointment : Aggregate
	{
		public void Create(string taskName) => ApplyEvent(new AppointmentCreatedEvent(Id, taskName));
		public void Rename(string newName) => ApplyEvent(new AppointmentRenamedEvent(Id, newName));
		public void Delete() => ApplyEvent(new AppointmentDeletedEvent(Id));

		internal string? name;

		[AggregateEventHandler]
		void WhenCreated(AppointmentCreatedEvent @event)
		{
			name = @event.Name;
		}
		[AggregateEventHandler]
		void WhenRenamed(AppointmentRenamedEvent @event)
		{
			name = @event.NewName;
		}
		[AggregateEventHandler]
		void WhenRenamed(AppointmentDeletedEvent @event)
		{
		}
	}
}
