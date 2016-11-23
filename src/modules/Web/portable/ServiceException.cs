using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Web
{
    public class ServiceException : FuxionException
    {
        public ServiceException(ErrorResponse response)
            : base(response.UserMessages != null
                  ? response.UserMessages.Aggregate("", (p, a) => a + "\r\n" + p, a => a.Trim('\r', '\n'))
                  : response.UserMessagesTitle)
        {
            Response = response;
            Title = response.UserMessagesTitle;
            Messages = response.UserMessages;
        }
        public ServiceException(string title, IEnumerable<string> messages)
            : base(messages != null
                  ? messages.Aggregate("", (p, a) => a + "\r\n" + p, a => a.Trim('\r', '\n'))
                  : title)
        {
            Title = title;
            Messages = messages;
        }
        public ErrorResponse Response { get; set; }
        public string Title { get; set; }
        public IEnumerable<string> Messages { get; set; }
    }
}
