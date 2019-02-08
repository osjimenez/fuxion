using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ordinem.Shell.Wpf.Tasks.AutoMapper
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
