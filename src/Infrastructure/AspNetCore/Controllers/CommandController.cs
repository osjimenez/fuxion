using Fuxion.Application.Commands;
using Fuxion.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace Fuxion.AspNetCore.Controllers;

[Route("api/[controller]")]
public class CommandController : ControllerBase
{
	public CommandController(ICommandDispatcher commandDispatcher, TypeKeyDirectory typeKeyDirectory)
	{
		_commandDispatcher = commandDispatcher;
		_typeKeyDirectory = typeKeyDirectory;
	}
	readonly ICommandDispatcher _commandDispatcher;
	readonly TypeKeyDirectory _typeKeyDirectory;
	[HttpPost]
	public async Task<IActionResult> Post([FromBody] CommandPod pod)
	{
		var com = pod.WithTypeKeyDirectory(_typeKeyDirectory)
			?? throw new InvalidCastException($"Command with discriminator '{pod.Discriminator}' is not registered in '{nameof(TypeKeyDirectory)}'");
		await _commandDispatcher.DispatchAsync(com);
		return Ok();
	}
}