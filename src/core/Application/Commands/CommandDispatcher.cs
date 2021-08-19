namespace Fuxion.Application.Commands;

using Fuxion.Domain;
using Microsoft.Extensions.DependencyInjection;

public class CommandDispatcher : ICommandDispatcher
{
	public CommandDispatcher(IServiceProvider serviceProvider) => this.serviceProvider = serviceProvider;

	private readonly IServiceProvider serviceProvider;
	public async Task DispatchAsync(Command command)
	{
		var handlers = serviceProvider.GetServices(typeof(ICommandHandler<>).MakeGenericType(command.GetType())).ToList();
		if (handlers.Count != 1)
		{
			throw new MoreThanOneCommandHandlerException($"Multiple command handlers was found for command '{command.GetType().GetSignature(true)}'");
		}
		else
		{
			ICommandHandler<Command> c;
			var met = handlers[0]?.GetType().GetMethod(nameof(c.HandleAsync));
			if (met == null) throw new InvalidProgramException($"'{nameof(c.HandleAsync)}' method not found");
			if (met.Invoke(handlers[0], new object[] { command }) is Task task)
				await task;
			else
				throw new InvalidProgramException($"'{nameof(c.HandleAsync)}' method not return a task");
		}
	}
}