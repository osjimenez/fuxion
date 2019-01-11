using Fuxion.Factories;
using Fuxion.Licensing.Test.Mocks;
using Fuxion.Test;
using Newtonsoft.Json;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
namespace Fuxion.Licensing.Test
{
    [Collection("Licensing")]
    public class LicenseContainerTest
    {
        public LicenseContainerTest(ITestOutputHelper output)
        {
            this.output = output;
            Container con = new Container();
            con.RegisterSingleton<ILicenseProvider>(new LicenseProviderMock());
            con.RegisterSingleton<ILicenseStore>(new LicenseStoreMock());
            con.RegisterSingleton<IHardwareIdProvider>(new HardwareIdProviderMock());
            con.RegisterSingleton<ITimeProvider>(new MockTimeProvider());
            con.Register<LicensingManager>();
            Factory.AddInjector(new SimpleInjectorFactoryInjector(con));
        }
        ITestOutputHelper output;
        [Fact]
        public void LicenseContainer_Serialization()
        {
            var hardwareId = Guid.NewGuid().ToString();
            var productId = Guid.NewGuid().ToString();
            var lic = new LicenseMock();//, DateTime.UtcNow, DateTime.UtcNow);
            lic.SetHarwareId(hardwareId);
            lic.SetProductId(productId);
            var con = new LicenseContainer
            {
                Signature = "signature"
            }.Set(lic);
            output.WriteLine("ToJson:");
            var json = con.ToJson();
            output.WriteLine(json);

            output.WriteLine("FromJson:");
            var con2 = json.FromJson<LicenseContainer>();
            var json2 = con2.ToJson();
            output.WriteLine(json2);
            Assert.Equal(json, json2);
            Assert.True(con2.Is<LicenseMock>());
            var lic2 = con2.As<LicenseMock>();
            Assert.NotNull(lic2);
            Assert.Equal(lic.HardwareId.Key, lic2.HardwareId.Key);
            Assert.Equal(lic.ProductId.Key, lic2.ProductId.Key);

            //Assert.Equal(con2.LicenseAs<LicenseMock>(), lic);

            var time = new Random((int)DateTime.Now.Ticks).Next(500, 1500);
            output.WriteLine("Time: " + time);
            Thread.Sleep(time);
            output.WriteLine("FromJson timed:");
            var con3 = json.FromJson<LicenseContainer>();
            var json3 = con3.ToJson();
            output.WriteLine(json3);

            Assert.Equal(json, json3);
            Assert.Equal(json2, json3);
            Assert.True(con3.Is<LicenseMock>());
            var lic3 = con3.As<LicenseMock>();
            Assert.NotNull(lic3);
            Assert.Equal(lic.HardwareId.Key, lic3.HardwareId.Key);
            Assert.Equal(lic.ProductId.Key, lic3.ProductId.Key);

            //Assert.Equal(con3.LicenseAs<LicenseMock>(), lic);

        }
        [Fact]
        public void LicenseContainer_Signing()
        {
            var hardwareId = Guid.NewGuid().ToString();
            var productId = Guid.NewGuid().ToString();
            var lic = new LicenseMock();
            lic.SetHarwareId(hardwareId);
            lic.SetProductId(productId);
            var con = LicenseContainer.Sign(lic, Const.FULL_KEY);
            output.WriteLine("ToJson:");
            output.WriteLine(con.ToJson());
            output.WriteLine("License.ToJson:");
            output.WriteLine(con.RawLicense.ToJson());
            Assert.True(con.VerifySignature(Const.PUBLIC_KEY));
        }
        [Fact]
        public void LicenseContainer_Comment()
        {
            var hardwareId = Guid.NewGuid().ToString();
            var productId = Guid.NewGuid().ToString();
            var lic = new LicenseMock();
            lic.SetHarwareId(hardwareId);
            lic.SetProductId(productId);
            var con = LicenseContainer.Sign(lic, Const.FULL_KEY);
            con.Comment = "Original comment";
            output.WriteLine("Original ToJson:");
            output.WriteLine(con.ToJson());
            Assert.True(con.VerifySignature(Const.PUBLIC_KEY));
            con.Comment = "Change comment";
            Assert.True(con.VerifySignature(Const.PUBLIC_KEY));
            output.WriteLine("Changed ToJson:");
            output.WriteLine(con.ToJson());
        }
    }
}
