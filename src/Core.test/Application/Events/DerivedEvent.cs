using Fuxion.Reflection;

namespace Fuxion.Application.Test.Events;

[TypeKey(nameof(Fuxion),nameof(Application),nameof(Test),nameof(Events), nameof(DerivedEvent))]
public record DerivedEvent(Guid AggregateId, string? Name, int Age, string? Nick) : BaseEvent(AggregateId, Name, Age);