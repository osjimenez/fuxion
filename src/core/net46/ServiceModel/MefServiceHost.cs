using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
namespace Fuxion.ServiceModel
{
	public class MefServiceHost : ServiceHostBase
	{
		public MefServiceHost(IMefServiceMetadata meta, params Uri[] baseAddresses)
		{
			if (meta == null) throw new ArgumentNullException("meta");
			Meta = meta;
			_baseAddresses = (baseAddresses == null || baseAddresses.Length == 0) ? null : baseAddresses;
			hostAtt = Meta.ServiceType.GetCustomAttributes(typeof(MefServiceHostAttribute), false).Cast<MefServiceHostAttribute>().SingleOrDefault();
			if (hostAtt != null) hostAtt.Host = this;
			endpointAtts = Meta.ServiceType.GetCustomAttributes(typeof(MefEndpointAttribute), true).Cast<MefEndpointAttribute>();
			InitializeDescription(new UriSchemeKeyedCollection(baseAddresses));
		}
		MefServiceHostAttribute hostAtt;
		IEnumerable<MefEndpointAttribute> endpointAtts;
		private static readonly Type HostedServiceType = typeof(IMefService);
		private readonly Uri[] _baseAddresses;
		public IMefServiceMetadata Meta { get; private set; }

		protected override TimeSpan DefaultCloseTimeout
		{
			get
			{
				if (hostAtt != null && hostAtt.DefaultCloseTimeout != TimeSpan.Zero)
					return hostAtt.DefaultCloseTimeout;
				return base.DefaultCloseTimeout;
			}
		}
		protected override TimeSpan DefaultOpenTimeout
		{
			get
			{
				if (hostAtt != null && hostAtt.DefaultOpenTimeout != TimeSpan.Zero)
					return hostAtt.DefaultOpenTimeout;
				return base.DefaultOpenTimeout;
			}
		}
		public override ReadOnlyCollection<ServiceEndpoint> AddDefaultEndpoints()
		{
			if (hostAtt != null) return hostAtt.OnAddDefaultEndpoints(base.AddDefaultEndpoints);
			else return base.AddDefaultEndpoints();
		}
		public override void AddServiceEndpoint(ServiceEndpoint endpoint)
		{
			if (hostAtt != null) hostAtt.OnAddServiceEndpoint(base.AddServiceEndpoint, endpoint);
			else base.AddServiceEndpoint(endpoint);
		}
		protected override void InitializeRuntime()
		{
			if (hostAtt != null) hostAtt.OnInitializeRuntime(base.InitializeRuntime);
			else base.InitializeRuntime();
		}
		protected override ServiceDescription CreateDescription(out IDictionary<string, ContractDescription> implementedContracts)
		{
			//Creo la descripción del servicio
			var serviceDescription = new ServiceDescription { ServiceType = Meta.ServiceType };
			//Creo la implementación de los contratos
			List<ContractDescription> descriptions = new List<ContractDescription>();
			foreach (var contract in Meta.ServiceContracts)
				descriptions.Add(ContractDescription.GetContract(contract, Meta.ServiceType));
			implementedContracts = descriptions.ToDictionary(cd => cd.ConfigurationName, cd => cd);
			//Creo los Endpoints
			foreach (var contractDescription in implementedContracts.Values)
				foreach (var endpointAtt in endpointAtts)
					serviceDescription.Endpoints.Add(endpointAtt.CreateEndpoint(contractDescription, Meta));
			//Me aseguro que el atributo ServiceBehavior esta aplicado
			EnsureServiceBehavior(serviceDescription);

			//foreach (var endpointAtt in endpointAtts)
			//	endpointAtt.UpdateServiceDescription(serviceDescription);

			//Agrego las direcciones base
			AddBaseAddresses(serviceDescription.Endpoints);
			return serviceDescription;
		}
		private void EnsureServiceBehavior(ServiceDescription description)
		{
			var attr = description.Behaviors.Find<ServiceBehaviorAttribute>();
			if (attr == null)
			{
				attr = new ServiceBehaviorAttribute();
                attr.InstanceContextMode = hostAtt.InstanceContextMode;
				attr.IncludeExceptionDetailInFaults = hostAtt.IncludeExceptionDetailInFaults;
			    attr.UseSynchronizationContext = false;
			    attr.ConcurrencyMode = ConcurrencyMode.Multiple;
				description.Behaviors.Insert(0, attr);
			} else
			{
				attr.InstanceContextMode = hostAtt.InstanceContextMode;
			}
		}
		private void AddBaseAddresses(IEnumerable<ServiceEndpoint> endpoints)
		{
			if (_baseAddresses == null)
			{
				var addresses = endpoints
					.Select(se => se.Address.Uri)
					.Distinct();

				foreach (Uri address in addresses)
					AddBaseAddress(address);
			}
		}
	}
}
