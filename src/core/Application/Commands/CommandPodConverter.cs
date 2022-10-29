using Fuxion.Domain;
using Fuxion.Json;

namespace Fuxion.Application.Commands;

public class CommandPodConverter : JsonPodConverter<CommandPod, Command, string> { }