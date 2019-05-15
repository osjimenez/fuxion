using Fuxion.Shell;
using Fuxion.Shell.Messages;
using Ordinem.Calendar.Shell.Wpf.ViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;
using Fuxion.Reflection;

namespace Ordinem.Calendar.Shell.Wpf
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
