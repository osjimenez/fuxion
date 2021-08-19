namespace Fuxion.Application.Factories;

using Fuxion.Application.Aggregates;
using Fuxion.Domain;

public class EventSourcingFactoryFeature<TAggregate> : IFactoryFeature<TAggregate> where TAggregate : Aggregate
{
	public void Create(TAggregate agg) => agg.AttachEventSourcing();
}