using PCLCrypto;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PCLCrypto.WinRTCrypto;
namespace Fuxion.Srm
{
    public class LicensingManager
    {
        public LicensingManager(ILicensingRepository repository, ILicensingProvider provider, IHardwareIdProvider hardwareIdProvider)
        {
            this.repository = repository;
            this.provider = provider;
            this.hardwareIdProvider = hardwareIdProvider;
        }
        ILicensingRepository repository;
        ILicensingProvider provider;
        IHardwareIdProvider hardwareIdProvider;
        ICryptographicKey key;
        bool isFullKey = false;
        public void GenerateKey(out string fullKey, out string publicKey)
        {
            byte[] fullKeyBytes, publicKeyBytes;
            GenerateKey(out fullKeyBytes, out publicKeyBytes);
            fullKey = Convert.ToBase64String(fullKeyBytes);
            publicKey = Convert.ToBase64String(publicKeyBytes);
        }
        public void GenerateKey(out byte[] fullKey, out byte[] publicKey)
        {
            var key = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(AsymmetricAlgorithm.RsaSignPssSha512).CreateKeyPair(2048);
            fullKey = key.Export();
            publicKey = key.ExportPublicKey();
        }
        public void SetKey(string base64key, bool isFullKey = false) { SetKey(Convert.FromBase64String(base64key), isFullKey); }
        public void SetKey(byte[] key, bool isFullKey = false)
        {
            if(isFullKey)
                this.key = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(AsymmetricAlgorithm.RsaSignPssSha512).ImportKeyPair(key);
            else
                this.key = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(AsymmetricAlgorithm.RsaSignPssSha512).ImportPublicKey(key);
            this.isFullKey = isFullKey;
        }
        public IQueryable<License> Query()
        {
            return repository.Query();
        }
        public bool Validate<T>(bool throwExceptionIfNotValidate = false) where T : LicenseContent
        {
            try
            {
                string validationMessage = null;
                var lics = Query().Where(l => l.Data.ContentIs<T>());
                if(!lics.Any())
                    throw new InvalidLicenseException($"Couldn't find any license of type ");
                lics.Any(l => l.Data.ContentAs<T>().Validate(out validationMessage));
                if (!Query().Any(l => l.Data.ContentIs<T>()  && l.Data.ContentAs<T>().Validate(out validationMessage)))
                    throw new InvalidLicenseException(validationMessage);
                return true;
            }
            catch
            {
                if (throwExceptionIfNotValidate) throw;
                return false;
            }
        }
        public License Obtain(LicenseContent content)
        {
            return provider.Create(content);
        }
        public License Sign(LicenseContent content)
        {
            if (key == null || !isFullKey) throw new InvalidKeyException($"You must provide a full key before sign licenses. Use '{nameof(SetKey)}' method.");
            var data = new LicenseData(content);
            var sign = CryptographicEngine.Sign(key, Encoding.Unicode.GetBytes(data.ToJson()));
            var lic = new License
            {
                Data = data,
                Sign = Convert.ToBase64String(sign)
            };
            return lic;
        }
        public void Add(License license)
        {
            repository.Add(license);
        }
        public bool Remove(License license)
        {
            return repository.Remove(license);
        }
    }
    public sealed class LicenseManagerStore
    {

    }
}
