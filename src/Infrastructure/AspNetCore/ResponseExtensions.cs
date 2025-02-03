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
	public static bool IncludeException { get; set; } = true;
	// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses?view=aspnetcore-9.0
	public static IResult ToApiResult<TPayload>(this Response<TPayload> me)
	{
		if (me.IsSuccess)
			if(me.Payload is not null)
				return Results.Ok(me.Payload);
			else if (me.Message is not null)
				return Results.Ok(me.Message);
			else
				return Results.NoContent();

		var extensions = me.Extensions?.ToDictionary();
		if (me.Payload is not null)
		{
			extensions ??= new();
			extensions["payload"] = me.Payload;
		}
		if (IncludeException && me.Exception is not null)
		{
			extensions ??= new();
			extensions["exception"] = me.Exception;
		}

		return me.ErrorType switch
		{
			ErrorType.NotFound
				=> Results.Problem(me.Message, statusCode: StatusCodes.Status404NotFound, title: "Not found", extensions:extensions),
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
			var _ => Results.Problem(me.Message, statusCode: StatusCodes.Status500InternalServerError, title: "Internal server error", extensions: extensions)
		};
	}
	public static IActionResult ToApiActionResult(this Response me)
	{
		if (me.IsSuccess)
			if (me.Payload is not null)
				return new OkObjectResult(me.Payload);
			else
				return new NoContentResult();

		var extensions = me.Extensions?.ToDictionary();
		if (me.Payload is not null)
		{
			extensions ??= new();
			extensions["payload"] = me.Payload;
		}

		return me.ErrorType switch
		{
			ErrorType.NotFound
				=> new ObjectResult(GetProblem(me.Message, StatusCodes.Status404NotFound, "Not found", extensions))
				{
					StatusCode = StatusCodes.Status404NotFound
				},
			ErrorType.PermissionDenied
				=> new ObjectResult(GetProblem(me.Message, StatusCodes.Status403Forbidden, "Forbidden", extensions))
				{
					StatusCode = StatusCodes.Status403Forbidden
				},
			ErrorType.InvalidData
				=> new ObjectResult(GetProblem(me.Message, StatusCodes.Status400BadRequest, "Bad request", extensions))
				{
					StatusCode = StatusCodes.Status400BadRequest
				},
			ErrorType.Conflict
				=> new ObjectResult(GetProblem(me.Message, StatusCodes.Status409Conflict, "Conflict", extensions))
				{
					StatusCode = StatusCodes.Status409Conflict
				},
			ErrorType.Critical
				=> new ObjectResult(GetProblem(me.Message, StatusCodes.Status500InternalServerError, "Internal server error", extensions))
				{
					StatusCode = StatusCodes.Status500InternalServerError
				},
			ErrorType.NotSupported
				=> new ObjectResult(GetProblem(me.Message, StatusCodes.Status501NotImplemented, "Not implemented", extensions))
				{
					StatusCode = StatusCodes.Status501NotImplemented
				},
			var _ => new ObjectResult(GetProblem(me.Message, StatusCodes.Status500InternalServerError, "Internal server error", extensions))
			{
				StatusCode = StatusCodes.Status500InternalServerError
			},
		};
		ProblemDetails GetProblem(string? detail,int? status, string? title, Dictionary<string,object?>? extensions)
		{
			var res = new ProblemDetails
			{
				Detail = detail,
				Status = status,
				Title = title
			};
			if(extensions is not null)
				res.Extensions = extensions;
			return res;
		}
	}
}
