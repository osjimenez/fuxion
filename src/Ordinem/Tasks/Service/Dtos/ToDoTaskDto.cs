using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ordinem.Tasks.Service.Dtos
{
	public class ToDoTaskDto
	{
		public ToDoTaskDto(Guid id, string name)
		{
			Id = id;
			Name = name;
		}
		public Guid Id { get; set; } = Guid.NewGuid();
		public string Name { get; set; }
	}
}
