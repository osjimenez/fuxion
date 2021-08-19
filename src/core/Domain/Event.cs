namespace Fuxion.Domain;

using Fuxion.Domain.Events;

public abstract record Event(Guid AggregateId)
{
	internal List<IEventFeature> Features { get; } = new();
}