using Fuxion.Domain;

namespace Fuxion.Application.Commands;

public interface ICommandDispatcher
{
	Task DispatchAsync(Command command);
}