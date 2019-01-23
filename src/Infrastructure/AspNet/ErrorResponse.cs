using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.AspNet
{
    public class ErrorResponse
    {
        public ErrorResponse() { }
        public ErrorResponse(string userMessageTitle, IEnumerable<string> userMessages)
        {
            Title = userMessageTitle;
            Messages = userMessages;
        }
        public string Title { get; set; }
        public IEnumerable<string> Messages { get; set; }
        public static ErrorResponse Create(Exception ex, string userMessageTitle, IEnumerable<string> userMessages, string debugMessage)
        {
            return ex != null
                ? new DebugErrorResponse(ex, userMessageTitle, userMessages, debugMessage)
                : new ErrorResponse(userMessageTitle, userMessages);
        }
    }
}
