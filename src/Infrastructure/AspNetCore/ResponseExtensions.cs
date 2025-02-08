using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Fuxion.Text.Json.Serialization;
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
			if (me.Payload is not null)
				return Results.Ok(me.Payload);
			else if (me.Message is not null)
				return Results.Content(me.Message);
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
			extensions["exception"] = JsonSerializer.SerializeToElement(me.Exception, options: new(){ Converters = { new ExceptionConverter() }});
		}

		return me.ErrorType switch
		{
			ErrorType.NotFound
				=> Results.Problem(me.Message, statusCode: StatusCodes.Status404NotFound, title: "Not found", extensions: extensions),
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
	public static IActionResult ToApiActionResult<TPayload>(this Response<TPayload> me)
	{
		if (me.IsSuccess)
			if (me.Payload is not null)
				return new OkObjectResult(me.Payload);
			else if (me.Message is not null)
				return new ContentResult
				{
					Content = me.Message,
					ContentType = "text/plain"
				};
			else
				return new NoContentResult();

		var extensions = me.Extensions?.ToDictionary();
		if (me.Payload is not null)
		{
			extensions ??= new();
			extensions["payload"] = me.Payload;
		}
		if (IncludeException && me.Exception is not null)
		{
			extensions ??= new();
			extensions["exception"] = JsonSerializer.SerializeToElement(me.Exception, options: new() { Converters = { new ExceptionConverter() } });
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
		ProblemDetails GetProblem(string? detail, int? status, string? title, Dictionary<string, object?>? extensions)
		{
			var res = new ProblemDetails
			{
				Detail = detail,
				Status = status,
				Title = title,
				Type = status is not null ? GetTypeFromInt(status.Value) : null
			};
			if (extensions is not null)
				res.Extensions = extensions;
			return res;
		}
	}
	static string GetTypeFromInt(int status) => GetTypeFromStatusCode((HttpStatusCode)status);
	static string GetTypeFromStatusCode(HttpStatusCode status)
	{
		return status switch
		{
			HttpStatusCode.Continue => "https://tools.ietf.org/html/rfc7231#section-6.2.1", // 100
			HttpStatusCode.SwitchingProtocols => "https://tools.ietf.org/html/rfc7231#section-6.2.2", // 101

			HttpStatusCode.OK => "https://tools.ietf.org/html/rfc7231#section-6.3.1", // 200
			HttpStatusCode.Created => "https://tools.ietf.org/html/rfc7231#section-6.3.2", // 201
			HttpStatusCode.Accepted => "https://tools.ietf.org/html/rfc7231#section-6.3.3", // 202
			HttpStatusCode.NonAuthoritativeInformation => "https://tools.ietf.org/html/rfc7231#section-6.3.4", // 203
			HttpStatusCode.NoContent => "https://tools.ietf.org/html/rfc7231#section-6.3.5", // 204
			HttpStatusCode.ResetContent => "https://tools.ietf.org/html/rfc7231#section-6.3.6", // 205
			HttpStatusCode.PartialContent => "https://tools.ietf.org/html/rfc7233#section-4.1", // 206

			HttpStatusCode.MultipleChoices => "https://tools.ietf.org/html/rfc7231#section-6.4.1", // 300
			HttpStatusCode.MovedPermanently => "https://tools.ietf.org/html/rfc7231#section-6.4.2", // 301
			HttpStatusCode.Found => "https://tools.ietf.org/html/rfc7231#section-6.4.3", // 302
			HttpStatusCode.SeeOther => "https://tools.ietf.org/html/rfc7231#section-6.4.4", // 303
			HttpStatusCode.NotModified => "https://tools.ietf.org/html/rfc7232#section-4.1", // 304
			HttpStatusCode.UseProxy => "https://tools.ietf.org/html/rfc7231#section-6.4.5", // 305
			HttpStatusCode.Unused => "https://tools.ietf.org/html/rfc7231#section-6.4.6", // 306
			HttpStatusCode.TemporaryRedirect => "https://tools.ietf.org/html/rfc7231#section-6.4.7", // 307

			HttpStatusCode.BadRequest => "https://tools.ietf.org/html/rfc7231#section-6.5.1", // 400
			HttpStatusCode.Unauthorized => "https://tools.ietf.org/html/rfc7235#section-3.1", // 401
			HttpStatusCode.PaymentRequired => "https://tools.ietf.org/html/rfc7231#section-6.5.2", // 402
			HttpStatusCode.Forbidden => "https://tools.ietf.org/html/rfc7231#section-6.5.3", // 403
			HttpStatusCode.NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4", // 404
			HttpStatusCode.MethodNotAllowed => "https://tools.ietf.org/html/rfc7231#section-6.5.5", // 405
			HttpStatusCode.NotAcceptable => "https://tools.ietf.org/html/rfc7231#section-6.5.6", // 406
			HttpStatusCode.ProxyAuthenticationRequired => "https://tools.ietf.org/html/rfc7235#section-3.2", // 407
			HttpStatusCode.RequestTimeout => "https://tools.ietf.org/html/rfc7231#section-6.5.7", // 408
			HttpStatusCode.Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8", // 409
			HttpStatusCode.Gone => "https://tools.ietf.org/html/rfc7231#section-6.5.9", // 410
			HttpStatusCode.LengthRequired => "https://tools.ietf.org/html/rfc7231#section-6.5.10", // 411
			HttpStatusCode.PreconditionFailed => "https://tools.ietf.org/html/rfc7232#section-4.2", // 412
			HttpStatusCode.RequestEntityTooLarge => "https://tools.ietf.org/html/rfc7231#section-6.5.11", // 413
			HttpStatusCode.RequestUriTooLong => "https://tools.ietf.org/html/rfc7231#section-6.5.12", //414
			HttpStatusCode.UnsupportedMediaType => "https://tools.ietf.org/html/rfc7231#section-6.5.13", //415
			HttpStatusCode.RequestedRangeNotSatisfiable => "https://tools.ietf.org/html/rfc7233#section-4.4", // 416
			HttpStatusCode.ExpectationFailed => "https://tools.ietf.org/html/rfc7231#section-6.5.14", // 417
			HttpStatusCode.UpgradeRequired => "https://tools.ietf.org/html/rfc7231#section-6.5.15", // 426

			HttpStatusCode.InternalServerError => "https://tools.ietf.org/html/rfc7231#section-6.6.1", // 500
			HttpStatusCode.NotImplemented => "https://tools.ietf.org/html/rfc7231#section-6.6.2", // 501
			HttpStatusCode.BadGateway => "https://tools.ietf.org/html/rfc7231#section-6.6.3", // 502
			HttpStatusCode.ServiceUnavailable => "https://tools.ietf.org/html/rfc7231#section-6.6.4", // 503
			HttpStatusCode.GatewayTimeout => "https://tools.ietf.org/html/rfc7231#section-6.6.5", // 504
			HttpStatusCode.HttpVersionNotSupported => "https://tools.ietf.org/html/rfc7231#section-6.6.6", // 505
			var _ => throw new NotImplementedException($"Status code '{status}' is not supported")
		};
	}
}
