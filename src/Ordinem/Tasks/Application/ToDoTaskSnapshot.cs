using Fuxion.Application;
using Fuxion.Reflection;
using Newtonsoft.Json;
using Ordinem.Tasks.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ordinem.Tasks.Application
{
	[TypeKey("Ordinem.Tasks.Application." + nameof(ToDoTaskSnapshot))]
	public class ToDoTaskSnapshot : Snapshot<ToDoTask>
	{
		[JsonProperty]
		public string Name { get; private set; }

		protected override void Hydrate(ToDoTask toDoTask) => toDoTask.name = Name;
		protected override void Load(ToDoTask toDoTask) => Name = toDoTask.name;
	}
}
