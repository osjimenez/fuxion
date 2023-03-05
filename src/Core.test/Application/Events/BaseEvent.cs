using Fuxion.Domain;
using Fuxion.Reflection;

namespace Fuxion.Application.Test.Events;

[TypeKey(nameof(Fuxion),nameof(Application),nameof(Test),nameof(Events), nameof(BaseEvent))]
public record BaseEvent(Guid AggregateId, string? Name, int Age) : Event(AggregateId);