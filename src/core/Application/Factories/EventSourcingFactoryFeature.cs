using Fuxion.Application.Aggregates;
using Fuxion.Domain;

namespace Fuxion.Application.Factories;

public class EventSourcingFactoryFeature<TAggregate> : IFactoryFeature<TAggregate> where TAggregate : Aggregate
{
	public void Create(TAggregate agg) => agg.AttachEventSourcing();
}