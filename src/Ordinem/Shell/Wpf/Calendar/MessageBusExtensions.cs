using Fuxion.Shell;
using Fuxion.Shell.Messages;
using Ordinem.Shell.Wpf.Calendar.ViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;
using Fuxion.Reflection;

namespace Ordinem.Shell.Wpf.Calendar
{
	public static class MessageBusExtensions
	{
		public static void OpenAppointmentList(this IMessageBus me)
		{
			me.OpenPanel(CalendarViewNames.AppointmentList);
		}
		public static void OpenAppointment(this IMessageBus me, Guid appointmentId)
		{
			me.OpenPanel(CalendarViewNames.AppointmentDetail(appointmentId), ("Id", appointmentId));
		}
	}
}
