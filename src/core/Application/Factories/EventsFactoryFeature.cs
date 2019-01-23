using Fuxion.Domain;
using Fuxion.Domain.Events;

namespace Fuxion.Application.Factories
{
	public class EventsFactoryFeature<TAggregate> : IFactoryFeature<TAggregate> where TAggregate : Aggregate
	{
		public void Create(TAggregate agg) => agg.AttachEvents();
	}
}