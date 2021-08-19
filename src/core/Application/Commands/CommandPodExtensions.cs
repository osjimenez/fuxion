namespace Fuxion.Application.Commands;

using Fuxion.Domain;

public static class CommandPodExtensions
{
	public static CommandPod ToCommandPod(this Command me) => new CommandPod(me);
}