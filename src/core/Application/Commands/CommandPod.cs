namespace Fuxion.Application.Commands;

using Fuxion.Application.Events;
using Fuxion.Domain;
using Fuxion.Json;
using Fuxion.Reflection;
using System.Text.Json.Serialization;

[JsonConverter(typeof(CommandPodConverter))]
public class CommandPod : JsonPod<Command, string>
{
	[JsonConstructor]
	protected CommandPod() { }
	internal CommandPod(Command command) : base(command, command.GetType().GetTypeKey()) { }

	public T? AsCommand<T>() where T : Command => As<T>();
	public Command? AsCommand(Type type) => ((Command?)base.As(type));
	public Command? WithTypeKeyDirectory(TypeKeyDirectory typeKeyDirectory) => AsCommand(typeKeyDirectory[PayloadKey]);
}
