#if false
using Fuxion.Domain;
using Fuxion.Reflection;

namespace Fuxion.Application.Test.Events;

[UriKey(UriKey.FuxionBaseUri + $"test/{nameof(BaseEvent)}/1.0.0")]
public record BaseEvent(Guid AggregateId, string? Name, int Age) : Event(AggregateId);
#endif