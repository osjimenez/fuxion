using Fuxion.Pods;
using Fuxion.Reflection;

namespace Fuxion.Domain;

[UriKey(UriKey.FuxionBaseUri + "domain/event/1.0.0")]
public abstract
#if NETSTANDARD2_0 || NET462
	class
#else
	record
#endif
	Event(Guid aggregateId) :
#if NETSTANDARD2_0 || NET462
	Featurizable<Event>,
#endif
	IFeaturizable<Event>
{
	public Guid AggregateId { get; set; } = aggregateId;
	IFeatureCollection<Event> IFeaturizable<Event>.Features { get; } = new FeatureCollection<Event>();
}

public static class EventExtensions
{
	public static IFeaturizable<Event> Features(this Event me) => me.Features<Event>();
}