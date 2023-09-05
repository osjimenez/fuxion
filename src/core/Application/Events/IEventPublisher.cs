using Fuxion.Domain;

namespace Fuxion.Application.Events;

// TODO - Ver si lo puedo quitar, o hacer internal o algo
public interface IEventPublisher
{
	Task PublishAsync(Event @event);
}