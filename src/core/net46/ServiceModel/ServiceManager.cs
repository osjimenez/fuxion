using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.ServiceModel;
using Fuxion.Factories;
using System.Threading.Tasks;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Fuxion.ServiceModel
{
	public class ServiceManager
	{
        //public CompositionContainer Container { get { return Singleton.Get<CompositionContainer>(); } }
        // TODO - Oscar - Comprobar que la factoria funciona aqui
        public CompositionContainer Container { get { return Factory.Get<CompositionContainer>(false); } }
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
			foreach (var ser in Services)
                if (ser.State != CommunicationState.Opened 
                    && ser.State != CommunicationState.Opening)
                    ser.Open();
		}



        public static void Publish<TService,TContract>() { }
        public static IHost Host<TService>() { return new _Host(typeof(TService)); }
        public static IProxy<TContract> Proxy<TContract>() { return new _Proxy<TContract>(); }
        public static IProxy<TContract> Proxy<TContract>(object callbackInstance) { return new _Proxy<TContract>(callbackInstance); }
    }
    public static class ServiceManagerExtensions
    {
        #region Host
        public static IHost WithEndpoint(this IHost me, Action<IEndpoint> action) {
            var end = new _Endpoint();
            action(end);
            (me as _Host).Endpoints.Add(end);
            return me;
        }
        //public static IHost WithEndpoint2(this IHost me, Action<IContract> contractAction, Action<IBinding> bindingAction,Action<IAddress> addressAction)
        //{
        //    var contract = new _Contract();
        //    contractAction(contract);
        //    var binding = new _Binding();
        //    bindingAction(binding);
        //    var address = new _Address();
        //    addressAction(address);
            
        //    return me;
        //}
        //public static ServiceHost Open(this IHost me) { return null; }
        //public static Task<ServiceHost> OpenAsync(this IHost me) { return null; }
        public static ServiceHost Create(this IHost me) {
            return (me as _Host).ServiceHost;
        }
        public static IHost ConfigureHost(this IHost me, Action<ServiceHost> action)
        {
            action((me as _Host).ServiceHost);
            return me;
        }
        #endregion
        #region Endpoint
        public static IEndpoint WithContract(this IEndpoint me, Func<ContractDescription> contractFunction) {
            var se = (me as _Endpoint);
            var con = contractFunction();
            if (se.ServiceEndpoint == null)
                se.ServiceEndpoint = new ServiceEndpoint(con);
            else
                se.ServiceEndpoint.Contract = con;
            return me;
        }
        public static IEndpoint WithContractOfType<TContract>(this IEndpoint me)
        {
            return me.WithContract(() => ContractDescription.GetContract(typeof(TContract)));
        }
        public static IEndpoint WithTcpBinding(this IEndpoint me, Action<ITcpBinding> action) {
            var bin = new _TcpBinding();
            action(bin);
            (me as _Endpoint).ServiceEndpoint.Binding = bin.Binding;
            return me;
        }
        public static IEndpoint ConfigureEndpoint(this IEndpoint me, Action<ServiceEndpoint> action)
        {
            action((me as _Endpoint).ServiceEndpoint);
            return me;
        }
        public static IEndpoint WithAddress(this IEndpoint me, string url) {
            (me as _Endpoint).ServiceEndpoint.Address = new EndpointAddress(url);
            return me;
        }
        #endregion
        #region Contract
        public static IContract OfType<TContract>(this IContract me) {
            (me as _Contract).ContractDescription = ContractDescription.GetContract(typeof(TContract));
            return me;
        }
        #endregion
        #region Binding
        public static TBinding OpenTimeout<TBinding>(this TBinding me, TimeSpan openTimeout) where TBinding : IBinding
        {
            (me as _Binding).Binding.OpenTimeout = openTimeout;
            return me;
        }
        #endregion
        #region TcpBinding
        public static ITcpBinding MaxBufferSize(this ITcpBinding me, int maxBufferSize) {
            (me as _TcpBinding).Binding.MaxBufferSize = maxBufferSize;
            return me;
        }
        public static ITcpBinding SecurityMode(this ITcpBinding me, SecurityMode mode)
        {
            (me as _TcpBinding).Binding.Security.Mode = mode;
            return me;
        }
        #endregion
        public static IProxy<TContract> WithTcpBinding<TContract>(this IProxy<TContract> me, Action<ITcpBinding> action)
        {
            var bin = new _TcpBinding();
            action(bin);
            (me as _Proxy<TContract>).ChannelFactory.Endpoint.Binding = bin.Binding;
            return me;
        }
        public static IProxy<TContract> WithAddress<TContract>(this IProxy<TContract> me, string url)
        {
            (me as _Proxy<TContract>).ChannelFactory.Endpoint.Address= new EndpointAddress(url);
            return me;
        }
        public static TContract Create<TContract>(this IProxy<TContract> me)
        {
            return (me as _Proxy<TContract>).ChannelFactory.CreateChannel();
        }
    }
    class _Host : IHost {
        public _Host(Type serviceType)
        {
            ServiceHost = new ServiceHost(serviceType);
            Endpoints.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                    foreach (var ep in e.NewItems.Cast<_Endpoint>())
                        ServiceHost.AddServiceEndpoint(ep.ServiceEndpoint);
            };
        }
        public ServiceHost ServiceHost { get; set; }
        public ObservableCollection<_Endpoint> Endpoints { get; set; } = new ObservableCollection<_Endpoint>();
    }
    class _Proxy<TContract> : IProxy<TContract>
    {
        public _Proxy(object callbackInstance)
        {
            ChannelFactory = new DuplexChannelFactory<TContract>(callbackInstance);
        }
        public _Proxy() { }
        public ChannelFactory<TContract> ChannelFactory { get; set; } = new ChannelFactory<TContract>();
    }
    class _Contract : IContract
    {
        public ContractDescription ContractDescription { get; set; }
    }
    class _Endpoint :IEndpoint
    {
        public ServiceEndpoint ServiceEndpoint{ get; set; }
    }
    abstract class _Binding : IBinding
    {
        public Binding Binding { get; set; }
    }
    class _TcpBinding : _Binding, ITcpBinding
    {
        public _TcpBinding()
        {
            base.Binding = Binding;
        }
        public new NetTcpBinding Binding { get; set; } = new NetTcpBinding();
    }
    class _Address : IAddress { }
    public interface IHost { }
    public interface IProxy<TProxy> { }
    public interface IEndpoint { }
    public interface IContract { }
    public interface IBinding { }
    public interface ITcpBinding : IBinding { }
    public interface IAddress { }
}
