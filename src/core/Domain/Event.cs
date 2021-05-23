using Fuxion.Domain.Events;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Fuxion.Domain
{
	//public abstract class Event
	//{
	//	protected Event(Guid aggregateId) => AggregateId = aggregateId;
	//	[JsonProperty]
	//	public Guid AggregateId { get; private set; }

	//	internal List<IEventFeature> Features { get; } = new List<IEventFeature>();
	//}
	public abstract record Event(Guid AggregateId)
	{
		//protected Event2(Guid aggregateId) => AggregateId = aggregateId;
		//[JsonProperty]
		//public Guid AggregateId { get; init; }

		internal List<IEventFeature> Features { get; } = new List<IEventFeature>();
	}
}
