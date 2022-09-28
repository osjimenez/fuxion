namespace Fuxion.Application.Test.Events;
using Fuxion.Reflection;

[TypeKey(nameof(EventDerived))]
public record EventDerived(Guid AggregateId) : EventBase(AggregateId)
{
	public string? Nick { get; set; }
}