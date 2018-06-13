using DemoWpf.Validation;
using Fuxion;
using Fuxion.Factories;
using Fuxion.Licensing;
using Fuxion.Licensing.Test;
using Fuxion.Licensing.Test.Mocks;
using Fuxion.Logging;
using Fuxion.Security;
using Fuxion.Threading.Tasks;
using Fuxion.Windows.Input;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Fuxion.Windows.Controls;
using DemoWpf.Windows;
using DemoWpf.Repositories;
using DemoWpf.Windows.Controls;
using DemoWpf.Windows.Threading;
using Fuxion.Windows.Threading;
using DemoWpf.ServiceModel;

namespace DemoWpf
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
			Container c = new Container();

			c.Register<ILogFactory, Log4netFactory>();

			c.RegisterSingleton<ILicenseStore>(new JsonFileLicenseStore(new[] { typeof(LicenseMock) }));
            c.RegisterSingleton<ILicenseProvider>(new LicenseProviderMock());
            c.Register<IHardwareIdProvider, HardwareIdHelper>();
            c.RegisterSingleton<LicensingManager>();
			c.Verify();
			Factory.AddInjector(new SimpleInjectorFactoryInjector(c));

			//new MainWindow().Show();
			//new Licensing().Show();
			//new TimeProvider().Show();
			//new ConvertersWindow().Show();
			//new ValidationWindow().Show();
			//new UnhandledExceptionWindowTest().Show();
			//new RepositoriesWindow().Show();
			//new AutoGridTest().Show();
			//new IDispatchableTest().Show();

			new ServiceWindow().Show();
			new ClientWindow().Show();
		}
    }
	public class HardwareIdHelper : IHardwareIdProvider
    {
        public Guid GetId()
        {
            return HardwareId.Motherboard;
        }
    }
    public class LicenseProviderMock : ILicenseProvider
    {
        public LicenseContainer Request(LicenseRequest request)
        {
            return LicenseContainer.Sign(new LicenseMock(), Const.FULL_KEY);
            //if (request is LicenseMock)
            //{
            //    var req = request as LicenseMock;
            //    var lic = new LicenseMock(req.HardwareId, req.ProductId,
            //        req.SaltoEnabled, req.PresenceEnabled, req.VisitsEnabled, req.LogisticEnabled, req.IntrusionEnabled);
            //    //string fullKey, publicKey;
            //    //LicensingManager.GenerateKey(out fullKey, out publicKey);
            //    return LicenseContainer.Sign(lic, FullKey);
            //}
            //throw new NotSupportedException($"License request of type '{request.GetType().Name}' is not supported by this provider");
        }
        public LicenseContainer Refresh(LicenseContainer oldLicense)
        {
            throw new NotImplementedException();
        }
    }
}
