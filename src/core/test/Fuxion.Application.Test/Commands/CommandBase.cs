namespace Fuxion.Application.Test.Commands;

using Fuxion.Domain;
using Fuxion.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[TypeKey(nameof(BaseCommand))]
public record BaseCommand(Guid Id, string Name) : Command(Id);
