using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.ServiceModel;
using Fuxion.Factories;

namespace Fuxion.ServiceModel
{
	public class ServiceManager
	{
        //public CompositionContainer Container { get { return Singleton.Get<CompositionContainer>(); } }
        // TODO - Oscar - Comprobar que la factoria funciona aqui
        public CompositionContainer Container { get { return Factory.Create<CompositionContainer>(); } }
		public ServiceManager()//CompositionContainer container)
		{
			if (Container == null) throw new ArgumentNullException("Container", "El contenedor 'CompositionContainer' debe estar agregado al 'Waf.Sigleton' para que el administrador de servicios pueda crear las instancias mediante MEF.");
			//Busco el proveedor de exportaciónes
			var providers = Container.Providers.Where(p => p is MefServiceHostExportProvider).Cast<MefServiceHostExportProvider>();
			//Me aseguro de que hay un proveedor de exportaciones adecuado y de que solo es uno.
			if (providers.Count() < 1) throw new ArgumentException("El contenedor usado para el '" + GetType().Name + "' debe tener el proveedor de exportación '" + typeof(MefServiceHostExportProvider).Name + "'");
			if (providers.Count() > 1) throw new ArgumentException("Solo puede haber un '" + typeof(MefServiceHostExportProvider).Name + "' en el contenedor usado en el '" + GetType().Name + "'");
			Container.ComposeExportedValue(this);
			Services = Container.GetExportedValues<MefServiceHost>();
		}
		public IEnumerable<MefServiceHost> Services { get; private set; }
		public void OpenAllServices()
		{
			foreach (var ser in Services) if (ser.State != CommunicationState.Opened && ser.State != CommunicationState.Opening) ser.Open();
		}
	}
}
