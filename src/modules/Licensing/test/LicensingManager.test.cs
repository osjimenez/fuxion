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
namespace Fuxion.Licensing.Test
{
    public class LicensingManagerTest
    {
        public LicensingManagerTest(ITestOutputHelper output)
        {
            this.output = output;
        }
        internal const string FULL_KEY = "MIIEywIBADANBgkqhkiG9w0BAQEFAASCBKYwggSiAgEAAoIBAQCXnR6XZhT9AojUUnnc074wg0u08RBYLZ8v5EtaUiGBysH/CIF5gGH8nc0VDGYwH+dSg4+sVnUEn3rSJH5/NkOSuK8QHEqfUalHpQhpuNTuVQrvNAhduHiejgCdi4wMiKQMPSTXXzGA2VaRPnuUHNNxMMyxfF6v1w9BwTiJEW3TecgrXgk/QeQM68qhCpgMCnlZhO1zOjJZWP39cgaUailWXEFmduRx5+UdqsN/IoPCVuBl6UW7J1JhjKdXP/3QclcF12c1/imeb0PfY/WMXSKpr2ra2c6Hyvtor/HMvSdaB9KKllUdKdARs2vpFLfBa1zbY8DM/nLXI0ZfDiABAA+TAgMBAAECggEAHdQ91o+xBW5gOVXYwAew3Z6XYIwlKRQgw61o3eZWzVB1BpZH0v3dSXRKix+bY1CDlIpp1ABWmYg/A+VGNgUZGl7XopXOYLYSVhQ0KYjB6/ozYoicNMQ8hwVtPZHdBgJENFw7SVxjQvR/wafHbIaXWye8vha4EmR/jfJUCzOwpfB8IAAxrmaErabeW6YhSBCn41Q0BeCAlPz15gVdPv7qfVe0RduFGqF8qFpQA2sZnZoMn+CacE2+WN3FyI+jatcw78D9fXwXcPYxYalbrxTqPydK/Vh5u/p6nVjux9wsX1xoo4HVx9Nh1DMsbf0PWn1Vqd1vXnSGIDWthwxmQJnVAQKBgQDUoCN1tPbkCPPpkuCBDmbJqhDylmsKLfV+y9jbWLlJsIrViSDTtSfHwEbNgZqZVWjczrFrOtrtyfOMKBpx+9In4ySsYrHs86qlOwOimxS6k89J9+P+0eCgs5GE2Yodt/sDzgkZjcMOyef5cvS9/fEsnk1CGzFGFaSRHuPjevPQwQKBgQC2iskiwa6ljnDAU0l7DVNRQlUjrmEx/NHeWvMLJEJ+mGSMbyZG769AtV1xAyW8beOvczfOur7b+7NXLUq2tMR7ZYdiG373Ro0j6AZRbMOYaMGM10vq3LTQzb899ZAcBvkm5A1Yl/650tVmw5sqBKoxXckofWN1pVh5WyDhDiqhUwKBgCKZ4Mth5J+dWVwUW9aEqnN86n8fvVGNwxqcP7EKUjTMtvsi8qWhIFUgVCVTRv5W6NauCC9EB6aUq76ONCeCbGOzUh9/fCbsTEPRQ7ktdYUbUqtd3Mt2ChD8x9yPritB6mZnHBH9gNWiQ87TR0K31YyGQlpUpIMcOIipNzqooo7BAoGARYWWFSau3V7ikl0mHzd0A/6/bld7rQ4c4BLDffrRWGWx5cpvSZT/qiVo8SHBvYIctTWsi4+UITQ7mXgmfG9cZaLFbRgLwyGbn5E+1sQVunYrQPi+cTSdqOizbWWY/ROq0KUKcDNzUFJ79CeLcPVV3HdbpZAb0TKn/5A1dYFvuwECgYA1rkOSuBFfrTG94QARPPwgu/AJumPazZihzz684g/mwydijDwmUPT/+JXz3UPYEn/o2W19J/vgXpqzSMSDqIZqOdMQKTDAdWPqYaZxTjda0FDS+nOypwWFKe2G7uUTemNGXhbJvJAcQmnl14rEAJniVCN5IfkHFEGJ7izR4yz2RKANMAsGA1UdDzEEAwIAEA==";
        internal const string PUBLIC_KEY = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAl50el2YU/QKI1FJ53NO+MINLtPEQWC2fL+RLWlIhgcrB/wiBeYBh/J3NFQxmMB/nUoOPrFZ1BJ960iR+fzZDkrivEBxKn1GpR6UIabjU7lUK7zQIXbh4no4AnYuMDIikDD0k118xgNlWkT57lBzTcTDMsXxer9cPQcE4iRFt03nIK14JP0HkDOvKoQqYDAp5WYTtczoyWVj9/XIGlGopVlxBZnbkceflHarDfyKDwlbgZelFuydSYYynVz/90HJXBddnNf4pnm9D32P1jF0iqa9q2tnOh8r7aK/xzL0nWgfSipZVHSnQEbNr6RS3wWtc22PAzP5y1yNGXw4gAQAPkwIDAQAB";
        internal const string HARDWARE_ID = nameof(HARDWARE_ID);
        internal const string PRODUCT_ID = nameof(PRODUCT_ID);

        ITestOutputHelper output;
        internal static LicensingManager GetManager(bool withFullKey = false)
        {
            Container con = new Container();
            con.RegisterSingleton<ILicenseProvider>(new LicenseProviderMock());
            con.RegisterSingleton<ILicenseStore>(new LicenseStoreMock());
            con.RegisterSingleton<IHardwareIdProvider>(new HardwareIdProviderMock());
            con.RegisterSingleton<ITimeProvider>(new TimeProviderMock());
            con.Register<LicensingManager>();
            Factory.AddToPipe(new SimpleInjectorFactory(con));
            var man = Factory.Get<LicensingManager>();
            return man;
        }
        [Fact]
        public void Licensing_GenerateKey()
        {
            var man = GetManager();
            string fullKey, publicKey;
            GenerateKey(out fullKey, out publicKey);
            output.WriteLine("Full key: " + fullKey);
            output.WriteLine("Public key: " + publicKey);
        }
        [Fact]
        public void License_Request()
        {
            var lic = GetManager(true)
                .GetProvider().Request(new LicenseRequestMock
                {
                    HardwareId = HARDWARE_ID,
                    ProductId = PRODUCT_ID
                });
            output.WriteLine($"Created license:");
            output.WriteLine(new[] { lic }.ToJson());
        }
        [Theory]
        [InlineData(new object[] { "Match", HARDWARE_ID, PRODUCT_ID, 10, true })]
        [InlineData(new object[] { "Deactivated", HARDWARE_ID, PRODUCT_ID, 50, false })]
        [InlineData(new object[] { "Expired", HARDWARE_ID, PRODUCT_ID, 400, false })]
        [InlineData(new object[] { "Product not match", HARDWARE_ID, "{105B337E-EBCE-48EA-87A7-852E3A699A98}", 10, false })]
        [InlineData(new object[] { "Hardware not match", "{AE2C70B9-6622-4341-81A0-10EAA078E7DF}", PRODUCT_ID, 10, false })]
        public void Validate(string _, string hardwareId, string productId, int offsetDays, bool expectedValidation)
        {
            // Crete LicensingManager
            var man = GetManager();
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
            var tp = Factory.Get<ITimeProvider>(false) as TimeProviderMock;
            tp.SetOffset(TimeSpan.FromDays(offsetDays));
            // Validate content
            if (expectedValidation)
                man.Validate<LicenseMock>(PUBLIC_KEY, true);
            else
                Assert.Throws(typeof(LicenseValidationException), () => man.Validate<LicenseMock>(PUBLIC_KEY, true));
        }
    }
    public class LicenseMock : License
    {
        public LicenseMock()
        {
            ExpirationUtcTime = new TimeLimitLicenseConstraint(DateTime.UtcNow.AddYears(1));
            DeactivationUtcTime = new TimeLimitLicenseConstraint(DateTime.UtcNow.AddMonths(1));
            HardwareId = new KeyModelLicenseConstraint(null);
            ProductId = new KeyModelLicenseConstraint(null);
        }
        [JsonConstructor]
        public LicenseMock(KeyModelLicenseConstraint hardwareId, KeyModelLicenseConstraint productId) : this()
        {
            HardwareId = hardwareId;
            ProductId = productId;
        }
        public TimeLimitLicenseConstraint DeactivationUtcTime { get; private set; }
        public TimeLimitLicenseConstraint ExpirationUtcTime { get; private set; }
        //[JsonProperty]
        public KeyModelLicenseConstraint HardwareId { get; private set; }
        //[JsonProperty]
        public KeyModelLicenseConstraint ProductId { get; private set; }
        protected override bool Validate(out string validationMessage)
        {
            var res = base.Validate(out validationMessage);
            res = res && ProductId.Validate(out validationMessage, LicensingManagerTest.PRODUCT_ID);
            res = res && HardwareId.Validate(out validationMessage, LicensingManagerTest.HARDWARE_ID);
            res = res && DeactivationUtcTime.Validate(out validationMessage);
            res = res && ExpirationUtcTime.Validate(out validationMessage);
            return res;
        }
        internal void SetHarwareId(string hardwareId)
        {
            HardwareId = new KeyModelLicenseConstraint(hardwareId);
        }
        internal void SetProductId(string productId)
        {
            ProductId = new KeyModelLicenseConstraint(productId);
        }
    }
    public class TimeProviderMock : ITimeProvider
    {
        public DateTime GetUtcNow()
        {
            return DateTime.UtcNow.Add(offset);
        }
        TimeSpan offset;
        public void SetOffset(TimeSpan offset)
        {
            this.offset = offset;
        }
    }
    public class LicenseRequestMock : LicenseRequest
    {
        public string HardwareId { get; set; }
        public string ProductId { get; set; }
    }
    public class HardwareIdProviderMock : IHardwareIdProvider
    {
        public Guid GetId() { return Guid.Parse(LicensingManagerTest.HARDWARE_ID); }
    }
    public class LicenseStoreMock : ILicenseStore
    {
        public LicenseStoreMock()
        {
            licenses = JsonConvert.DeserializeObject<LicenseContainer[]>(File.ReadAllText("licenses.json")).ToList();
        }
        List<LicenseContainer> licenses;
        public IQueryable<LicenseContainer> Query() { return licenses.AsQueryable(); }
        public void Add(LicenseContainer license) { licenses.Add(license); }
        public bool Remove(LicenseContainer license) { return licenses.Remove(license); }
    }
    public class LicenseProviderMock : ILicenseProvider {
        public LicenseContainer Request(LicenseRequest request) {
            if(request is LicenseRequestMock)
            {
                var req = request as LicenseRequestMock;
                var lic = new LicenseMock();
                lic.SetHarwareId(req.HardwareId);
                lic.SetProductId(req.ProductId);
                return Sign(lic, LicensingManagerTest.FULL_KEY);
            }
            throw new NotSupportedException($"License request of type '{request.GetType().Name}' is not supported by this provider");
        }
        public LicenseContainer Refresh(LicenseContainer oldLicense)
        {
            throw new NotImplementedException();
        }
    }
}
