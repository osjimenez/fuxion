using Fuxion.Domain;
using Fuxion.Reflection;

namespace Fuxion.Application.Test.Commands;

[TypeKey(new[]{nameof(Fuxion),nameof(Application),nameof(Test),nameof(Commands), nameof(BaseCommand)})]
public record BaseCommand(Guid Id, string Name) : Command(Id);