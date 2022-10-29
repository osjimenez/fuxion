using Fuxion.Domain;
using Fuxion.Reflection;

namespace Fuxion.Application.Test.Commands;

[TypeKey(nameof(BaseCommand))]
public record BaseCommand(Guid Id, string Name) : Command(Id);
