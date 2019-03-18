using Fuxion.Domain;
using Fuxion.Reflection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ordinem.Calendar.Domain
{
	[TypeKey("Ordinem.Calendar.Domain." + nameof(AppointmentCreatedEvent))]
	public class AppointmentCreatedEvent : Event
	{
		public AppointmentCreatedEvent(Guid appointmentId, string name) : base(appointmentId) => Name = name;
		public string Name { get; private set; }
	}
}
