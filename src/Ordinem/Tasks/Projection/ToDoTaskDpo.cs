using System;
using System.Collections.Generic;
using System.Text;

namespace Ordinem.Tasks.Projection
{
	public class ToDoTaskDpo
	{
		public ToDoTaskDpo(Guid id, string name)
		{
			Id = id;
			Name = name;
		}
		public Guid Id { get; set; }
		public string Name { get; set; }
	}
}
