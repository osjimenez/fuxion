using Fuxion.Domain;
using Fuxion.Reflection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ordinem.Calendar.Domain
{
	[TypeKey("Ordinem.Calendar.Application." + nameof(CreateAppointmentCommand))]
	public class CreateAppointmentCommand : Command
	{
		public CreateAppointmentCommand(Guid id, string appointmentName)
		{
			Id = id;
			AppointmentName = appointmentName;
		}
		public Guid Id { get; set; } = Guid.NewGuid();
		public string AppointmentName { get; set; }
	}
}
