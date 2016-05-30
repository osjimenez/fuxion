using Fuxion.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.ServiceModel.Dispatcher
{
    public class FactoryInstanceProvider : IInstanceProvider
    {
        private Type serviceType;

        public FactoryInstanceProvider(Type serviceType)
        {
            this.serviceType = serviceType;
        }
        public object GetInstance(InstanceContext instanceContext)
        {
            return GetInstance(instanceContext, null);
        }
        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            return Factory.Get(serviceType);
        }
        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
        }
    }
}
