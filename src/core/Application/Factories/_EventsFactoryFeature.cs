#if false
using Fuxion.Domain;
using Fuxion.Domain.Aggregates;

namespace Fuxion.Application.Factories;

public class EventsFactoryFeature<TAggregate> : IFactoryFeature<TAggregate> where TAggregate : IAggregate, new()
{
	Factory<TAggregate>? IFactoryFeature<TAggregate>.Factory { get; set; }
	public void Initialize(TAggregate agg) => agg.Add<EventsAggregateFeature>();
}
#endif