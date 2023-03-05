using Fuxion.Domain;
using Fuxion.Json;
using Fuxion.Reflection;

namespace Fuxion.Application.Commands;

public class CommandPodConverter : JsonPodConverter<CommandPod, TypeKey, Command> { }