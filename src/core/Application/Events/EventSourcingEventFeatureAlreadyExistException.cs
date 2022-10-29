using Fuxion.Domain.Events;

namespace Fuxion.Application.Events;

public class EventSourcingEventFeatureAlreadyExistException : EventFeatureAlreadyExistException
{
	public EventSourcingEventFeatureAlreadyExistException(string message) : base(message) { }
}