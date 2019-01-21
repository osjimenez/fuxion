using Fuxion.Domain.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Fuxion.Domain
{
	public abstract class Event
	{
		protected Event(Guid aggregateId) => AggregateId = aggregateId;
		public Guid AggregateId { get; }

		internal List<IEventFeature> Features { get; } = new List<IEventFeature>();
	}
}
