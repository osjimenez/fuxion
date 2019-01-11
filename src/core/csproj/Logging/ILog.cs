using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fuxion.Logging
{
	public interface ILog : IDisposable
	{
		bool IsEmergencyEnabled { get; }
		void Emergency(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null);
		void Emergency(object message, Exception exception);
		bool IsFatalEnabled { get; }
		void Fatal(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null);
		void Fatal(object message, Exception exception);
		bool IsAlertEnabled { get; }
		void Alert(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null);
		void Alert(object message, Exception exception);
		bool IsCriticalEnabled { get; }
		void Critical(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null);
		void Critical(object message, Exception exception);
		bool IsSevereEnabled { get; }
		void Severe(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null);
		void Severe(object message, Exception exception);
		bool IsErrorEnabled { get; }
		void Error(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null);
		void Error(object message, Exception exception);
		bool IsWarnEnabled { get; }
		void Warn(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null);
		void Warn(object message, Exception exception);
		bool IsNoticeEnabled { get; }
		void Notice(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null);
		void Notice(object message, Exception exception);
		bool IsInfoEnabled { get; }
		void Info(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null);
		void Info(object message, Exception exception);
		bool IsDebugEnabled { get; }
		void Debug(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null);
		void Debug(object message, Exception exception);
		bool IsTraceEnabled { get; }
		void Trace(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null);
		void Trace(object message, Exception exception);
		bool IsVervoseEnabled { get; }
		void Verbose(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null);
		void Verbose(object message, Exception exception);
	}
}
