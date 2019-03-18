using AutoMapper;
using Ordinem.Calendar.Projection;
using Ordinem.Calendar.Service.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ordinem.Tasks.Service.AutoMapper
{
	public class AppointmentProfile : Profile
	{
		public AppointmentProfile()
		{
			CreateMap<AppointmentDpo, AppointmentDto>();
		}
	}
}
