using Fuxion.Reflection;

namespace Fuxion.Application.Test.Events;

[TypeKey(nameof(DerivedEvent))]
public record DerivedEvent(Guid AggregateId, string? Name, int Age, string? Nick) : BaseEvent(AggregateId, Name, Age);