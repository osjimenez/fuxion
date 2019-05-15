using System;
using System.Collections.Generic;
using System.Text;

namespace Ordinem.Calendar.Shell.Wpf
{
	public class AppointmentDto
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public string Name { get; set; }
	}
}
