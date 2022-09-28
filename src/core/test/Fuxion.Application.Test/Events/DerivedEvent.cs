namespace Fuxion.Application.Test.Events;
using Fuxion.Reflection;

[TypeKey(nameof(DerivedEvent))]
public record DerivedEvent(Guid AggregateId, string? Name, int Age, string? Nick) : BaseEvent(AggregateId, Name, Age);