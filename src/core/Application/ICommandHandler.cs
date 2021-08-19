namespace Fuxion.Application;

using Fuxion.Domain;

public interface ICommandHandler<TCommand> where TCommand : Command
{
	Task HandleAsync(TCommand command);
}