using Fuxion.Application;
using Ordinem.Calendar.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ordinem.Tasks.Application
{
	public class DeleteAppointmentCommandHandler : ICommandHandler<DeleteAppointmentCommand>
	{
		public DeleteAppointmentCommandHandler(IRepository<Appointment> repository)
		{
			this.repository = repository;
		}
		readonly IRepository<Appointment> repository;
		public async Task HandleAsync(DeleteAppointmentCommand command)
		{
			var appointment = await repository.GetAsync(command.Id);
			appointment.Delete();
			await repository.CommitAsync();
		}
	}
}
