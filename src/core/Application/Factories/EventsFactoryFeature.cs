namespace Fuxion.Application.Factories;

using Fuxion.Domain;
using Fuxion.Domain.Events;

public class EventsFactoryFeature<TAggregate> : IFactoryFeature<TAggregate> where TAggregate : Aggregate
{
	public void Create(TAggregate agg) => agg.AttachEvents();
}