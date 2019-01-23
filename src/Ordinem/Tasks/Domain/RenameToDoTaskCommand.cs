using Fuxion.Domain;
using Fuxion.Reflection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ordinem.Tasks.Domain
{
	[TypeKey("Ordinem.Tasks.Application." + nameof(RenameToDoTaskCommand))]
	public class RenameToDoTaskCommand : Command
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public string NewName { get; set; }
	}
}
