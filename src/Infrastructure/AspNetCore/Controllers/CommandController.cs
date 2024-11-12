using Fuxion.Application.Commands;
using Fuxion.Domain;
using Fuxion.Json;
using Fuxion.Pods;
using Fuxion.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace Fuxion.AspNetCore.Controllers;

[Route("api/[controller]")]
public class CommandController(ICommandDispatcher commandDispatcher, IUriKeyResolver uriKeyResolver) : ControllerBase
{
	[HttpPost]
	public async Task<IActionResult> Post([FromBody] UriKeyPod<Command> pod)
	{
		pod.Resolver ??= uriKeyResolver;
		var com = pod.Payload;
		// var com = pod.WithTypeKeyResolver(typeKeyResolver)
		// 	?? throw new InvalidCastException($"Command with discriminator '{pod.Discriminator}' is not registered in '{nameof(UriKeyDirectory)}'");
		await commandDispatcher.DispatchAsync(com);
		return Ok();
	}
}