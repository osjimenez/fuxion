using Fuxion.Domain;
using Fuxion.Reflection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ordinem.Calendar.Domain
{
	[TypeKey("Ordinem.Calendar.Domain." + nameof(AppointmentDeletedEvent))]
	public class AppointmentDeletedEvent : Event
	{
		public AppointmentDeletedEvent(Guid toDoTaskId) : base(toDoTaskId) { }
	}
}
