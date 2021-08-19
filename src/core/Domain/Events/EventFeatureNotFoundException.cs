namespace Fuxion.Domain.Events;

public class EventFeatureNotFoundException : FuxionException
{
	public EventFeatureNotFoundException(string message) : base(message) { }
}