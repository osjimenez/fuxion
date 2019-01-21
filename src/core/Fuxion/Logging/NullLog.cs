using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fuxion.Logging
{
	class NullLog : ILog
	{
		public bool IsEmergencyEnabled { get { return false; } }
		public void Emergency(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null) { }
		public void Emergency(object message, Exception exception) { }
		public bool IsFatalEnabled { get { return false; } }
		public void Fatal(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null) { }
		public void Fatal(object message, Exception exception) { }
		public bool IsAlertEnabled { get { return false; } }
		public void Alert(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null) { }
		public void Alert(object message, Exception exception) { }
		public bool IsCriticalEnabled { get { return false; } }
		public void Critical(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null) { }
		public void Critical(object message, Exception exception) { }
		public bool IsSevereEnabled { get { return false; } }
		public void Severe(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null) { }
		public void Severe(object message, Exception exception) { }
		public bool IsErrorEnabled { get { return false; } }
		public void Error(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null) { }
		public void Error(object message, Exception exception) { }
		public bool IsWarnEnabled { get { return false; } }
		public void Warn(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null) { }
		public void Warn(object message, Exception exception) { }
		public bool IsNoticeEnabled { get { return false; } }
		public void Notice(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null) { }
		public void Notice(object message, Exception exception) { }
		public bool IsInfoEnabled { get { return false; } }
		public void Info(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null) { }
		public void Info(object message, Exception exception) { }
		public bool IsDebugEnabled { get { return false; } }
		public void Debug(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null) { }
		public void Debug(object message, Exception exception) { }
		public bool IsTraceEnabled { get { return false; } }
		public void Trace(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null) { }
		public void Trace(object message, Exception exception) { }
		public bool IsVervoseEnabled { get { return false; } }
		public void Verbose(object message, string title = null, Exception exception = null, int? eventId = null, int? priority = null, ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null) { }
		public void Verbose(object message, Exception exception) { }
		public void Dispose() { }
	}
}
