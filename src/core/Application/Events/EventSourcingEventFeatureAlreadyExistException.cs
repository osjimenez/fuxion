namespace Fuxion.Application.Events;

using Fuxion.Domain.Events;

public class EventSourcingEventFeatureAlreadyExistException : EventFeatureAlreadyExistException
{
	public EventSourcingEventFeatureAlreadyExistException(string message) : base(message) { }
}