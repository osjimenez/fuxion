using Fuxion.Domain;
using Fuxion.Reflection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ordinem.Calendar.Domain
{
	[TypeKey("Ordinem.Calendar.Application." + nameof(RenameAppointmentCommand))]
	public class RenameAppointmentCommand : Command
	{
		public RenameAppointmentCommand(Guid id, string newName)
		{
			Id = id;
			NewName = newName;
		}
		public Guid Id { get; set; } = Guid.NewGuid();
		public string NewName { get; set; }
	}
}
