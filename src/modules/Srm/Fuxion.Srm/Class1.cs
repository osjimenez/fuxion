using PCLCrypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static PCLCrypto.WinRTCrypto;
namespace Fuxion.Srm
{
    public class Class1
    {
        private static string GenerateHash(string input, string key)
        {
            
            var k = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(AsymmetricAlgorithm.RsaSignPssSha512).CreateKeyPair(2048);
            var result = CryptographicEngine.Sign(k, new byte[] { 0xFF });

            return null;
            //AsymmetricKeyAlgorithmProvider.OpenAlgorithm(AsymmetricAlgorithm.RsaSignPssSha512).Algorithm.
            //var mac = WinRTCrypto.MacAlgorithmProvider.OpenAlgorithm(MacAlgorithm.HmacSha1);
            //var keyMaterial = WinRTCrypto.CryptographicBuffer.ConvertStringToBinary(key, Encoding.UTF8);
            //var cryptoKey = mac.CreateKey(keyMaterial);
            //var hash = WinRTCrypto.CryptographicEngine.Sign(cryptoKey, WinRTCrypto.CryptographicBuffer.ConvertStringToBinary(input, Encoding.UTF8));
            //return WinRTCrypto.CryptographicBuffer.EncodeToBase64String(hash);
        }
    }
}
