using Fuxion.Application;
using Ordinem.Calendar.Domain;
using System;

namespace Ordinem.Calendar.Application
{
	public class AppointmentFactory : Factory<Appointment>
	{
		public AppointmentFactory(IServiceProvider sp) : base(sp) { }
		public Appointment Create(Guid id, string name)
		{
			var appointment = Create(id);
			appointment.Create(name);
			return appointment;
		}
	}
}
