using Fuxion.Factories;
using Fuxion.Security;
using Newtonsoft.Json;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static Fuxion.Licensing.LicensingManager;
using static Fuxion.Licensing.LicenseContainer;
using Fuxion.Licensing.Test.Mocks;
using Fuxion.Test;

namespace Fuxion.Licensing.Test
{
    [Collection("Licensing")]
    public class LicensingManagerTest
    {
        public LicensingManagerTest(ITestOutputHelper output)
        {
            this.output = output;
            Factory.RemoveAllInjectors();
            var con = new Container();
            con.RegisterSingleton<ILicenseProvider>(new LicenseProviderMock());
            con.RegisterSingleton<ILicenseStore>(new LicenseStoreMock());
            con.RegisterSingleton<IHardwareIdProvider>(new HardwareIdProviderMock());
            con.RegisterSingleton<ITimeProvider>(new MockTimeProvider());
            con.Register<LicensingManager>();
            Factory.AddInjector(new SimpleInjectorFactoryInjector(con));
        }
        ITestOutputHelper output;
        internal static LicensingManager GetManager(bool withFullKey = false)
        {
            return Factory.Get<LicensingManager>();
        }
        [Fact]
        public void Licensing_GenerateKey()
        {
            var man = Factory.Get<LicensingManager>();
            string fullKey, publicKey;
            GenerateKey(out fullKey, out publicKey);
            output.WriteLine("Full key: " + fullKey);
            output.WriteLine("Public key: " + publicKey);
        }
        [Fact]
        public void License_Request()
        {
            var lic = Factory.Get<LicensingManager>()
                .GetProvider().Request(new LicenseRequestMock
                {
                    HardwareId = Const.HARDWARE_ID,
                    ProductId = Const.PRODUCT_ID
                });
            output.WriteLine($"Created license:");
            output.WriteLine(new[] { lic }.ToJson());
        }
        [Theory]
        [InlineData(new object[] { "Match", Const.HARDWARE_ID, Const.PRODUCT_ID, 10, true })]
        [InlineData(new object[] { "Deactivated", Const.HARDWARE_ID, Const.PRODUCT_ID, 50, false })]
        [InlineData(new object[] { "Expired", Const.HARDWARE_ID, Const.PRODUCT_ID, 400, false })]
        [InlineData(new object[] { "Product not match", Const.HARDWARE_ID, "{105B337E-EBCE-48EA-87A7-852E3A699A98}", 10, false })]
        [InlineData(new object[] { "Hardware not match", "{AE2C70B9-6622-4341-81A0-10EAA078E7DF}", Const.PRODUCT_ID, 10, false })]
        public void Validate(string _, string hardwareId, string productId, int offsetDays, bool expectedValidation)
        {
            // Crete LicensingManager
            var man = Factory.Get<LicensingManager>();
            // Remove all existing licenses
            foreach (var l in man.Store.Query().ToList())
                Assert.True(man.Store.Remove(l), "Failed on remove license");
            // Create license with given parameters
            var lic = man.GetProvider().Request(new LicenseRequestMock
            {
                HardwareId = hardwareId,
                ProductId = productId
            });
            man.Store.Add(lic);
            var tp = Factory.Get<ITimeProvider>(false) as MockTimeProvider;
            tp.SetOffset(TimeSpan.FromDays(offsetDays));
            // Validate content
            if (expectedValidation)
                man.Validate<LicenseMock>(Const.PUBLIC_KEY, true);
            else
            {
                try
                {
                    man.Validate<LicenseMock>(Const.PUBLIC_KEY, true);
                }catch(LicenseValidationException lvex)
                {
                    if(expectedValidation)
                        Assert.False(true, lvex.Message);
                }
            }
            tp.SetOffset(TimeSpan.Zero);
        }
    }
    

    public class LicenseRequestMock : LicenseRequest
    {
        public string HardwareId { get; set; }
        public string ProductId { get; set; }
    }
    public class HardwareIdProviderMock : IHardwareIdProvider
    {
        public Guid GetId() { return Guid.Parse(Const.HARDWARE_ID); }
    }
    public class LicenseProviderMock : ILicenseProvider {
        public LicenseContainer Request(LicenseRequest request) {
            if(request is LicenseRequestMock)
            {
                var req = request as LicenseRequestMock;
                var lic = new LicenseMock();
                lic.SetHarwareId(req.HardwareId);
                lic.SetProductId(req.ProductId);
                return Sign(lic, Const.FULL_KEY);
            }
            throw new NotSupportedException($"License request of type '{request.GetType().Name}' is not supported by this provider");
        }
        public LicenseContainer Refresh(LicenseContainer oldLicense)
        {
            throw new NotImplementedException();
        }
    }
}
