using Fuxion.Application.Events;
using Fuxion.Domain;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Fuxion.Application.Commands
{
	public class CommandDispatcher : ICommandDispatcher
	{
		public CommandDispatcher(IServiceProvider serviceProvider) => this.serviceProvider = serviceProvider;

		private readonly IServiceProvider serviceProvider;
		public async Task DispatchAsync(Command command)
		{
			var handlers = serviceProvider.GetServices(typeof(ICommandHandler<>).MakeGenericType(command.GetType())).ToList();
			if (handlers.Count != 1)
			{
				Debug.WriteLine("");
			}
			else
			{
				IEventHandler<Event> c;
				await (Task)handlers[0].GetType().GetMethod(nameof(c.HandleAsync)).Invoke(handlers[0], new object[] { command });
			}
		}
	}
}
