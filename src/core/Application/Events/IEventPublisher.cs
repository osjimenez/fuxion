namespace Fuxion.Application.Events;

using Fuxion.Domain;

// TODO - Ver si lo puedo quitar, o hacer internal o algo
public interface IEventPublisher
{
	Task PublishAsync(Event @event);
}