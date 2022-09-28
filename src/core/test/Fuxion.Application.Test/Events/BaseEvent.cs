namespace Fuxion.Application.Test.Events;

using Fuxion.Domain;
using Fuxion.Reflection;

[TypeKey(nameof(BaseEvent))]
public record BaseEvent(Guid AggregateId, string? Name, int Age) : Event(AggregateId);
