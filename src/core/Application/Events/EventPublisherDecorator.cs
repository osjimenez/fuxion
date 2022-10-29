using Fuxion.Domain;

namespace Fuxion.Application.Events;

class EventPublisherDecorator<TAggregate> : IEventPublisher<TAggregate> where TAggregate : Aggregate
{
	public EventPublisherDecorator(IEventPublisher publisher) => this.publisher = publisher;
	readonly IEventPublisher publisher;
	public   Task            PublishAsync(Event @event) => publisher.PublishAsync(@event);
}