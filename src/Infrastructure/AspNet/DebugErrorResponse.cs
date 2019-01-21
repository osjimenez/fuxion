using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.AspNet
{
    public class DebugErrorResponse : ErrorResponse
    {
        public DebugErrorResponse() { }
        internal DebugErrorResponse(Exception ex, string title, IEnumerable<string> messages, string debugMessage)
            : base(title, messages)
        {
            InnerExceptions = new List<DebugErrorResponse>();
            DebugMessage = debugMessage;
            ExceptionType = ex.GetType().FullName;
            ExceptionMessage = ex.Message;
            ExceptionStackTrace = ex.StackTrace.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            var aux = ex.InnerException;
            while (aux != null)
            {
                InnerExceptions.Add(new DebugErrorResponse(aux, title, messages, debugMessage));
                aux = aux.InnerException;
            }
        }
        public string DebugMessage { get; set; }
        public string ExceptionType { get; set; }
        public string ExceptionMessage { get; set; }
        public string[] ExceptionStackTrace { get; set; }
        public List<DebugErrorResponse> InnerExceptions { get; set; }
    }
}
