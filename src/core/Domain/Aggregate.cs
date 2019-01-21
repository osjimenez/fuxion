using Fuxion.Domain.Aggregates;
using Fuxion.Domain.Events;
using System;
using System.Collections.Generic;

namespace Fuxion.Domain
{
	public abstract class Aggregate
	{
		public Guid Id { get; internal set; }
		internal List<IAggregateFeature> Features { get; set; } = new List<IAggregateFeature>();
		protected void ApplyEvent(Event @event) => this.Events().ApplyEvent(@event);
	}
}
