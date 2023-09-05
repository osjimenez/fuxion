using Fuxion.Domain;

namespace Fuxion.Application;

public interface IEventDispatcher
{
	Task DispatchAsync(Event @event);
}