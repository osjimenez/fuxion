using Fuxion.Application;
using Ordinem.Calendar.Application;
using Ordinem.Calendar.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ordinem.Tasks.Application
{
	public class CreateAppointmentCommandHandler : ICommandHandler<CreateAppointmentCommand>
	{
		public CreateAppointmentCommandHandler(IRepository<Appointment> repository, AppointmentFactory factory)
		{
			this.repository = repository;
			this.factory = factory;
		}

		private readonly IRepository<Appointment> repository;
		private readonly AppointmentFactory factory;
		public async Task HandleAsync(CreateAppointmentCommand command)
		{
			var appointment = factory.Create(command.Id, command.AppointmentName);
			await repository.AddAsync(appointment);
			await repository.CommitAsync();
		}
	}
}
