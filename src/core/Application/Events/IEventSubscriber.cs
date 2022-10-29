using Fuxion.Domain;

namespace Fuxion.Application.Events;

public interface IEventSubscriber
{
	void Subscribe<TEvent>() where TEvent : Event;
}