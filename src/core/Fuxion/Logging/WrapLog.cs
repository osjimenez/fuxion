using System;
using System.Collections.Generic;
using System.Linq;
namespace Fuxion.Logging
{
    // TODO - Oscar - Use buffer to send events received before composition
    class WrapLog : ILog
	{
		public WrapLog(Type declaringType)
		{
			this.declaringType = declaringType;
		}
		public WrapLog(string loggerName)
		{
			this.loggerName = loggerName;
		}

		readonly Type declaringType;
		readonly string loggerName;
		ILog log;
		readonly ILog nullLog = new NullLog();
		private ILog ActualLog
		{
			get
			{
				if (log != null)
					return log;
				if (LogManager.factory == null)
					return nullLog;
				else
					return declaringType != null
						? log = LogManager.Create(declaringType)
						: log = LogManager.Create(loggerName);
			}
		}
		#region ILog
		public bool IsEmergencyEnabled { get { return ActualLog.IsEmergencyEnabled; } }
		public void Emergency(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null)
		{
			ActualLog.Emergency(message, title, exception, eventId, priority, categories, extendedProperties);
		}
		public void Emergency(object message, Exception exception)
		{
			ActualLog.Emergency(message, exception);
		}
		public bool IsFatalEnabled { get { return ActualLog.IsFatalEnabled; } }
		public void Fatal(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null)
		{
			ActualLog.Fatal(message, title, exception, eventId, priority, categories, extendedProperties);
		}
		public void Fatal(object message, Exception exception)
		{
			ActualLog.Fatal(message, exception);
		}
		public bool IsAlertEnabled { get { return ActualLog.IsAlertEnabled; } }
		public void Alert(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null)
		{
			ActualLog.Alert(message, title, exception, eventId, priority, categories, extendedProperties);
		}
		public void Alert(object message, Exception exception)
		{
			ActualLog.Alert(message, exception);
		}
		public bool IsCriticalEnabled { get { return ActualLog.IsCriticalEnabled; } }
		public void Critical(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null)
		{
			ActualLog.Critical(message, title, exception, eventId, priority, categories, extendedProperties);
		}
		public void Critical(object message, Exception exception)
		{
			ActualLog.Critical(message, exception);
		}
		public bool IsSevereEnabled { get { return ActualLog.IsSevereEnabled; } }
		public void Severe(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null)
		{
			ActualLog.Severe(message, title, exception, eventId, priority, categories, extendedProperties);
		}
		public void Severe(object message, Exception exception)
		{
			ActualLog.Severe(message, exception);
		}
		public bool IsErrorEnabled { get { return ActualLog.IsErrorEnabled; } }
		public void Error(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null)
		{
			ActualLog.Error(message, title, exception, eventId, priority, categories, extendedProperties);
		}
		public void Error(object message, Exception exception)
		{
			ActualLog.Error(message, exception);
		}
		public bool IsWarnEnabled { get { return ActualLog.IsWarnEnabled; } }
		public void Warn(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null)
		{
			ActualLog.Warn(message, title, exception, eventId, priority, categories, extendedProperties);
		}
		public void Warn(object message, Exception exception)
		{
			ActualLog.Warn(message, exception);
		}
		public bool IsNoticeEnabled { get { return ActualLog.IsNoticeEnabled; } }
		public void Notice(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null)
		{
			ActualLog.Notice(message, title, exception, eventId, priority, categories, extendedProperties);
		}
		public void Notice(object message, Exception exception)
		{
			ActualLog.Notice(message, exception);
		}
		public bool IsInfoEnabled { get { return ActualLog.IsInfoEnabled; } }
		public void Info(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null)
		{
			ActualLog.Info(message, title, exception, eventId, priority, categories, extendedProperties);
		}
		public void Info(object message, Exception exception)
		{
			ActualLog.Info(message, exception);
		}
		public bool IsDebugEnabled { get { return ActualLog.IsDebugEnabled; } }
		public void Debug(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null)
		{
			ActualLog.Debug(message, title, exception, eventId, priority, categories, extendedProperties);
		}
		public void Debug(object message, Exception exception)
		{
			ActualLog.Debug(message, exception);
		}
		public bool IsTraceEnabled { get { return ActualLog.IsTraceEnabled; } }
		public void Trace(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null)
		{
			ActualLog.Trace(message, title, exception, eventId, priority, categories, extendedProperties);
		}
		public void Trace(object message, Exception exception)
		{
			ActualLog.Trace(message, exception);
		}
		public bool IsVervoseEnabled { get { return ActualLog.IsVervoseEnabled; } }
		public void Verbose(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null)
		{
			ActualLog.Verbose(message, title, exception, eventId, priority, categories, extendedProperties);
		}
		public void Verbose(object message, Exception exception)
		{
			ActualLog.Verbose(message, exception);
		}
		public void Dispose() { }
		#endregion
	}
}
