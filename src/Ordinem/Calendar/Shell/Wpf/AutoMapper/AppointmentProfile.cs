using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ordinem.Calendar.Shell.Wpf.AutoMapper
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
