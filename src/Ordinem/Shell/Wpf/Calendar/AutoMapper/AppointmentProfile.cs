using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ordinem.Shell.Wpf.Calendar.AutoMapper
{
	public class AppointmentProfile : Profile
	{
		public AppointmentProfile()
		{
			CreateMap<AppointmentDvo, AppointmentDto>();
			CreateMap<AppointmentDto, AppointmentDvo>();
		}
	}
}
