using System.Collections;
using Fuxion.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Fuxion.Application.Events;

public class ServiceProviderEventDispatcher : IEventDispatcher
{
	public ServiceProviderEventDispatcher(IServiceProvider serviceProvider) => this.serviceProvider = serviceProvider;
	readonly IServiceProvider serviceProvider;
	public async Task DispatchAsync(Event @event)
	{
		var handlers = (IEnumerable)serviceProvider.GetServices(typeof(IEventHandler<>).MakeGenericType(@event.GetType()));
		foreach (var handler in handlers)
		{
			var met = handler.GetType().GetMethod(nameof(IEventHandler<Event>.HandleAsync))
						 ?? throw new InvalidProgramException($"'{nameof(IEventHandler<Event>.HandleAsync)}' method not found");
				if (met.Invoke(handler, new object[] {
					@event
				}) is Task task)
				await task;
			else
				throw new InvalidProgramException($"'{nameof(IEventHandler<Event>.HandleAsync)}' method not return a task");
		}
	}
}