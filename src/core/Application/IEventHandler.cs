using Fuxion.Domain;

namespace Fuxion.Application;

public interface IEventHandler<in TEvent> where TEvent : Event
{
	Task HandleAsync(TEvent @event);
}