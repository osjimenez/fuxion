using Fuxion.Application;
using Ordinem.Calendar.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ordinem.Calendar.Projection.EventHandlers
{
	public class AppointmentDeletedToProjectionEventHandler : IEventHandler<AppointmentDeletedEvent>
	{
		public AppointmentDeletedToProjectionEventHandler(CalendarDbContext context)
		{
			this.context = context;
		}
		CalendarDbContext context;
		public async Task HandleAsync(AppointmentDeletedEvent @event)
		{
			var appointment = await context.Appointments.FindAsync(@event.AggregateId);
			context.Remove(appointment);
			await context.SaveChangesAsync();
		}
	}
}
