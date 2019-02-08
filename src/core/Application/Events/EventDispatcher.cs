using Fuxion.Domain;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Fuxion.Application.Events
{
	public class EventDispatcher : IEventDispatcher
	{
		public EventDispatcher(IServiceProvider serviceProvider)
		{
			this.serviceProvider = serviceProvider;
		}

		readonly IServiceProvider serviceProvider;
		public async Task DispatchAsync(Event @event)
		{
			var handlers = (IEnumerable)serviceProvider.GetServices(typeof(IEventHandler<>).MakeGenericType(@event.GetType()));
			IEventHandler<Event> c;
			foreach (var handler in handlers)
				await (Task)handler.GetType().GetMethod(nameof(c.HandleAsync)).Invoke(handler, new object[] { @event });
		}
	}
}