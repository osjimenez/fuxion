using Fuxion.Domain;
using Fuxion.Reflection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ordinem.Calendar.Domain
{
	[TypeKey("Ordinem.Calendar.Domain." + nameof(AppointmentRenamedEvent))]
	public class AppointmentRenamedEvent : Event
	{
		public AppointmentRenamedEvent(Guid appointmentId, string newName) : base(appointmentId)
		{
			NewName = newName;
		}
		public string NewName { get; set; }
	}
}
