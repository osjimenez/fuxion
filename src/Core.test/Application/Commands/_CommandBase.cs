#if false
using Fuxion.Domain;
using Fuxion.Reflection;

namespace Fuxion.Application.Test.Commands;
[UriKey(UriKey.FuxionBaseUri + $"test/{nameof(BaseCommand)}/1.0.0")]
public record BaseCommand(Guid AggregateId, string Name) : Command(AggregateId);
#endif