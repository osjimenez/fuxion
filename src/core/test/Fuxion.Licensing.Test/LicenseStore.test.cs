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
using Fuxion.Factories;
using Fuxion.Test;

namespace Fuxion.Licensing.Test
{
    [Collection("Licensing")]
    public class LicenseStoreTest
    {
        public LicenseStoreTest(ITestOutputHelper output)
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
        [Theory]
        [InlineData(new object[] { "Match", Const.HARDWARE_ID, Const.PRODUCT_ID, 10, true })]
        [InlineData(new object[] { "Deactivated", Const.HARDWARE_ID, Const.PRODUCT_ID, 50, false })]
        [InlineData(new object[] { "Expired", Const.HARDWARE_ID, Const.PRODUCT_ID, 400, false })]
        [InlineData(new object[] { "Product not match", Const.HARDWARE_ID, "{105B337E-EBCE-48EA-87A7-852E3A699A98}", 10, false })]
        [InlineData(new object[] { "Hardware not match", "{AE2C70B9-6622-4341-81A0-10EAA078E7DF}", Const.PRODUCT_ID, 10, false })]
        public void OnlyValidLicenses(string _, string hardwareId, string productId, int offsetDays, bool expectedValidation)
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
            var tp = Factory.Get<ITimeProvider>() as MockTimeProvider;
            output.WriteLine($"Current offset is '{tp.Offset}'");
            tp.SetOffset(TimeSpan.FromDays(offsetDays));
            output.WriteLine($"After set offset is '{tp.Offset}'");
            Assert.True(man.Store.Query().OnlyValidOfType<LicenseMock>(Const.PUBLIC_KEY).Count() == (expectedValidation ? 1 : 0));
            tp.SetOffset(TimeSpan.Zero);
            output.WriteLine($"After reset offset is '{tp.Offset}'");
        }
    }
}
