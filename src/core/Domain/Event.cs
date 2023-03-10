using Fuxion.Reflection;

namespace Fuxion.Domain;

[TypeKey(nameof(Fuxion), nameof(Domain), nameof(Event))]
public abstract record Event(Guid AggregateId) : IFeaturizable<Event>
{
	IFeatureCollection<Event> IFeaturizable<Event>.Features { get; } = new FeatureCollection<Event>();
}

public static class EventExtensions
{
	public static IFeaturizable<Event> Features(this Event me) => me.Features<Event>();
}