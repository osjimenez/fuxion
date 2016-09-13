using Fuxion.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Fuxion.Test.Helpers
{
    public class xUnitLog : ILog
    {
        public xUnitLog(ITestOutputHelper output)
        {
            this.output = output;
        }
        ITestOutputHelper output;
        public bool IsAlertEnabled
        {
            get
            {
                return true;
            }
        }

        public bool IsCriticalEnabled
        {
            get
            {
                return true;
            }
        }

        public bool IsDebugEnabled
        {
            get
            {
                return true;
            }
        }

        public bool IsEmergencyEnabled
        {
            get
            {
                return true;
            }
        }

        public bool IsErrorEnabled
        {
            get
            {
                return true;
            }
        }

        public bool IsFatalEnabled
        {
            get
            {
                return true;
            }
        }

        public bool IsInfoEnabled
        {
            get
            {
                return true;
            }
        }

        public bool IsNoticeEnabled
        {
            get
            {
                return true;
            }
        }

        public bool IsSevereEnabled
        {
            get
            {
                return true;
            }
        }

        public bool IsTraceEnabled
        {
            get
            {
                return true;
            }
        }

        public bool IsVervoseEnabled
        {
            get
            {
                return true;
            }
        }

        public bool IsWarnEnabled
        {
            get
            {
                return true;
            }
        }

        public void Alert(object message, Exception exception)
        {
            output.WriteLine($"ALERT    - {message}");
        }
        public void Alert(object message, string title = null, Exception exception = null, int? eventId = default(int?), int? priority = default(int?), ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null)
        {
            output.WriteLine($"ALERT    - {message}");
        }
        public void Critical(object message, Exception exception)
        {
            output.WriteLine($"CRITICAL - {message}");
        }

        public void Critical(object message, string title = null, Exception exception = null, int? eventId = default(int?), int? priority = default(int?), ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null)
        {
            output.WriteLine($"CRITICAL - {message}");
        }

        public void Debug(object message, Exception exception)
        {
            output.WriteLine($"DEBUG    - {message}");
        }

        public void Debug(object message, string title = null, Exception exception = null, int? eventId = default(int?), int? priority = default(int?), ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null)
        {
            output.WriteLine($"DEBUG    - {message}");
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Emergency(object message, Exception exception)
        {
            output.WriteLine($"EMERGENCY - {message}");
        }

        public void Emergency(object message, string title = null, Exception exception = null, int? eventId = default(int?), int? priority = default(int?), ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null)
        {
            output.WriteLine($"EMERGENCY - {message}");
        }

        public void Error(object message, Exception exception)
        {
            output.WriteLine($"ERROR     - {message}");
        }

        public void Error(object message, string title = null, Exception exception = null, int? eventId = default(int?), int? priority = default(int?), ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null)
        {
            output.WriteLine($"ERROR     - {message}");
        }

        public void Fatal(object message, Exception exception)
        {
            output.WriteLine($"FATAL     - {message}");
        }

        public void Fatal(object message, string title = null, Exception exception = null, int? eventId = default(int?), int? priority = default(int?), ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null)
        {
            output.WriteLine($"FATAL     - {message}");
        }

        public void Info(object message, Exception exception)
        {
            output.WriteLine($"INFO      - {message}");
        }

        public void Info(object message, string title = null, Exception exception = null, int? eventId = default(int?), int? priority = default(int?), ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null)
        {
            output.WriteLine($"INFO      - {message}");
        }

        public void Notice(object message, Exception exception)
        {
            output.WriteLine($"NOTICE    - {message}");
        }

        public void Notice(object message, string title = null, Exception exception = null, int? eventId = default(int?), int? priority = default(int?), ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null)
        {
            output.WriteLine($"NOTICE    - {message}");
        }

        public void Severe(object message, Exception exception)
        {
            output.WriteLine($"SEVERE    - {message}");
        }

        public void Severe(object message, string title = null, Exception exception = null, int? eventId = default(int?), int? priority = default(int?), ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null)
        {
            output.WriteLine($"SEVERE    - {message}");
        }

        public void Trace(object message, Exception exception)
        {
            output.WriteLine($"TRACE     - {message}");
        }

        public void Trace(object message, string title = null, Exception exception = null, int? eventId = default(int?), int? priority = default(int?), ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null)
        {
            output.WriteLine($"TRACE     - {message}");
        }

        public void Verbose(object message, Exception exception)
        {
            output.WriteLine($"VERBOSE   - {message}");
        }

        public void Verbose(object message, string title = null, Exception exception = null, int? eventId = default(int?), int? priority = default(int?), ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null)
        {
            output.WriteLine($"VERBOSE   - {message}");
        }

        public void Warn(object message, Exception exception)
        {
            output.WriteLine($"WARN      - {message}");
        }

        public void Warn(object message, string title = null, Exception exception = null, int? eventId = default(int?), int? priority = default(int?), ICollection<string> categories = null, IDictionary<string, object> extendedProperties = null)
        {
            output.WriteLine($"WARN      - {message}");
        }
    }
}
