using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    public class LicenseContainer
    {
        public string Comment { get; set; }
        public string Signature { get; set; }
        public JRaw License { get; set; }
        public LicenseContainer SetLicense(License license) { License = new JRaw(license.ToJson(Formatting.None)); return this; }
        public T LicenseAs<T>() where T : License
        {
            return License.Value.ToString().FromJson<T>();
        }
        public bool LicenseIs<T>() where T : License
        {
            try
            {
                License.Value.ToString().FromJson<T>();
                return true;
            }
            catch(Exception ex)
            {
                Debug.WriteLine("" + ex.Message);
                return false;
            }
        }
        public static LicenseContainer Sign(License content, string key) { return Sign(content, Convert.FromBase64String(key)); }
        public static LicenseContainer Sign(License license, byte[] key)
        {
            license.SignatureUtcTime = DateTime.UtcNow;
            var sign = CryptographicEngine.Sign(
                AsymmetricKeyAlgorithmProvider.OpenAlgorithm(AsymmetricAlgorithm.RsaSignPssSha512).ImportKeyPair(key),
                Encoding.Unicode.GetBytes(license.ToJson(Formatting.None)));
            return new LicenseContainer
            {
                Signature = Convert.ToBase64String(sign),
            }.SetLicense(license);
        }
        public bool VerifySignature(string key) { return VerifySignature(Convert.FromBase64String(key)); }
        public bool VerifySignature(byte[] key)
        {
            Debug.WriteLine("VERIFICATION:"+Signature);
            Debug.WriteLine(License.ToJson(Formatting.None));
            return CryptographicEngine.VerifySignature(
                AsymmetricKeyAlgorithmProvider.OpenAlgorithm(AsymmetricAlgorithm.RsaSignPssSha512).ImportPublicKey(key),
                Encoding.Unicode.GetBytes(License.ToJson(Formatting.None)),
                Convert.FromBase64String(Signature));
        }
    }
}
