
namespace Fuxion.Logging
{
	using Fuxion;
	using Microsoft.Extensions.Logging;
	using Microsoft.Extensions.Options;
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using static Fuxion.ConsoleTools;

	// https://docs.microsoft.com/en-us/dotnet/core/extensions/custom-logging-provider
	public class ColorConsoleLoggerConfiguration
	{
		public int EventId { get; set; }
		public string Title
		{
			get => Console.Title;
			set => Console.Title = value;
		}
		public Func<string> SingleLinePrefix
		{
			get => ConsoleTools.SingleLinePrefix;
			set => ConsoleTools.SingleLinePrefix = value;
		}
		public Func<string> FirstLinePrefix
		{
			get => ConsoleTools.FirstLinePrefix;
			set => ConsoleTools.FirstLinePrefix = value;
		}
		public Func<string> MidLinePrefix
		{
			get => ConsoleTools.MidLinePrefix;
			set => ConsoleTools.MidLinePrefix = value;
		}
		public Func<string> LastLinePrefix
		{
			get => ConsoleTools.LastLinePrefix;
			set => ConsoleTools.LastLinePrefix = value;
		}
		public Func<string> MessagePrefix
		{
			get => ConsoleTools.MessagePrefix;
			set => ConsoleTools.MessagePrefix = value;
		}

		public Func<string> FirstLineSufix
		{
			get => ConsoleTools.FirstLineSufix;
			set => ConsoleTools.FirstLineSufix = value;
		}
		public Func<string> MidLineSufix
		{
			get => ConsoleTools.MidLineSufix;
			set => ConsoleTools.MidLineSufix = value;
		}
		public Func<string> LastLineSufix
		{
			get => ConsoleTools.LastLineSufix;
			set => ConsoleTools.LastLineSufix = value;
		}
		public Func<string> MessageSufix
		{
			get => ConsoleTools.MessageSufix;
			set => ConsoleTools.MessageSufix = value;
		}

		public bool UseLock { get; set; }

		public Dictionary<LogLevel, Func<ConsoleColor>> LogLevels { get; set; } = new()
		{
			[LogLevel.Critical] = () => CriticalColor,
			[LogLevel.Error] = () => ErrorColor,
			[LogLevel.Warning] = () => WarnColor,
			[LogLevel.Information] = () => InfoColor,
			[LogLevel.Debug] = () => DebugColor,
			[LogLevel.Trace] = () => TraceColor,
			[LogLevel.None] = () => ConsoleColor.Black,
		};
	}
	public class ColorConsoleLogger : ILogger
	{
		private readonly string _name;
		private readonly ColorConsoleLoggerConfiguration _config;

		public ColorConsoleLogger(
			string name,
			ColorConsoleLoggerConfiguration config) =>
			(_name, _config) = (name, config);

		public IDisposable BeginScope<TState>(TState state) => default!;

		public bool IsEnabled(LogLevel logLevel) =>
			_config.LogLevels.ContainsKey(logLevel);

		private static readonly object lockObject = new();
		public void Log<TState>(
			LogLevel logLevel,
			EventId eventId,
			TState state,
			Exception? exception,
			Func<TState, Exception?, string> formatter)
		{
			if (!IsEnabled(logLevel))
			{
				return;
			}

			if (_config.EventId == 0 || _config.EventId == eventId.Id)
			{
				var message = formatter(state, exception);
				var foreground = state switch
				{
					ColorConsoleState s => s.ForegroundColor,
					_ => _config.LogLevels[logLevel]()
				};
				if (_config.UseLock)
					lock (lockObject)
					{
						WriteLine(message, foreground);
					}
				else
					WriteLine(message, foreground);
			}
		}
	}
	public sealed class ColorConsoleState
	{
		public ConsoleColor ForegroundColor { get; set; }
		public ConsoleColor BackgroundColor { get; set; }
		public string Message { get; set; } = string.Empty;
		public object[] Arguments { get; set; } = Array.Empty<object>();
	}
	public sealed class ColorConsoleLoggerProvider : ILoggerProvider
	{
		private readonly IDisposable _onChangeToken;
		private ColorConsoleLoggerConfiguration _currentConfig;
		private readonly ConcurrentDictionary<string, ColorConsoleLogger> _loggers = new();

		public ColorConsoleLoggerProvider(
			IOptionsMonitor<ColorConsoleLoggerConfiguration> config)
		{
			_currentConfig = config.CurrentValue;
			_onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
		}

		public ILogger CreateLogger(string categoryName) =>
			_loggers.GetOrAdd(categoryName, name => new ColorConsoleLogger(name, _currentConfig));

		public void Dispose()
		{
			_loggers.Clear();
			_onChangeToken.Dispose();
		}
	}
}
namespace Microsoft.Extensions.Logging
{
	using Fuxion.Logging;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.DependencyInjection.Extensions;
	using Microsoft.Extensions.Logging.Configuration;
	using System;
	using static Fuxion.ConsoleTools;

	public static class ColorConsoleLoggerExtensions
	{
		public static ILoggingBuilder AddColorConsoleLogger(
			   this ILoggingBuilder builder)
		{
			builder.AddConfiguration();

			builder.Services.TryAddEnumerable(
				ServiceDescriptor.Singleton<ILoggerProvider, ColorConsoleLoggerProvider>());

			LoggerProviderOptions.RegisterProviderOptions
				<ColorConsoleLoggerConfiguration, ColorConsoleLoggerProvider>(builder.Services);

			return builder;
		}
		public static ILoggingBuilder AddColorConsoleLogger(
			this ILoggingBuilder builder,
			Action<ColorConsoleLoggerConfiguration> configure)
		{
			builder.AddColorConsoleLogger();
			builder.Services.Configure(configure);

			return builder;
		}

		public static void WhiteLine(this ILogger logger, params object[] args) => logger.LogCritical(string.Empty, args);

		public static void LogOk(this ILogger logger, EventId eventId, Exception? exception, string message, params object[] args)
			=> logger.Log(LogLevel.Information, eventId, new ColorConsoleState
			{
				ForegroundColor = OkColor,
				Message = message,
				Arguments = args
			}, exception, (state, _) => state.Message);
		public static void LogOk(this ILogger logger, EventId eventId, string message, params object[] args)
			=> LogOk(logger, eventId, default, message, args);
		public static void LogOk(this ILogger logger, Exception exception, string message, params object[] args)
			=> LogOk(logger, default, exception, message, args);
		public static void LogOk(this ILogger logger, string message, params object[] args)
			=> LogOk(logger, default, default, message, args);

		public static void LogNotice(this ILogger logger, EventId eventId, Exception? exception, string message, params object[] args)
			=> logger.Log(LogLevel.Information, eventId, new ColorConsoleState
			{
				ForegroundColor = NoticeColor,
				Message = message,
				Arguments = args
			}, exception, (state, _) => state.Message);
		public static void LogNotice(this ILogger logger, EventId eventId, string message, params object[] args)
			=> LogNotice(logger, eventId, default, message, args);
		public static void LogNotice(this ILogger logger, Exception exception, string message, params object[] args)
			=> LogNotice(logger, default, exception, message, args);
		public static void LogNotice(this ILogger logger, string message, params object[] args)
			=> LogNotice(logger, default, default, message, args);

		public static void LogHighlight(this ILogger logger, EventId eventId, Exception? exception, string message, params object[] args)
			=> logger.Log(LogLevel.Information, eventId, new ColorConsoleState
			{
				ForegroundColor = HighlightColor,
				Message = message,
				Arguments = args
			}, exception, (state, _) => state.Message);
		public static void LogHighlight(this ILogger logger, EventId eventId, string message, params object[] args)
			=> LogHighlight(logger, eventId, default, message, args);
		public static void LogHighlight(this ILogger logger, Exception exception, string message, params object[] args)
			=> LogHighlight(logger, default, exception, message, args);
		public static void LogHighlight(this ILogger logger, string message, params object[] args)
			=> LogHighlight(logger, default, default, message, args);
	}
}
