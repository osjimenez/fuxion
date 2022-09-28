namespace Fuxion.Application.Test.Events;

using Fuxion.Domain;
using Fuxion.Reflection;

[TypeKey(nameof(EventBase))]
public record EventBase(Guid AggregateId) : Event(AggregateId)
{
	public string? Name { get; set; }
	public int Age { get; set; }
}
