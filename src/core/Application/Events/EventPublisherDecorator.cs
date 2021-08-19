namespace Fuxion.Application.Events;

using Fuxion.Domain;

internal class EventPublisherDecorator<TAggregate> : IEventPublisher<TAggregate> where TAggregate : Aggregate
{
	public EventPublisherDecorator(IEventPublisher publisher) => this.publisher = publisher;

	private readonly IEventPublisher publisher;
	public Task PublishAsync(Event @event) => publisher.PublishAsync(@event);
}