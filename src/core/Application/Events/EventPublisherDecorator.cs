using Fuxion.Domain;
using System.Threading.Tasks;

namespace Fuxion.Application.Events
{
	internal class EventPublisherDecorator<TAggregate> : IEventPublisher<TAggregate> where TAggregate : Aggregate
	{
		public EventPublisherDecorator(IEventPublisher publisher)
		{
			this.publisher = publisher;
		}
		IEventPublisher publisher;
		public Task PublishAsync(Event @event) => publisher.PublishAsync(@event);
	}
}
