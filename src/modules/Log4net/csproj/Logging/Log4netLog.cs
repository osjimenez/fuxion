#if (NET461 || NET462 || NET47)
using System;
using System.Collections.Generic;
using log4net;
using log4net.Core;
using Fuxion.Logging;
namespace Fuxion.Logging
{
	public class Log4netLog : LogImpl, ILog4netLog
	{
		public Log4netLog(ILogger logger) : base(logger) { }
		private readonly static Type ThisDeclaringType = typeof(Log4netLog);
		#region ILog
		public bool IsEmergencyEnabled { get { return Logger.IsEnabledFor(Level.Fine); } }
		public void Emergency(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null)
		{
			LogWithContext(Level.Emergency, message, title, exception, eventId, priority, categories, extendedProperties);
		}
		public void Emergency(object message, Exception exception)
		{
			Logger.Log(ThisDeclaringType, Level.Emergency, message, exception);
		}
		//public bool IsFatalEnabled { get { return ActualLog.IsFatalEnabled; } }
		public void Fatal(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null)
		{
			LogWithContext(Level.Fatal, message, title, exception, eventId, priority, categories, extendedProperties);
		}
		//public void Fatal(object message, Exception exception)
		//{
		//	ActualLog.Fatal(message, exception);
		//}
		public bool IsAlertEnabled { get { return Logger.IsEnabledFor(Level.Alert); } }
		public void Alert(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null)
		{
			LogWithContext(Level.Alert, message, title, exception, eventId, priority, categories, extendedProperties);
		}
		public void Alert(object message, Exception exception)
		{
			Logger.Log(ThisDeclaringType, Level.Alert, message, exception);
		}
		public bool IsCriticalEnabled { get { return Logger.IsEnabledFor(Level.Critical); } }
		public void Critical(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null)
		{
			LogWithContext(Level.Critical, message, title, exception, eventId, priority, categories, extendedProperties);
		}
		public void Critical(object message, Exception exception)
		{
			Logger.Log(ThisDeclaringType, Level.Critical, message, exception);
		}
		public bool IsSevereEnabled { get { return Logger.IsEnabledFor(Level.Severe); } }
		public void Severe(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null)
		{
			LogWithContext(Level.Severe, message, title, exception, eventId, priority, categories, extendedProperties);
		}
		public void Severe(object message, Exception exception)
		{
			Logger.Log(ThisDeclaringType, Level.Severe, message, exception);
		}
		//public bool IsErrorEnabled { get { return ActualLog.IsErrorEnabled; } }
		public void Error(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null)
		{
			LogWithContext(Level.Error, message, title, exception, eventId, priority, categories, extendedProperties);
		}
		//public void Error(object message, Exception exception)
		//{
		//	ActualLog.Error(message, exception);
		//}
		//public bool IsWarnEnabled { get { return ActualLog.IsWarnEnabled; } }
		public void Warn(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null)
		{
			LogWithContext(Level.Warn, message, title, exception, eventId, priority, categories, extendedProperties);
		}
		//public void Warn(object message, Exception exception)
		//{
		//	ActualLog.Warn(message, exception);
		//}
		public bool IsNoticeEnabled { get { return Logger.IsEnabledFor(Level.Notice); } }
		public void Notice(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null)
		{
			LogWithContext(Level.Notice, message, title, exception, eventId, priority, categories, extendedProperties);
		}
		public void Notice(object message, Exception exception)
		{
			Logger.Log(ThisDeclaringType, Level.Notice, message, exception);
		}
		//public bool IsInfoEnabled { get { return ActualLog.IsInfoEnabled; } }
		public void Info(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null)
		{
			LogWithContext(Level.Info, message, title, exception, eventId, priority, categories, extendedProperties);
		}
		//public void Info(object message, Exception exception)
		//{
		//	ActualLog.Info(message, exception);
		//}
		//public bool IsDebugEnabled { get { return ActualLog.IsDebugEnabled; } }
		public void Debug(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null)
		{
			LogWithContext(Level.Debug, message, title, exception, eventId, priority, categories, extendedProperties);
		}
		//public void Debug(object message, Exception exception)
		//{
		//	ActualLog.Debug(message, exception);
		//}
		public bool IsTraceEnabled { get { return Logger.IsEnabledFor(Level.Trace); } }
		public void Trace(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null)
		{
			LogWithContext(Level.Trace, message, title, exception, eventId, priority, categories, extendedProperties);
		}
		public void Trace(object message, Exception exception)
		{
			Logger.Log(ThisDeclaringType, Level.Trace, message, exception);
		}
		public bool IsVervoseEnabled { get { return Logger.IsEnabledFor(Level.Verbose); } }
		public void Verbose(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null)
		{
			LogWithContext(Level.Verbose, message, title, exception, eventId, priority, categories, extendedProperties);
		}
		public void Verbose(object message, Exception exception)
		{
			Logger.Log(ThisDeclaringType, Level.Verbose, message, exception);
		}
		public void Dispose() { }
		#endregion
		public void LogWithContext(Level level, object message, string title, Exception exception, int? eventId, int? priority, ICollection<string> categories, IDictionary<string, object> extendedProperties)
		{
			PushContext(title, eventId, priority, categories, extendedProperties);
			Logger.Log(ThisDeclaringType, level, message, exception);
			PopContext(title, eventId, priority, categories, extendedProperties);
		}
		public void PushContext(string title, int? eventId, int? priority, ICollection<string> categories, IDictionary<string, object> extendedProperties)
		{
			if (title != null) ThreadContext.Stacks["Title"].Push(title);
			if (eventId != null) ThreadContext.Stacks["EventId"].Push(eventId.ToString());
			if (priority != null) ThreadContext.Stacks["Priority"].Push(priority.ToString());
			if (categories != null)
				foreach (var cat in categories)
					ThreadContext.Stacks["Category:" + cat].Push(cat);
			if (extendedProperties != null)
				foreach (var pair in extendedProperties)
					ThreadContext.Stacks[pair.Key].Push(pair.Value.ToString());
		}
		public void PopContext(string title, int? eventId, int? priority, ICollection<string> categories, IDictionary<string, object> extendedProperties)
		{
			if (title != null) ThreadContext.Stacks["Title"].Pop();
			if (eventId != null) ThreadContext.Stacks["EventId"].Pop();
			if (priority != null) ThreadContext.Stacks["Priority"].Pop();
			if (categories != null)
				foreach (var cat in categories)
					ThreadContext.Stacks["Category:" + cat].Pop();
			if (extendedProperties != null)
				foreach (var pair in extendedProperties)
					ThreadContext.Stacks[pair.Key].Pop();
		}
	}
}
#endif