using Fuxion.Reflection;
using Fuxion.Application.Commands;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.AspNetCore.Controllers
{
	[Route("api/[controller]")]
	public class CommandController : ControllerBase
	{
		public CommandController(ICommandDispatcher commandDispatcher, TypeKeyDirectory typeKeyDirectory)
		{
			this.commandDispatcher = commandDispatcher;
			this.typeKeyDirectory = typeKeyDirectory;
		}
		ICommandDispatcher commandDispatcher;
		TypeKeyDirectory typeKeyDirectory;
		[HttpPost]
		public async Task<IActionResult> Post([FromBody] dynamic body)
		{
			if (body is JObject jo)
			{
				var pod = jo.ToObject<CommandPod>();
				if (!typeKeyDirectory.ContainsKey(pod.PayloadKey)) return BadRequest($"Command '{pod.PayloadKey}' is not expected");
				var com = pod.WithTypeKeyDirectory(typeKeyDirectory);
				await commandDispatcher.DispatchAsync(com);
				return Ok();
			}
			return BadRequest("Body couldn't be parsed as JSON");
		}
	}
}
