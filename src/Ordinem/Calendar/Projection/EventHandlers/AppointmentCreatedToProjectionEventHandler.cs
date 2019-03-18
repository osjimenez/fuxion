using Fuxion.Application;
using Ordinem.Calendar.Application;
using Ordinem.Calendar.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ordinem.Calendar.Projection.EventHandlers
{
	public class AppointmentCreatedToProjectEventHandler : IEventHandler<AppointmentCreatedEvent>
	{
		public AppointmentCreatedToProjectEventHandler(CalendarDbContext context)
		{
			this.context = context;
		}

		readonly CalendarDbContext context;
		public async Task HandleAsync(AppointmentCreatedEvent @event)
		{
			await context.Appointments.AddAsync(new AppointmentDpo
			{
				Id = @event.AggregateId,
				Name = @event.Name
			});
			await context.SaveChangesAsync();
		}
	}
}
