using System;
using System.Collections.Generic;
using System.Text;

namespace Ordinem.Calendar.Shell.Wpf
{
	public class AppointmentDto
	{
		public AppointmentDto(Guid id, string name)
		{
			Id = id;
			Name = name;
		}
		public Guid Id { get; set; } = Guid.NewGuid();
		public string Name { get; set; }
	}
}
