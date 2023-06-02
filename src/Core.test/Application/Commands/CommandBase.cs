using Fuxion.Domain;
using Fuxion.Reflection;

namespace Fuxion.Application.Test.Commands;

[TypeKey(nameof(Fuxion), nameof(Application), nameof(Test), nameof(Commands), nameof(BaseCommand))]
public record BaseCommand(Guid AggregateId, string Name) : Command(AggregateId);