using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Results;
using Newtonsoft.Json;

namespace Fuxion.AspNet;

public static class ResponseExtensions
{
	public static IHttpActionResult ToApiResult<TPayload>(this Response<TPayload> me)
	{
		if (me.IsSuccess)
			return HttpActionResultFactory.Ok(me.Payload);

		var extensions = me.Extensions?.ToDictionary(e=>e.Key, e=>e.Value);
		if (me.Payload is not null)
		{
			extensions ??= new();
			extensions["payload"] = me.Payload;
		}

		return me.ErrorType switch
		{
			ErrorType.NotFound
				=> HttpActionResultFactory.Problem(me.Message, HttpStatusCode.NotFound, "Not found", extensions),
			ErrorType.PermissionDenied
				=> HttpActionResultFactory.Problem(me.Message, HttpStatusCode.Forbidden, "Forbidden", extensions),
			ErrorType.InvalidData
				=> HttpActionResultFactory.Problem(me.Message, HttpStatusCode.BadRequest, "Bad request", extensions),
			ErrorType.Conflict
				=> HttpActionResultFactory.Problem(me.Message, HttpStatusCode.Conflict, "Conflict", extensions),
			ErrorType.Critical
				=> HttpActionResultFactory.Problem(me.Message, HttpStatusCode.InternalServerError, "Internal server error", extensions),
			ErrorType.NotSupported
				=> HttpActionResultFactory.Problem(me.Message, HttpStatusCode.NotImplemented, "Not implemented", extensions),
			ErrorType.Unavailable
				=> HttpActionResultFactory.Problem(me.Message, HttpStatusCode.ServiceUnavailable, "Service unavailable", extensions),
			var _ => HttpActionResultFactory.Problem(me.Message, HttpStatusCode.InternalServerError, "Internal server error", extensions)
		};
	}
}
file class HttpActionResultFactory(HttpStatusCode status, object? payload = null) : IHttpActionResult
{
	public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
		=> payload is null
			? Task.FromResult(new HttpResponseMessage(status))
			: Task.FromResult(new HttpResponseMessage(status)
			{
				Content = new ObjectContent(payload.GetType(), payload, new JsonMediaTypeFormatter()),
			});
	//public static IHttpActionResult Ok() => new HttpActionResultFactory(HttpStatusCode.OK);
	public static IHttpActionResult Ok(object? payload) 
		=> payload is null
			? new HttpActionResultFactory(HttpStatusCode.NoContent, payload)
			: new HttpActionResultFactory(HttpStatusCode.OK, payload);
	public static IHttpActionResult Problem(string? detail, HttpStatusCode statusCode, string title, Dictionary<string,object?>? extensions)
		=> new HttpActionResultFactory(statusCode, new ProblemDetails(statusCode, title, detail)
		{
			Extensions = extensions
		});
}

public class ProblemDetails
{
	public ProblemDetails(HttpStatusCode status, string? title, string? detail)
	{
		Type = GetTypeFromStatusCode(status);
		Title = title;
		Detail = detail;
		Status = (int)status;
		//Payload = payload;
	}
	string GetTypeFromStatusCode(HttpStatusCode status)
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
	string GetTypeFromInt(int status) => GetTypeFromStatusCode((HttpStatusCode)status);

	[JsonProperty("type", DefaultValueHandling = DefaultValueHandling.Ignore)]
	public string? Type { get; set; }
	[JsonProperty("title", DefaultValueHandling = DefaultValueHandling.Ignore)]
	public string? Title { get; set; }
	[JsonProperty("detail", DefaultValueHandling = DefaultValueHandling.Ignore)]
	public string? Detail { get; set; }
	[JsonProperty("status", DefaultValueHandling = DefaultValueHandling.Ignore)]
	public int? Status { get; set; }
	//[JsonProperty("instance", DefaultValueHandling = DefaultValueHandling.Ignore)]
	//public string? Instance { get; set; }
	//[JsonProperty("payload", DefaultValueHandling = DefaultValueHandling.Ignore)]
	//public object? Payload { get; set; }
	[JsonExtensionData]
	public Dictionary<string, object?>? Extensions { get; internal set; }
}