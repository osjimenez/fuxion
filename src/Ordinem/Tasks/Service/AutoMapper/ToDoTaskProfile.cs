using AutoMapper;
using Ordinem.Tasks.Projection;
using Ordinem.Tasks.Service.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ordinem.Tasks.Service.AutoMapper
{
	public class ToDoTaskProfile : Profile
	{
		public ToDoTaskProfile()
		{
			CreateMap<ToDoTaskDpo, ToDoTaskDto>();
		}
	}
}
