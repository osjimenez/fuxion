#if false
using Fuxion.Reflection;

namespace Fuxion.Application.Test.Events;

[UriKey($"{nameof(DerivedEvent)}/1.0.0")]
public record DerivedEvent(Guid AggregateId, string? Name, int Age, string? Nick) : BaseEvent(AggregateId, Name, Age);
#endif