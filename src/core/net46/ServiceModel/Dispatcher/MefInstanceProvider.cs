using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using Fuxion.Logging;
using Fuxion.Factories;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;

namespace Fuxion.ServiceModel.Dispatcher
{
    public class MefInstanceProvider : IInstanceProvider
	{
		public MefInstanceProvider(string name)
		{
			_name = name;
		}
		ILog log = LogManager.Create<MefInstanceProvider>();
		private readonly string _name;
		//internal protected CompositionContainer Container { get { return Singleton.Get<CompositionContainer>(); } }
        // TODO - Oscar - Comprobar que la factoria funciona aqui
        internal protected CompositionContainer Container { get { return Factory.Get<CompositionContainer>(); } }
        public object GetInstance(InstanceContext instanceContext, System.ServiceModel.Channels.Message message)
		{
			var requiredMetadata = new Dictionary<string, Type>();
			requiredMetadata.Add("Name", typeof(string));
			//requiredMetadata.Add(typeof(CreationPolicy).FullName, typeof(CreationPolicy));

		    log.Notice("Creando una instancia del servicio '" + _name + "'");
			var exports = Container.GetExports(new ContractBasedImportDefinition(
				AttributedModelServices.GetContractName(typeof(IMefService)),
				null, //AttributedModelServices.GetTypeIdentity(typeof(IMefService)),
				requiredMetadata,
				ImportCardinality.ZeroOrMore,
				false,
				true,
				CreationPolicy.NonShared));
			var selectedExports = exports.Where(l =>
				((string)l.Metadata["Name"]).Equals(_name, StringComparison.OrdinalIgnoreCase)
				//&&
				//((CreationPolicy)l.Metadata[typeof(CreationPolicy).FullName]) == CreationPolicy.NonShared
				);
			var values = selectedExports.Select(l => l.Value);
			if (values.Count() == 0) throw new ApplicationException("No se puede crear una instancia de servicio con el nombre de contrato '" + _name + "'. No se han encontrado exportaciones de servicios coincidentes.");
			return values.FirstOrDefault();

			//return _container
			//	.GetExports<IMefService, IMefServiceMetadata>()
			//	.Where(l => l.Metadata.Name.Equals(_name, StringComparison.OrdinalIgnoreCase))
			//	.Select(l => l.Value)
			//	.FirstOrDefault();
		}
		public object GetInstance(InstanceContext instanceContext) { return GetInstance(instanceContext, null); }
		public void ReleaseInstance(InstanceContext instanceContext, object instance)
		{
			var disposable = instance as IDisposable;
			if (disposable != null)
				disposable.Dispose();
		}
	}
}