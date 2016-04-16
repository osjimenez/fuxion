using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xunit;
using static PCLCrypto.WinRTCrypto;
using PCLCrypto;
using System.Text;
using System.Diagnostics;
using Xunit.Abstractions;
using System.Linq;

namespace Fuxion.Srm.Test
{
    public class Signing
    {
        public Signing(ITestOutputHelper output)
        {
            this.output = output;
        }
        ITestOutputHelper output;
        [Theory]
        [InlineData]
        public void Sign_ExportKey()
        {
            var k = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(AsymmetricAlgorithm.RsaSignPssSha512).CreateKeyPair(2048);
            var full = k.Export();
            var @public = k.ExportPublicKey();
            var pars = k.ExportParameters(true);
            output.WriteLine($"key exported with lenght of '{full.Length}' bytes");
            var imported = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(AsymmetricAlgorithm.RsaSignPssSha512).ImportKeyPair(full);
        }
        //[Fact]
        [Theory]
        [InlineData]
        public void Sign_FirstApproach()
        {
            var input = new DataToSign { IntegerValue = 137, StringValue = "fuxion :)", DateTimeOffsetValue = DateTimeOffset.Now };
            var json = input.ToJson();
            output.WriteLine("Input: " + json);
            output.WriteLine("");
            var k = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(AsymmetricAlgorithm.RsaSignPssSha512).CreateKeyPair(2048);
            //k.Export(CryptographicPrivateKeyBlobType.)
            var result = CryptographicEngine.Sign(k, Encoding.Default.GetBytes(json));
            var resultStr = Convert.ToBase64String(result);
            output.WriteLine("Result base64: " + resultStr);
            output.WriteLine("");
            var signed = new SignedData<DataToSign>
            {
                Data = input,
                Sign = resultStr
            };
            output.WriteLine("SignedData: " + signed.ToJson());
            var verificationResult = CryptographicEngine.VerifySignature(k, Encoding.Default.GetBytes(json), result);
            output.WriteLine("");
            output.WriteLine("Verification result: "+verificationResult);
            var verification2 = CryptographicEngine.VerifySignature(k, Encoding.Default.GetBytes(signed.Data.ToJson()), Convert.FromBase64String(signed.Sign));
            output.WriteLine("");
            output.WriteLine("Verification result: " + verification2);
        }
    }
    public class DataToSign
    {
        public int IntegerValue { get; set; }
        public string StringValue { get; set; }
        public DateTimeOffset DateTimeOffsetValue { get; set; }
    }
    public class SignedData<T>
    {
        public T Data { get; set; }
        public string Sign { get; set; }
    }
}
