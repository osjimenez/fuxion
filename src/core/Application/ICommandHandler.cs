using Fuxion.Domain;

namespace Fuxion.Application;

public interface ICommandHandler<TCommand> where TCommand : Command
{
	Task HandleAsync(TCommand command);
}