using Fuxion.Factories;
using Newtonsoft.Json;
using PCLCrypto;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PCLCrypto.WinRTCrypto;
namespace Fuxion.Licensing
{
    public class LicensingManager
    {
        public LicensingManager(ILicenseStore store)
        {
            Store = store;
        }
        public ILicenseStore Store { get; set; }
        public ILicenseProvider GetProvider() { return Factory.Get<ILicenseProvider>(false); }
        public static void GenerateKey(out string fullKey, out string publicKey)
        {
            byte[] fullKeyBytes, publicKeyBytes;
            GenerateKey(out fullKeyBytes, out publicKeyBytes);
            fullKey = Convert.ToBase64String(fullKeyBytes);
            publicKey = Convert.ToBase64String(publicKeyBytes);
        }
        public static void GenerateKey(out byte[] fullKey, out byte[] publicKey)
        {
            var key = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(AsymmetricAlgorithm.RsaSignPssSha512).CreateKeyPair(2048);
            fullKey = key.Export();
            publicKey = key.ExportPublicKey();
        }
        // TODO - Oscar - Implement server-side validation and try to avoid public key hardcoding violation
        public bool Validate<T>(string key, bool throwExceptionIfNotValidate = false) where T : License
        {
            return Validate<T>(Convert.FromBase64String(key), throwExceptionIfNotValidate);
        }
        public bool Validate<T>(byte[] key, bool throwExceptionIfNotValidate = false) where T : License
        {
            try
            {
                string validationMessage = null;
                var cons = Store.Query()
                    .Where(c =>
                        c.VerifySignature(key)
                        && 
                        c.LicenseIs<T>());
                if (!cons.Any())
                    throw new LicenseValidationException($"Couldn't find any license of type '{typeof(T).Name}'");
                //cons.Any(l => l.LicenseAs<T>().Validate(out validationMessage));
                //var o1 = Store.Query().First().LicenseIs<T>();
                //var oo = Store.Query().First().LicenseAs<T>();
                //var o2 = oo.Validate(out validationMessage);
                if (!Store.Query().Any(l => l.LicenseIs<T>()  && l.LicenseAs<T>().Validate(out validationMessage)))
                    throw new LicenseValidationException(validationMessage);
                return true;
            }
            catch
            {
                if (throwExceptionIfNotValidate) throw;
                return false;
            }
        }       
    }
}
