using Fuxion.Factories;
using Newtonsoft.Json;
using PCLCrypto;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static PCLCrypto.WinRTCrypto;
namespace Fuxion.Licensing.Test
{
    public class LicenseContainerTest
    {
        public LicenseContainerTest(ITestOutputHelper output)
        {
            this.output = output;
            Container con = new Container();
            con.RegisterSingleton<ILicenseProvider>(new LicenseProviderMock());
            con.RegisterSingleton<ILicenseStore>(new LicenseStoreMock());
            con.RegisterSingleton<IHardwareIdProvider>(new HardwareIdProviderMock());
            con.RegisterSingleton<ITimeProvider>(new TimeProviderMock());
            con.Register<LicensingManager>();
            Factory.AddToPipe(new SimpleInjectorFactory(con));
        }
        ITestOutputHelper output;
        [Fact]
        public void LicenseContainer_Serialization()
        {
            var hardwareId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var lic = new LicenseMock(hardwareId, productId);//, DateTime.UtcNow, DateTime.UtcNow);
            var con = new LicenseContainer
            {
                Signature = "signature"
            }.SetLicense(lic);
            output.WriteLine("ToJson:");
            var json = con.ToJson();
            output.WriteLine(json);

            output.WriteLine("FromJson:");
            var con2 = json.FromJson<LicenseContainer>();
            var json2 = con2.ToJson();
            output.WriteLine(json2);

            var time = new Random((int)DateTime.Now.Ticks).Next(500, 1500);
            output.WriteLine("Time: " + time);
            Thread.Sleep(time);
            output.WriteLine("FromJson timed:");
            var con3 = json.FromJson<LicenseContainer>();
            var json3 = con3.ToJson();
            output.WriteLine(json3);

            Assert.Equal(json, json2);
            Assert.Equal(json, json3);
            Assert.True(con.LicenseIs<LicenseMock>());
            Assert.NotNull(con.LicenseAs<LicenseMock>());

        }
        [Fact]
        public void LicenseContainer_Serialization2()
        {
            var hardwareId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var lic = new DeactivationLicense();//, DateTime.UtcNow, DateTime.UtcNow);
            var con = new LicenseContainer
            {
                Signature = "signature"
            }.SetLicense(lic);
            output.WriteLine("ToJson:");
            var json = con.ToJson();
            output.WriteLine(json);

            output.WriteLine("FromJson:");
            var con2 = json.FromJson<LicenseContainer>();
            var json2 = con2.ToJson();
            output.WriteLine(json2);

            var time = new Random((int)DateTime.Now.Ticks).Next(500, 1500);
            output.WriteLine("Time: " + time);
            Thread.Sleep(time);
            output.WriteLine("FromJson timed:");
            var con3 = json.FromJson<LicenseContainer>();
            var json3 = con3.ToJson();
            output.WriteLine(json3);

            Assert.Equal(json, json2);
            Assert.Equal(json, json3);
            Assert.True(con.LicenseIs<DeactivationLicense>());
            Assert.NotNull(con.LicenseAs<DeactivationLicense>());

        }
        [Fact]
        public void LicenseContainer_Signing()
        {
            var hardwareId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var lic = new LicenseMock(hardwareId, productId);
            var con = LicenseContainer.Sign(lic, LicensingManagerTest.FULL_KEY);
            output.WriteLine("ToJson:");
            output.WriteLine(con.ToJson());
            output.WriteLine("License.ToJson:");
            output.WriteLine(con.License.ToJson());
            Assert.True(con.VerifySignature(LicensingManagerTest.PUBLIC_KEY));
        }
        [Fact]
        public void LicenseContainer_Comment()
        {
            var hardwareId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var lic = new LicenseMock(hardwareId, productId);
            var con = LicenseContainer.Sign(lic, LicensingManagerTest.FULL_KEY);
            con.Comment = "Original comment";
            output.WriteLine("Original ToJson:");
            output.WriteLine(con.ToJson());
            Assert.True(con.VerifySignature(LicensingManagerTest.PUBLIC_KEY));
            con.Comment = "Change comment";
            Assert.True(con.VerifySignature(LicensingManagerTest.PUBLIC_KEY));
            output.WriteLine("Changed ToJson:");
            output.WriteLine(con.ToJson());
        }
    }
}
