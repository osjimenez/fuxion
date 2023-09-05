#if false
using Fuxion.Application.Aggregates;
using Fuxion.Domain;

namespace Fuxion.Application.Factories;

public class EventSourcingFactoryFeature<TAggregate> : IFactoryFeature<TAggregate> where TAggregate : IAggregate, new()
{
	Factory<TAggregate>? IFactoryFeature<TAggregate>.Factory { get; set; }
	public void Initialize(TAggregate agg) => agg.Add<EventSourcingAggregateFeature>();
}
#endif