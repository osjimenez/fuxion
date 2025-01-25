using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Fuxion.AspNetCore;

public static class ResponseExtensions
{
	// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses?view=aspnetcore-9.0
	public static IResult ToApiResult<TPayload>(this Response<TPayload> me)
	{
		if(me.Payload is not null)
		{
			if(me.IsSuccess)
				return Results.Ok(me.Payload);

			var extensions = new Dictionary<string, object?>
			{
				{
					"payload", me.Payload
				}
			};

			return me.ErrorType switch
			{
				ErrorType.NotFound
					=> Results.Problem(me.Message, statusCode: StatusCodes.Status404NotFound, title: "Not found",extensions: extensions),
				ErrorType.PermissionDenied
					=> Results.Problem(me.Message, statusCode: StatusCodes.Status403Forbidden, title: "Forbidden", extensions: extensions),
				ErrorType.InvalidData
					=> Results.Problem(me.Message, statusCode: StatusCodes.Status400BadRequest, title: "Bad request", extensions: extensions),
				ErrorType.Conflict
					=> Results.Problem(me.Message, statusCode: StatusCodes.Status409Conflict, title: "Conflict", extensions: extensions),
				ErrorType.Critical
					=> Results.Problem(me.Message, statusCode: StatusCodes.Status500InternalServerError, title: "Internal server error", extensions: extensions),
				ErrorType.NotSupported
					=> Results.Problem(me.Message, statusCode: StatusCodes.Status501NotImplemented, title: "Not implemented", extensions: extensions),
				ErrorType.Unavailable
					=> Results.Problem(me.Message, statusCode: StatusCodes.Status503ServiceUnavailable, title: "Service unavailable", extensions: extensions),
				var _ => Results.Problem(me.Message, statusCode: StatusCodes.Status500InternalServerError, title: "Internal server error", extensions: extensions)
			};
		}
		if (me.IsSuccess)
			return Results.Ok();

		return me.ErrorType switch
		{
			ErrorType.NotFound
				=> Results.Problem(me.Message, statusCode: StatusCodes.Status404NotFound, title: "Not found"),
			ErrorType.PermissionDenied
				=> Results.Problem(me.Message, statusCode: StatusCodes.Status403Forbidden, title: "Forbidden"),
			ErrorType.InvalidData
				=> Results.Problem(me.Message, statusCode: StatusCodes.Status400BadRequest, title: "Bad request"),
			ErrorType.Conflict
				=> Results.Problem(me.Message, statusCode: StatusCodes.Status409Conflict, title: "Conflict"),
			ErrorType.Critical
				=> Results.Problem(me.Message, statusCode: StatusCodes.Status500InternalServerError, title: "Internal server error"),
			ErrorType.NotSupported
				=> Results.Problem(me.Message, statusCode: StatusCodes.Status501NotImplemented, title: "Not implemented"),
			var _ => Results.Problem(me.Message, statusCode: StatusCodes.Status500InternalServerError, title: "Internal server error")
		};
	}
	public static IActionResult ToControllerApiResult(this Response me)
	{
		return new ContentResult();
	}
}
