namespace Fuxion.Application.Events;

using Fuxion.Domain;

public interface IEventSubscriber
{
	void Subscribe<TEvent>() where TEvent : Event;
}