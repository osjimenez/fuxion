namespace Fuxion.Application.Events;

public class EventSubscription
{
	public EventSubscription(Type eventType) => EventType = eventType;
	public Type EventType { get; }
}