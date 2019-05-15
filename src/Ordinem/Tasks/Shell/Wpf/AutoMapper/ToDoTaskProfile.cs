using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ordinem.Tasks.Shell.Wpf.AutoMapper
{
	public class ToDoTaskProfile : Profile
	{
		public ToDoTaskProfile()
		{
			CreateMap<ToDoTaskDvo, ToDoTaskDto>();
			CreateMap<ToDoTaskDto, ToDoTaskDvo>();
		}
	}
}
