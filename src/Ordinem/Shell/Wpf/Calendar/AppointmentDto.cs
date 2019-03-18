using System;
using System.Collections.Generic;
using System.Text;

namespace Ordinem.Shell.Wpf.Calendar
{
	public class AppointmentDto
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public string Name { get; set; }
	}
}
