namespace Fuxion.Domain.Events;

public class EventFeatureAlreadyExistException : FuxionException
{
	public EventFeatureAlreadyExistException(string message) : base(message) { }
}