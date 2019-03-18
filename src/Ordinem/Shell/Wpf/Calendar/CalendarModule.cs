using AutoMapper;
using Fuxion.Collections.Generic;
using Fuxion.Shell;
using Microsoft.Extensions.DependencyInjection;
using Ordinem.Shell.Wpf.Calendar.AutoMapper;
using Ordinem.Shell.Wpf.Calendar.ViewModels;
using Ordinem.Shell.Wpf.Calendar.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ordinem.Shell.Wpf.Calendar
{
	public class CalendarModule : IModule
	{
		public void Register(IServiceCollection services)
		{
			services.AddSingleton<CalendarProxy>();

			services.AddAutoMapperProfile<AppointmentProfile>();

			services.AddMenu("Eventos", () => MessageBus.Current.OpenAppointmentList());

			services.AddPanel<AppointmentListView, AppointmentListViewModel>(defaultPosition: PanelPosition.DockedLeft, removeOnHide: false);
			services.AddPanel<AppointmentDetailView, AppointmentDetailViewModel>();
		}
		public void Initialize(IServiceProvider serviceProvider)
		{
			serviceProvider.GetRequiredService<Cache>().Add(new GenericEqualityComparer<AppointmentDvo>((t1, t2) => t1.Name == t2.Name, t => t.GetHashCode()));
		}
	}
}
