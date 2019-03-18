using Fuxion.Application;
using Ordinem.Calendar.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ordinem.Tasks.Application
{
	public class RenameAppointmentCommandHandler : ICommandHandler<RenameAppointmentCommand>
	{
		public RenameAppointmentCommandHandler(IRepository<Appointment> repository)
		{
			this.repository = repository;
		}
		readonly IRepository<Appointment> repository;
		public async Task HandleAsync(RenameAppointmentCommand command)
		{
			var appointment = await repository.GetAsync(command.Id);
			appointment.Rename(command.NewName);
			await repository.CommitAsync();
		}
	}
}
