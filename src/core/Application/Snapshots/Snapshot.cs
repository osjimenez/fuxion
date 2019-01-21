using Fuxion.Domain;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fuxion.Application.Snapshots
{
	public abstract class Snapshot
	{
		internal Snapshot() { }
		[JsonProperty]
		public Guid Id { get; internal set; } = Guid.NewGuid();
		[JsonProperty]
		public Guid AggregateId { get; internal set; }
		[JsonProperty]
		public int Version { get; internal set; }
		internal abstract void Load(Aggregate aggregate);
	}
}
