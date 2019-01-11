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
            : base(response.Messages != null
                  ? response.Messages.Aggregate("", (p, a) => a + "\r\n" + p, a => a.Trim('\r', '\n'))
                  : response.Title)
        {
            Response = response;
            Title = response.Title;
            Messages = response.Messages;
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
