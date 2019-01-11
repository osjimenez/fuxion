using Fuxion.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Web
{
    public class WebApiProxy : Notifier<WebApiProxy>, IWebApiProxy
    {
        public WebApiProxy(IWebApiProxyProvider provider)
        {
            Provider = provider;
        }
        public IWebApiProxyProvider Provider { get; }
    }
}
