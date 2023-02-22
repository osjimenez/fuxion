using Fuxion;
using Fuxion.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Configuration;

namespace Microsoft.Extensions.Logging;

using static ConsoleTools;

public static class ColorConsoleLoggerExtensions
{
	public static ILoggingBuilder AddColorConsoleLogger(this ILoggingBuilder builder)
	{
		builder.AddConfiguration();
		builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, ColorConsoleLoggerProvider>());
		LoggerProviderOptions.RegisterProviderOptions<ColorConsoleLoggerConfiguration, ColorConsoleLoggerProvider>(builder.Services);
		return builder;
	}
	public static ILoggingBuilder AddColorConsoleLogger(this ILoggingBuilder builder, Action<ColorConsoleLoggerConfiguration> configure)
	{
		builder.AddColorConsoleLogger();
		builder.Services.Configure(configure);
		return builder;
	}
	public static void WhiteLine(this ILogger logger, params object[] args) => logger.LogCritical(string.Empty, args);
	public static void LogOk(this ILogger logger, EventId eventId, Exception? exception, string message, params object[] args) =>
		logger.Log(LogLevel.Information, eventId, new ColorConsoleState {
			ForegroundColor = OkColor, Message = message, Arguments = args
		}, exception, (state, _) => state.Message);
	public static void LogOk(this ILogger logger, EventId eventId, string message, params object[] args) => LogOk(logger, eventId, default, message, args);
	public static void LogOk(this ILogger logger, Exception exception, string message, params object[] args) => LogOk(logger, default, exception, message, args);
	public static void LogOk(this ILogger logger, string message, params object[] args) => LogOk(logger, default, default, message, args);
	public static void LogNotice(this ILogger logger, EventId eventId, Exception? exception, string message, params object[] args) =>
		logger.Log(LogLevel.Information, eventId, new ColorConsoleState {
			ForegroundColor = NoticeColor, Message = message, Arguments = args
		}, exception, (state, _) => state.Message);
	public static void LogNotice(this ILogger logger, EventId eventId, string message, params object[] args) => LogNotice(logger, eventId, default, message, args);
	public static void LogNotice(this ILogger logger, Exception exception, string message, params object[] args) => LogNotice(logger, default, exception, message, args);
	public static void LogNotice(this ILogger logger, string message, params object[] args) => LogNotice(logger, default, default, message, args);
	public static void LogHighlight(this ILogger logger, EventId eventId, Exception? exception, string message, params object[] args) =>
		logger.Log(LogLevel.Information, eventId, new ColorConsoleState {
			ForegroundColor = HighlightColor, Message = message, Arguments = args
		}, exception, (state, _) => state.Message);
	public static void LogHighlight(this ILogger logger, EventId eventId, string message, params object[] args) => LogHighlight(logger, eventId, default, message, args);
	public static void LogHighlight(this ILogger logger, Exception exception, string message, params object[] args) => LogHighlight(logger, default, exception, message, args);
	public static void LogHighlight(this ILogger logger, string message, params object[] args) => LogHighlight(logger, default, default, message, args);
}