using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Web
{
    public class DebugErrorResponse : ErrorResponse
    {
        public DebugErrorResponse() { }
        internal DebugErrorResponse(Exception ex, string userMessageTitle, IEnumerable<string> userMessages, string debugMessage)
            : base(userMessageTitle, userMessages)
        {
            InnerExceptions = new List<DebugErrorResponse>();
            DebugMessage = debugMessage;
            Type = ex.GetType().FullName;
            Message = ex.Message;
            StackTrace = ex.StackTrace.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            var aux = ex.InnerException;
            while (aux != null)
            {
                InnerExceptions.Add(new DebugErrorResponse(aux, "", null, null));
                aux = aux.InnerException;
            }
        }
        public string DebugMessage { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
        public string[] StackTrace { get; set; }
        public List<DebugErrorResponse> InnerExceptions { get; set; }
    }
}
