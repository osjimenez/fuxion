using Fuxion.Shell;
using Ordinem.Calendar.Shell.Wpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using Fuxion.Reflection;

namespace Ordinem.Calendar.Shell.Wpf
{
	public static class CalendarViewNames
	{
		public static PanelName AppointmentList => PanelName.Parse(typeof(AppointmentListViewModel).GetTypeKey());
		public static PanelName AppointmentDetail(Guid? appointmentId = null) => new PanelName(typeof(AppointmentDetailViewModel).GetTypeKey(), appointmentId?.ToString() ?? Guid.NewGuid().ToString());
	}
}
