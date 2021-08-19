namespace Fuxion.Application;

using Fuxion.Domain;

public interface IEventDispatcher
{
	Task DispatchAsync(Event @event);
}