using Fuxion.Factories;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace Fuxion.Web.Test
{
    public class UnitTest1
    {
        [Fact]
        public async Task TestMethod1()
        {
            WebApiProxyProvider pro = new WebApiProxyProvider("https://numerator.azurewebsites.net");
            Factory.AddInjector(new InstanceInjector<IWebApiProxyProvider>(pro));
            var res = await pro.Get<string>("service/version", true);
            Debug.WriteLine("");
        }
    }
}