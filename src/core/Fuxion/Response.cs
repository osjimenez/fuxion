using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Fuxion.Text.Json.Serialization;

namespace Fuxion;

public class Response
	: Response<object?>
{
	internal Response(
		bool isSuccess,
		object? payload,
		string? message = null,
		object? type = null,
		Exception? exception = null)
		: base(isSuccess, payload, message, type, exception) { }
	public Response(
		bool isSuccess,
		string? message = null,
		object? type = null,
		Exception? exception = null)
		: this(isSuccess, isSuccess ? new object() : null, message, type, exception) { }
	public bool TryGetPayload<TPayload>([NotNullWhen(true)] out TPayload payload)
	{
		if (Payload is TPayload tp)
		{
			payload = tp;
			return true;
		}
		payload = default!;
		return false;
	}
	public static IResponseFactory Get => new ResponseFactory();
}

public class Response<TPayload>(
	bool isSuccess,
	TPayload payload,
	string? message = null,
	object? type = null,
	Exception? exception = null)
{
	[MemberNotNullWhen(true, nameof(Payload))]
	public bool IsSuccess { get; } = isSuccess;
	[JsonIgnore]
	[MemberNotNullWhen(false, nameof(Payload))]
	public bool IsError => !IsSuccess;

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Message { get; } = message;

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public TPayload? Payload { get; } = payload;

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public object? ErrorType { get; } = type;
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	[JsonConverter(typeof(ExceptionConverter))]
	public Exception? Exception { get; } = exception;

	[JsonExtensionData]
	public Dictionary<string, object?>? Extensions { get; init; }

	public static implicit operator TPayload?(Response<TPayload> response) => response.Payload;
	public static implicit operator Response<TPayload>(TPayload payload) => new(true, payload);

	public static implicit operator Response(Response<TPayload> response) 
		=> new(
			response.IsSuccess,
			response.Payload,
			response.Message,
			response.ErrorType,
			response.Exception)
		{
			Extensions = response.Extensions
		};
	public static implicit operator Response<TPayload>(Response response) => new(
		response.IsSuccess,
		response.Payload is TPayload payload ? payload : default!,
		response.Message,
		response.ErrorType,
		response.Exception)
	{
		Extensions = response.Extensions
	};
}

public static class ResponseExtensions
{
	public static Response CombineResponses(this IEnumerable<Response<object?>> responses, string? message = null)
	{
		if (responses.Any(r => r.IsError))
			return new Response<IEnumerable<Response<object?>>>(false, responses.Where(r => r.IsError), message);
		return new Response<IEnumerable<Response<object?>>>(true, responses, message);
	}

	// Response.Get helpers
	public static Response Success(this IResponseFactory me, string message)
		=> new(true, message);
	public static Response Success(this IResponseFactory me, string? message = null, params (string Property, object? Value)[] extensions)
		=> new(true, message)
		{
			Extensions = extensions.ToDictionary(t => t.Property, t => t.Value)
		};
	public static Response<TPayload> Success<TPayload>(this IResponseFactory me, TPayload payload, string? message = null, params (string Property, object? Value)[] extensions)
		=> new(true, payload, message)
		{
			Extensions = extensions.ToDictionary(t => t.Property, t => t.Value)
		};

	public static Response Error(this IResponseFactory me, string message, object? type = null, Exception? exception = null, params (string Property, object? Value)[] extensions)
		=> new(false, message, type, exception)
		{
			Extensions = extensions.ToDictionary(t => t.Property, t => t.Value)
		};
	public static Response<TPayload> Error<TPayload>(this IResponseFactory me, string message, TPayload payload, object? type = null, Exception? exception = null, params (string Property, object? Value)[] extensions)
		=> new(false, payload, message, type, exception)
		{
			Extensions = extensions.ToDictionary(t => t.Property, t => t.Value)
		};
	public static bool IsErrorType<TPayload>(this Response<TPayload> res, object type)
		=> res.ErrorType?.Equals(type) == true;

	// Specific error types
	public static Response NotFound(this IErrorResponseFactory me, string message, Exception? exception = null, params (string Property, object? Value)[] extensions)
		=> me.Factory.Error(message, ErrorType.NotFound, exception, extensions);
	public static Response<TPayload> NotFound<TPayload>(this IErrorResponseFactory me, string message, TPayload payload, Exception? exception = null, params (string Property, object? Value)[] extensions)
		=> me.Factory.Error(message, payload, ErrorType.NotFound, exception, extensions);
	public static bool IsNotFound<TPayload>(this Response<TPayload> res)
		=> res.IsErrorType(ErrorType.NotFound);

	public static Response PermissionDenied(this IErrorResponseFactory me, string message, Exception? exception = null, params (string Property, object? Value)[] extensions)
		=> me.Factory.Error(message, ErrorType.PermissionDenied, exception, extensions);
	public static Response<TPayload> PermissionDenied<TPayload>(this IErrorResponseFactory me, string message, TPayload payload, Exception? exception = null, params (string Property, object? Value)[] extensions)
		=> me.Factory.Error(message, payload, ErrorType.PermissionDenied, exception, extensions);
	public static bool IsPermissionDenied<TPayload>(this Response<TPayload> res)
		=> res.IsErrorType(ErrorType.PermissionDenied);

	public static Response InvalidData(this IErrorResponseFactory me, string message, Exception? exception = null, params (string Property, object? Value)[] extensions)
		=> me.Factory.Error(message, ErrorType.InvalidData, exception, extensions);
	public static Response<TPayload> InvalidData<TPayload>(this IErrorResponseFactory me, string message, TPayload payload, Exception? exception = null, params (string Property, object? Value)[] extensions)
		=> me.Factory.Error(message, payload, ErrorType.InvalidData, exception, extensions);
	public static bool IsInvalidData<TPayload>(this Response<TPayload> res)
		=> res.IsErrorType(ErrorType.InvalidData);

	public static Response Conflict(this IErrorResponseFactory me, string message, Exception? exception = null, params (string Property, object? Value)[] extensions)
		=> me.Factory.Error(message, ErrorType.Conflict, exception, extensions);
	public static Response<TPayload> Conflict<TPayload>(this IErrorResponseFactory me, string message, TPayload payload, Exception? exception = null, params (string Property, object? Value)[] extensions)
		=> me.Factory.Error(message, payload, ErrorType.Conflict, exception, extensions);
	public static bool IsConflict<TPayload>(this Response<TPayload> res)
		=> res.IsErrorType(ErrorType.Conflict);

	public static Response Critical(this IErrorResponseFactory me, string message, Exception? exception = null, params (string Property, object? Value)[] extensions)
		=> me.Factory.Error(message, ErrorType.Critical, exception, extensions);
	public static Response<TPayload> Critical<TPayload>(this IErrorResponseFactory me, string message, TPayload payload, Exception? exception = null, params (string Property, object? Value)[] extensions)
		=> me.Factory.Error(message, payload, ErrorType.Critical, exception, extensions);
	public static bool IsCritical<TPayload>(this Response<TPayload> res)
		=> res.IsErrorType(ErrorType.Critical);

	public static Response NotSupported(this IErrorResponseFactory me, string message, Exception? exception = null, params (string Property, object? Value)[] extensions)
		=> me.Factory.Error(message, ErrorType.NotSupported, exception, extensions);
	public static Response<TPayload> NotSupported<TPayload>(this IErrorResponseFactory me, string message, TPayload payload, Exception? exception = null, params (string Property, object? Value)[] extensions)
		=> me.Factory.Error(message, payload, ErrorType.NotSupported, exception, extensions);
	public static bool IsNotSupported<TPayload>(this Response<TPayload> res)
		=> res.IsErrorType(ErrorType.NotSupported);

	public static Response Unavailable(this IErrorResponseFactory me, string message, Exception? exception = null, params (string Property, object? Value)[] extensions)
		=> me.Factory.Error(message, ErrorType.Unavailable, exception, extensions);
	public static Response<TPayload> Unavailable<TPayload>(this IErrorResponseFactory me, string message, TPayload payload, Exception? exception = null, params (string Property, object? Value)[] extensions)
		=> me.Factory.Error(message, payload, ErrorType.Unavailable, exception, extensions);
	public static bool IsUnavailable<TPayload>(this Response<TPayload> res)
		=> res.IsErrorType(ErrorType.Unavailable);
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ErrorType
{
	////Ok,
	////Created,
	//Error,
	//Forbidden,
	//Unauthorized,
	//Invalid,
	//NotFound,
	////NoContent,
	//Conflict,
	//CriticalError,
	//Unavailable,

	NotFound,
	PermissionDenied,
	InvalidData,
	Conflict,
	Critical,
	NotSupported,
	Unavailable
}

public interface IResponseFactory
{
	IErrorResponseFactory Error { get; }
}

internal class ResponseFactory : IResponseFactory
{
	public ResponseFactory()
	{
		Error = new ErrorResponseFactory(this);
	}
	public IErrorResponseFactory Error { get; }
}

public interface IErrorResponseFactory
{
	internal IResponseFactory Factory { get; }
}
internal class ErrorResponseFactory(IResponseFactory factory) : IErrorResponseFactory
{
	public IResponseFactory Factory { get; } = factory;
}