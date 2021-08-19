namespace Fuxion.Application;

using Fuxion.Domain;

public interface IEventHandler<in TEvent> where TEvent : Event
{
	Task HandleAsync(TEvent @event);
}