using Fuxion.Domain;
using System;
using System.Collections.Generic;

namespace Fuxion.Application.Events
{
	public interface IEventSubscriber
	{
		void RegisterTypes(IEnumerable<string> integrationEventTypeIds);
		void Subscribe<TEvent>() where TEvent : Event;
	}
}
