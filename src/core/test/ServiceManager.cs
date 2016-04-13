using Fuxion.Factories;
using Fuxion.ServiceModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Fuxion.Test
{
    public class ServiceManager
    {
        [Fact]
        public void ServiceManager_Init()
        {
            CompositionContainer container = new CompositionContainer(
                //new DirectoryCatalog(".", "*.dll"),
                new AssemblyCatalog(typeof(TestService).Assembly),
                //Seguro para multihilo
                true,
                //Proveedores de exportación
                new MefServiceHostExportProvider());
            //container.ComposeParts();
            //container.ComposeExportedValue(container);
            Factory.AddToPipe(new InstanceFactory<CompositionContainer>(container));
            var manager = new ServiceModel.ServiceManager();
            Assert.True(manager.Services.Count() == 1);
        }
    }
    [ServiceContract]
    public interface ITestService { }
    [ExportService("WafParking", typeof(TestService), typeof(ITestService))]
    [MefServiceHost]
    public class TestService : ITestService
    {

    }
}
