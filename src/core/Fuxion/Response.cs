using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Fuxion;


public interface IResponse
{
	bool IsSuccess { get; }
	[JsonIgnore]
#if NETSTANDARD2_0 || NET462
	bool IsError { get; }
#else
	bool IsError => !IsSuccess;
#endif
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	string? Message { get; }
#if NETSTANDARD2_0 || NET462
	internal object? GetPayloadObject();
#else
	internal object? GetPayloadObject() => null;
#endif
}
public interface IResponse<out TPayload> : IResponse
{
#if !NETSTANDARD2_0 && !NET462
	object? IResponse.GetPayloadObject() => Payload;
#endif
	TPayload Payload { get; }
}

public abstract class Response(bool isSuccess, string? message) : IResponse
{
	public bool IsSuccess { get; } = isSuccess;
#if NETSTANDARD2_0 || NET462
	public bool IsError => !IsSuccess;
#endif
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Message { get; } = message;

#if NETSTANDARD2_0 || NET462
	object? IResponse.GetPayloadObject() => null;
#endif

	public static IResponseFactory Get { get; } = null!;
}

public interface IResponseFactory;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ErrorType { NotFound, Invalid, Unauthorized, Forbidden, Conflict, InternalError }
public class SuccessResponse(string? message = null) : Response(true, message);
public class SuccessResponse<TPayload>(TPayload value, string? message = null) : SuccessResponse(message), IResponse<TPayload>
{
	public TPayload Payload { get; } = value;

	public static implicit operator TPayload(SuccessResponse<TPayload> res) => res.Payload;
}
public class ErrorResponse(string message, int? code = null, object? type = null, Exception? exception = null)
	: Response(false, message)
{
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public int? ErrorCode { get; } = code;

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public object? ErrorType { get; } = type;

	[JsonIgnore]
	public Exception? Exception { get; } = exception;
}
public class ErrorResponse<TPayload>(string message, TPayload value, int? code = null, object? type = null, Exception? exception = null)
	: ErrorResponse(message, code, type, exception), IResponse<TPayload>
{
	public TPayload Payload { get; } = value;

	public static implicit operator TPayload(ErrorResponse<TPayload> res) => res.Payload;
}

public static class ResponseExtensions
{
	public static IResponse CombineResponses(this IEnumerable<IResponse> responses)
	{
		foreach (var res in responses)
			if (res.IsError)
				return res;
		if (responses.Any(r => r.IsError))
			return new ErrorResponse<IEnumerable<IResponse>>("Multiple errors", responses.Where(r => r.IsError));
		return new SuccessResponse<IEnumerable<IResponse>>(responses);
	}

	public static SuccessResponse Success(this IResponseFactory me) => new();
	public static SuccessResponse SuccessMessage(this IResponseFactory me, string? message = null) => new(message);
	public static SuccessResponse<TPayload> Success<TPayload>(this IResponseFactory me, TPayload payload, string? message = null) => new(payload, message);

	public static ErrorResponse Error(this IResponseFactory me, string message) => new(message);
	public static ErrorResponse<TPayload> Error<TPayload>(this IResponseFactory me, string message, TPayload payload) => new(message, payload);

	public static bool IsErrorType(this IResponse res, object type) =>
		res is ErrorResponse er && er.ErrorType?.Equals(type) == true;
	// Specific error types

	public static ErrorResponse NotFound(this IResponseFactory me, string message = "Not found") => new(message, type: ErrorType.NotFound);
	public static ErrorResponse<TPayload> NotFound<TPayload>(this IResponseFactory me, TPayload payload, string message = "Not found") => new(message, payload, type: ErrorType.NotFound);
	public static bool IsNotFound(this IResponse res) => res.IsErrorType(ErrorType.NotFound);

	public static ErrorResponse Invalid(this IResponseFactory me, string message = "Invalid") => new(message, type: ErrorType.Invalid);

	public static bool IsInvalid(this IResponse res) => res is ErrorResponse { ErrorType: ErrorType.Invalid };
	public static bool IsUnauthorized(this IResponse res) => res is ErrorResponse { ErrorType: ErrorType.Unauthorized };
	public static bool IsForbidden(this IResponse res) => res is ErrorResponse { ErrorType: ErrorType.Forbidden };
	public static bool IsConflict(this IResponse res) => res is ErrorResponse { ErrorType: ErrorType.Conflict };
	public static bool IsInternalError(this IResponse res) => res is ErrorResponse { ErrorType: ErrorType.InternalError };

	public static object? GetPayload(this IResponse res) => res.GetPayloadObject();
}
