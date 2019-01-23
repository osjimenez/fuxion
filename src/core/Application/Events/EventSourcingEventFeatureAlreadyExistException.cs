using Fuxion.Domain.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fuxion.Application.Events
{
	public class EventSourcingEventFeatureAlreadyExistException : EventFeatureAlreadyExistException
	{
		public EventSourcingEventFeatureAlreadyExistException(string message) : base(message) { }
	}
}
