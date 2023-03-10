using Fuxion.Domain;

namespace Fuxion.Application.Events;

class EventPublisherDecorator<TAggregate> : IEventPublisher<TAggregate> where TAggregate : IAggregate, new()
{
	public EventPublisherDecorator(IEventPublisher publisher) => this.publisher = publisher;
	readonly IEventPublisher publisher;
	public Task PublishAsync(Event @event) => publisher.PublishAsync(@event);
}