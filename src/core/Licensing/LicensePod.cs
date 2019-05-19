using Fuxion.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Fuxion.Licensing
{
	public class LicensePod : JsonPod<License, Type>
	{
		public string? Comment { get; set; }
		public string? Signature { get; set; }
		//[JsonProperty(PropertyName = "License")]
		//public JRaw RawLicense { get; set; }
		//public LicensePod Set(License license) { RawLicense = new JRaw(license.ToJson(Formatting.None)); return this; }

		//public T As<T>() where T : License
		//{
		//	return RawLicense.Value.ToString().FromJson<T>();
		//}

		//public License As(Type type)
		//{
		//	return (License)RawLicense.Value.ToString().FromJson(type);
		//}

		public License AsLicense(Type type) => (License)As(type);

		//public bool Is<T>() where T : License
		//{
		//	return Is(typeof(T));
		//}

		//public bool Is(Type type)
		//{
		//	try
		//	{
		//		RawLicense.Value.ToString().FromJson(type);
		//		return true;
		//	}
		//	catch (Exception ex)
		//	{
		//		Debug.WriteLine("" + ex.Message);
		//		return false;
		//	}
		//}
		//public static LicensePod Sign(License license, string key)
		//{
		//	license.SignatureUtcTime = DateTime.UtcNow;
		//	byte[] originalData = Encoding.Unicode.GetBytes(license.ToJson(Formatting.None));
		//	byte[] signedData;
		//	RSACryptoServiceProvider pro = new RSACryptoServiceProvider();
		//	pro.FromXmlString(key);
		//	signedData = pro.SignData(originalData, new SHA1CryptoServiceProvider());
		//	return new LicensePod
		//	{
		//		Signature = Convert.ToBase64String(signedData),
		//	}.Set(license);


		//	// Export the key information to an RSAParameters object.
		//	// You must pass true to export the private key for signing.
		//	// However, you do not need to export the private key
		//	// for verification.
		//	//RSAParameters Key = pro.ExportParameters(true);

		//	// Sign
		//	//signedData = pro.SignData(originalData, new SHA1CryptoServiceProvider());
		//	//=> Sign(content, Convert.FromBase64String(key));
		//}
		public bool VerifySignature(string key)
		{
			RSACryptoServiceProvider pro = new RSACryptoServiceProvider();
			pro.FromXmlString(key);
			return pro.VerifyData(
				Encoding.Unicode.GetBytes(Payload.ToJson(Formatting.None)),
				new SHA1CryptoServiceProvider(),
				Convert.FromBase64String(Signature));
		}
	}
	public static class LicensePodExtensions
	{
		public static IQueryable<LicensePod> WithValidSignature(this IQueryable<LicensePod> me, string publicKey)
		{
			return me.Where(l => l.VerifySignature(publicKey));
		}

		public static IQueryable<LicensePod> OfType<TLicense>(this IQueryable<LicensePod> me) where TLicense : License
		{
			return me.Where(l => l.Is<TLicense>());
		}

		public static IQueryable<LicensePod> OfType(this IQueryable<LicensePod> me, Type type)
		{
			return me.Where(l => l.Is(type));
		}

		public static IQueryable<LicensePod> OnlyValidOfType<TLicense>(this IQueryable<LicensePod> me, string publicKey) where TLicense : License
		{
			string _;
			return me.WithValidSignature(publicKey).OfType<TLicense>().Where(l => l.As<TLicense>().Validate(out _));
		}
		public static IQueryable<LicensePod> OnlyValidOfType(this IQueryable<LicensePod> me, string publicKey, Type type)
		{
			string _;
			return me.WithValidSignature(publicKey).OfType(type).Where(l => l.AsLicense(type).Validate(out _));
		}
	}
}
