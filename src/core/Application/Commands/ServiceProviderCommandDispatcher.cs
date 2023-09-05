using Fuxion.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Fuxion.Application.Commands;

public class ServiceProviderCommandDispatcher : ICommandDispatcher
{
	readonly IServiceProvider serviceProvider;
	public ServiceProviderCommandDispatcher(IServiceProvider serviceProvider) => this.serviceProvider = serviceProvider;
	public async Task DispatchAsync(Command command)
	{
		var handlers = serviceProvider.GetServices(typeof(ICommandHandler<>).MakeGenericType(command.GetType())).ToList();
		if (handlers.Count == 0) throw new NoCommandHandlerFoundException($"No command handlers was found for command '{command.GetType().GetSignature(true)}'");
		if (handlers.Count != 1) throw new MoreThanOneCommandHandlerException($"Multiple command handlers was found for command '{command.GetType().GetSignature(true)}'");
		var met = handlers[0]?.GetType().GetMethod(nameof(ICommandHandler<Command>.HandleAsync))
					 ?? throw new InvalidProgramException($"'{nameof(ICommandHandler<Command>.HandleAsync)}' method not found");
		if (met.Invoke(handlers[0], new object[]
			 {
				 command
			 }) is Task task)
			await task;
		else
			throw new InvalidProgramException($"'{nameof(ICommandHandler<Command>.HandleAsync)}' method not return a task");
	}
}