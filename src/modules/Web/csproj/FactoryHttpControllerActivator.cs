#if (NET471)
using Fuxion.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;

namespace Fuxion.Web
{
    public class FactoryHttpControllerActivator : IHttpControllerActivator
    {
        public FactoryHttpControllerActivator(HttpConfiguration configuration) { }

        public IHttpController Create(HttpRequestMessage request
            , HttpControllerDescriptor controllerDescriptor, Type controllerType)
        {
            return Factory.Get(controllerType) as IHttpController;
        }
    }
}
#endif