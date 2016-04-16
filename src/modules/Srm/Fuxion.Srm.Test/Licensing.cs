using Fuxion.Security;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Srm.Test
{
    public class Licensing
    {
        // Primera licencia
        // Refresco de licencia
        // Renovación de licencia
        public Licensing(ITestOutputHelper output)
        {
            this.output = output;
        }
        internal const string FULL_KEY = "MIIEywIBADANBgkqhkiG9w0BAQEFAASCBKYwggSiAgEAAoIBAQCXnR6XZhT9AojUUnnc074wg0u08RBYLZ8v5EtaUiGBysH/CIF5gGH8nc0VDGYwH+dSg4+sVnUEn3rSJH5/NkOSuK8QHEqfUalHpQhpuNTuVQrvNAhduHiejgCdi4wMiKQMPSTXXzGA2VaRPnuUHNNxMMyxfF6v1w9BwTiJEW3TecgrXgk/QeQM68qhCpgMCnlZhO1zOjJZWP39cgaUailWXEFmduRx5+UdqsN/IoPCVuBl6UW7J1JhjKdXP/3QclcF12c1/imeb0PfY/WMXSKpr2ra2c6Hyvtor/HMvSdaB9KKllUdKdARs2vpFLfBa1zbY8DM/nLXI0ZfDiABAA+TAgMBAAECggEAHdQ91o+xBW5gOVXYwAew3Z6XYIwlKRQgw61o3eZWzVB1BpZH0v3dSXRKix+bY1CDlIpp1ABWmYg/A+VGNgUZGl7XopXOYLYSVhQ0KYjB6/ozYoicNMQ8hwVtPZHdBgJENFw7SVxjQvR/wafHbIaXWye8vha4EmR/jfJUCzOwpfB8IAAxrmaErabeW6YhSBCn41Q0BeCAlPz15gVdPv7qfVe0RduFGqF8qFpQA2sZnZoMn+CacE2+WN3FyI+jatcw78D9fXwXcPYxYalbrxTqPydK/Vh5u/p6nVjux9wsX1xoo4HVx9Nh1DMsbf0PWn1Vqd1vXnSGIDWthwxmQJnVAQKBgQDUoCN1tPbkCPPpkuCBDmbJqhDylmsKLfV+y9jbWLlJsIrViSDTtSfHwEbNgZqZVWjczrFrOtrtyfOMKBpx+9In4ySsYrHs86qlOwOimxS6k89J9+P+0eCgs5GE2Yodt/sDzgkZjcMOyef5cvS9/fEsnk1CGzFGFaSRHuPjevPQwQKBgQC2iskiwa6ljnDAU0l7DVNRQlUjrmEx/NHeWvMLJEJ+mGSMbyZG769AtV1xAyW8beOvczfOur7b+7NXLUq2tMR7ZYdiG373Ro0j6AZRbMOYaMGM10vq3LTQzb899ZAcBvkm5A1Yl/650tVmw5sqBKoxXckofWN1pVh5WyDhDiqhUwKBgCKZ4Mth5J+dWVwUW9aEqnN86n8fvVGNwxqcP7EKUjTMtvsi8qWhIFUgVCVTRv5W6NauCC9EB6aUq76ONCeCbGOzUh9/fCbsTEPRQ7ktdYUbUqtd3Mt2ChD8x9yPritB6mZnHBH9gNWiQ87TR0K31YyGQlpUpIMcOIipNzqooo7BAoGARYWWFSau3V7ikl0mHzd0A/6/bld7rQ4c4BLDffrRWGWx5cpvSZT/qiVo8SHBvYIctTWsi4+UITQ7mXgmfG9cZaLFbRgLwyGbn5E+1sQVunYrQPi+cTSdqOizbWWY/ROq0KUKcDNzUFJ79CeLcPVV3HdbpZAb0TKn/5A1dYFvuwECgYA1rkOSuBFfrTG94QARPPwgu/AJumPazZihzz684g/mwydijDwmUPT/+JXz3UPYEn/o2W19J/vgXpqzSMSDqIZqOdMQKTDAdWPqYaZxTjda0FDS+nOypwWFKe2G7uUTemNGXhbJvJAcQmnl14rEAJniVCN5IfkHFEGJ7izR4yz2RKANMAsGA1UdDzEEAwIAEA==";
        internal const string PUBLIC_KEY = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAl50el2YU/QKI1FJ53NO+MINLtPEQWC2fL+RLWlIhgcrB/wiBeYBh/J3NFQxmMB/nUoOPrFZ1BJ960iR+fzZDkrivEBxKn1GpR6UIabjU7lUK7zQIXbh4no4AnYuMDIikDD0k118xgNlWkT57lBzTcTDMsXxer9cPQcE4iRFt03nIK14JP0HkDOvKoQqYDAp5WYTtczoyWVj9/XIGlGopVlxBZnbkceflHarDfyKDwlbgZelFuydSYYynVz/90HJXBddnNf4pnm9D32P1jF0iqa9q2tnOh8r7aK/xzL0nWgfSipZVHSnQEbNr6RS3wWtc22PAzP5y1yNGXw4gAQAPkwIDAQAB";
        internal const string HARDWARE_ID = "{BAEA3230-49C0-49C0-B603-7FEE4F945328}";
        internal const string PRODUCT_ID = "{FD20E06D-2711-434A-922A-5E04B22A6742}";
        ITestOutputHelper output;
        internal static LicensingManager GetManager(bool withFullKey = false)
        {
            var rep = new LicensingRepositoryMock();
            var pro = new LicensingProviderMock();
            var har = new HardwareIdProviderMock();
            var man = new LicensingManager(rep, pro, har);
            if (withFullKey)
                man.SetKey(FULL_KEY, true);
            else
                man.SetKey(PUBLIC_KEY);
            return man;
        }
        [Fact]
        public void Licensing_GenerateKey()
        {
            var man = GetManager();
            string fullKey, publicKey;
            man.GenerateKey(out fullKey, out publicKey);
            output.WriteLine("Full key: " + fullKey);
            output.WriteLine("Public key: " + publicKey);
        }
        [Fact]
        public void Licensing_Obtain()
        {
            var lic = GetManager(true)
                .Obtain(new LicenseContentMock
                {
                    HardwareId = Guid.Parse(HARDWARE_ID),
                    ProductId = Guid.Parse(PRODUCT_ID)
                });
            output.WriteLine($"Created license:");
            output.WriteLine(new[] { lic }.ToJson());
        }
        [Theory]
        [InlineData(new object[] { HARDWARE_ID, PRODUCT_ID, true })]
        [InlineData(new object[] { HARDWARE_ID, "{105B337E-EBCE-48EA-87A7-852E3A699A98}", false })]
        [InlineData(new object[] { "{AE2C70B9-6622-4341-81A0-10EAA078E7DF}", PRODUCT_ID, false })]
        [InlineData(new object[] { "{AE2C70B9-6622-4341-81A0-10EAA078E7DF}", "{105B337E-EBCE-48EA-87A7-852E3A699A98}", false })]
        public void Licensing_Validate(string hardwareId, string productId, bool expectedValidation)
        {
            // Crete LicensingManager
            var man = GetManager();
            // Remove all existing licenses
            foreach (var l in man.Query().ToList())
                Assert.True(man.Remove(l), "Failed on remove license");
            // Create license with given parameters
            var lic = man.Obtain(new LicenseContentMock
            {
                HardwareId = Guid.Parse(hardwareId),
                ProductId = Guid.Parse(productId)
            });
            man.Add(lic);
            // Validate content
            if (expectedValidation)
                man.Validate<LicenseContentMock>(true);
            else
                Assert.Throws(typeof(InvalidLicenseException), () => man.Validate<LicenseContentMock>(true));
        }
    }
    public class LicenseContentMock : LicenseContent
    {
        public Guid HardwareId { get; set; }
        public Guid ProductId { get; set; }
        protected override bool Validate(out string validationMessage)
        {
            validationMessage = null;
            if (HardwareId != Guid.Parse(Licensing.HARDWARE_ID))
                validationMessage = "Hardware id not match";
            if (ProductId != Guid.Parse(Licensing.PRODUCT_ID))
                validationMessage = "Product id not match";
            return validationMessage == null;
        }
    }
    public class HardwareIdProviderMock : IHardwareIdProvider
    {
        public Guid GetId() { return Guid.Parse(Licensing.HARDWARE_ID); }
    }
    public class LicensingRepositoryMock : ILicensingRepository
    {
        public LicensingRepositoryMock()
        {
            licenses = JsonConvert.DeserializeObject<License[]>(File.ReadAllText("licenses.json")).ToList();
        }
        List<License> licenses;
        public IQueryable<License> Query() { return licenses.AsQueryable<License>(); }
        public void Add(License license) { licenses.Add(license); }
        public bool Remove(License license) { return licenses.Remove(license); }
    }
    public class LicensingProviderMock : ILicensingProvider
    {
        public License Create(LicenseContent content) { return Licensing.GetManager(true).Sign(content); }
        public License Refresh(License oldLicense)
        {
            throw new NotImplementedException();
        }
    }
}
