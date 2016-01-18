using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

namespace Fuxion.ServiceModel
{
	//public enum HttpBindingType { BasicHttp, WSHttp }
	//[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	//public class HttpEndpointAttribute : MefEndpointAttribute
	//{
	//	private const int DefaultPort = 50001;

	//	public HttpEndpointAttribute() : base(DefaultPort) { EnableGet = true; }
	//	public HttpBindingType BindingType { get; set; }
	//	public bool UseHttps { get; set; }
	//	public bool EnableGet { get; set; }
	//	protected internal override ServiceEndpoint CreateEndpoint(ContractDescription description, IMefServiceMetadata meta)
	//	{
	//		var uri = CreateUri((UseHttps) ? "https" : "http", meta);
	//		var address = new EndpointAddress(uri);

	//		var binding = CreateBinding(BindingType);
	//		return new ServiceEndpoint(description, binding, address);
	//	}
	//	protected virtual Binding CreateBinding(HttpBindingType bindingType)
	//	{
	//		switch (bindingType)
	//		{
	//			case HttpBindingType.BasicHttp:
	//				return (BindingConfiguration == null)
	//						   ? new BasicHttpBinding()
	//						   : new BasicHttpBinding(BindingConfiguration);
	//			case HttpBindingType.WSHttp:
	//				return (BindingConfiguration == null)
	//						   ? new WSHttpBinding()
	//						   : new WSHttpBinding(BindingConfiguration);
	//			default:
	//				throw new ArgumentNullException("Unsupported binding type: " + bindingType);
	//		}
	//	}
	//	private static ServiceMetadataBehavior EnsureServiceMetadataBehavior(ServiceDescription description)
	//	{
	//		var behaviour = description.Behaviors
	//			.OfType<ServiceMetadataBehavior>()
	//			.SingleOrDefault();

	//		if (behaviour == null)
	//		{
	//			behaviour = new ServiceMetadataBehavior();
	//			description.Behaviors.Add(behaviour);
	//		}

	//		return behaviour;
	//	}
	//	protected internal override void UpdateServiceDescription(ServiceDescription description)
	//	{
	//		var metaBehavior = EnsureServiceMetadataBehavior(description);
	//		if (EnableGet)
	//		{
	//			if (UseHttps)
	//				metaBehavior.HttpsGetEnabled = true;
	//			else
	//				metaBehavior.HttpGetEnabled = true;
	//		}
	//	}
	//}
}
