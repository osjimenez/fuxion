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
                ICryptographicKey currentKey = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(AsymmetricAlgorithm.RsaSignPssSha512).ImportPublicKey(key);
                string validationMessage = null;
                var lics = Store.Query()
                    .Where(l =>
                        CryptographicEngine.VerifySignature(currentKey, Encoding.Unicode.GetBytes(l.Data.ToJson(Formatting.Indented, new JsonSerializerSettings())), Convert.FromBase64String(l.Signature))
                        && 
                        l.Data.ContentIs<T>());
                if (!lics.Any())
                    throw new LicenseValidationException($"Couldn't find any license of type '{typeof(T).Name}'");
                lics.Any(l => l.Data.ContentAs<T>().Validate(out validationMessage));
                if (!Store.Query().Any(l => l.Data.ContentIs<T>()  && l.Data.ContentAs<T>().Validate(out validationMessage)))
                    throw new LicenseValidationException(validationMessage);
                return true;
            }
            catch
            {
                if (throwExceptionIfNotValidate) throw;
                return false;
            }
        }        
        public static LicenseContainer Sign(License content, string key) { return Sign(content, Convert.FromBase64String(key)); }
        public static LicenseContainer Sign(License content, byte[] key)
        {
            ICryptographicKey currentKey = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(AsymmetricAlgorithm.RsaSignPssSha512).ImportKeyPair(key);
            //if (currentKey == null || !isFullKey) throw new InvalidKeyException($"You must provide a full key before sign licenses. Use '{nameof(SetKey)}' method.");
            content.SignatureUtcTime = DateTime.UtcNow;
            var data = new LicenseData(content);
            var sign = CryptographicEngine.Sign(currentKey, Encoding.Unicode.GetBytes(data.ToJson(Formatting.Indented)));
            var lic = new LicenseContainer
            {
                Data = data,
                Signature = Convert.ToBase64String(sign)
            };
            return lic;
        }        
    }


}
