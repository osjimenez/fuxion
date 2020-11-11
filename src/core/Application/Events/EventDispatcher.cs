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
			{
				var met = handler.GetType().GetMethod(nameof(c.HandleAsync));
				if (met == null) throw new InvalidProgramException($"'{nameof(c.HandleAsync)}' method not found");
				if (met.Invoke(handler, new object[] { @event }) is Task task)
					await task;
				else
					throw new InvalidProgramException($"'{nameof(c.HandleAsync)}' method not return a task");
			}
		}
	}
}