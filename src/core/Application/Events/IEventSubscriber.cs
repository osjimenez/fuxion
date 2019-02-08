using Fuxion.Domain;
using System;
using System.Collections.Generic;

namespace Fuxion.Application.Events
{
	public interface IEventSubscriber
	{
		void Subscribe<TEvent>() where TEvent : Event;
	}
}
