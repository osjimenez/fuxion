using Fuxion.Domain;
using Fuxion.Reflection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ordinem.Tasks.Domain
{
	[TypeKey("Ordinem.Tasks.Application." + nameof(DeleteToDoTaskCommand))]
	public class DeleteToDoTaskCommand : Command
	{
		public Guid Id { get; set; } = Guid.NewGuid();
	}
}
