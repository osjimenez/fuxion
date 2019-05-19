using Fuxion.Application;
using Fuxion.Reflection;
using Newtonsoft.Json;
using Ordinem.Calendar.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ordinem.Calendar.Application
{
	[TypeKey("Ordinem.Calendar.Application." + nameof(AppointmentSnapshot))]
	public class AppointmentSnapshot : Snapshot<Appointment>
	{
		[JsonProperty]
		public string? Name { get; private set; }

		protected override void Hydrate(Appointment appointment) => appointment.name = Name;
		protected override void Load(Appointment appointment) => Name = appointment.name;
	}
}
