using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using Fuxion.ServiceModel.Dispatcher;

namespace Fuxion.ServiceModel.Description
{
    public class MefServiceBehavior : IServiceBehavior
	{
		private readonly string _name;
		public MefServiceBehavior(string name)
		{
			_name = name;
		}
		public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase host)
		{
			foreach (ChannelDispatcher dispatcher in host.ChannelDispatchers)
				foreach (var endpoint in dispatcher.Endpoints)
					endpoint.DispatchRuntime.InstanceProvider = new MefInstanceProvider(_name);
		}
		public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters) { }
		public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase) { }
	}
}
