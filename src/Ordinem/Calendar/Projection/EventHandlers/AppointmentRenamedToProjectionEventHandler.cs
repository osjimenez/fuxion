using Fuxion.Application;
using Ordinem.Calendar.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ordinem.Calendar.Projection.EventHandlers
{
	public class AppointmentRenamedToProjectionEventHandler : IEventHandler<AppointmentRenamedEvent>
	{
		public AppointmentRenamedToProjectionEventHandler(CalendarDbContext context)
		{
			this.context = context;
		}

		readonly CalendarDbContext context;
		public async Task HandleAsync(AppointmentRenamedEvent @event)
		{
			var appointment = context.Appointments.Find(@event.AggregateId);
			appointment.Name = @event.NewName;
			await context.SaveChangesAsync();
		}
	}
}
