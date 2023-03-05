using Fuxion.Application.Commands;
using Fuxion.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace Fuxion.AspNetCore.Controllers;

[Route("api/[controller]")]
public class CommandController : ControllerBase
{
	readonly ICommandDispatcher _commandDispatcher;
	readonly ITypeKeyResolver typeKeyResolver;
	public CommandController(ICommandDispatcher commandDispatcher, ITypeKeyResolver typeKeyResolver)
	{
		_commandDispatcher = commandDispatcher;
		this.typeKeyResolver = typeKeyResolver;
	}
	[HttpPost]
	public async Task<IActionResult> Post([FromBody] CommandPod pod)
	{
		var com = pod.WithTypeKeyResolver(typeKeyResolver)
			?? throw new InvalidCastException($"Command with discriminator '{pod.Discriminator}' is not registered in '{nameof(TypeKeyDirectory)}'");
		await _commandDispatcher.DispatchAsync(com);
		return Ok();
	}
}