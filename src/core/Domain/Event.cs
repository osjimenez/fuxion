using Fuxion.Domain.Events;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Fuxion.Domain
{
	public abstract record Event(Guid AggregateId)
	{
		internal List<IEventFeature> Features { get; } = new List<IEventFeature>();
	}
}
