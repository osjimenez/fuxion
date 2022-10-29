using Fuxion.Domain.Events;

namespace Fuxion.Domain;

public abstract record Event(Guid AggregateId)
{
	internal List<IEventFeature> Features { get; } = new();
}