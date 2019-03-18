using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ordinem.Calendar.Service.Dtos
{
	public class AppointmentDto
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public string Name { get; set; }
	}
}
