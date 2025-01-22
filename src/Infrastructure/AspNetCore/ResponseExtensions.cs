using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fuxion.AspNetCore;
public static class ResponseExtensions
{
	public static IResult ToMinimalApiResult(this IResponse me)
	{
		return Results.Ok();
	}
	public static IActionResult ToControllerApiResult(this IResponse me)
	{
		return new ContentResult();
	}
}
