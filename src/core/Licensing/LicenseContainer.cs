using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
namespace Fuxion.Licensing
{
	public class LicenseContainer
	{
		public string Comment { get; set; }
		public string Signature { get; set; }
		[JsonProperty(PropertyName = "License")]
		public JRaw RawLicense { get; set; }
		public LicenseContainer Set(License license) { RawLicense = new JRaw(license.ToJson(Formatting.None)); return this; }
		public T As<T>() where T : License => RawLicense.Value.ToString().FromJson<T>();
		public License As(Type type) => (License)RawLicense.Value.ToString().FromJson(type);
		public bool Is<T>() where T : License => Is(typeof(T));
		public bool Is(Type type)
		{
			try
			{
				RawLicense.Value.ToString().FromJson(type);
				return true;
			}
			catch (Exception ex)
			{
				Debug.WriteLine("" + ex.Message);
				return false;
			}
		}
		public static LicenseContainer Sign(License license, string key)
		{
			license.SignatureUtcTime = DateTime.UtcNow;
			var originalData = Encoding.Unicode.GetBytes(license.ToJson(Formatting.None));
			byte[] signedData;
			var pro = new RSACryptoServiceProvider();
			pro.FromXmlString(key);
			signedData = pro.SignData(originalData, new SHA1CryptoServiceProvider());
			return new LicenseContainer
			{
				Signature = Convert.ToBase64String(signedData),
			}.Set(license);


			// Export the key information to an RSAParameters object.
			// You must pass true to export the private key for signing.
			// However, you do not need to export the private key
			// for verification.
			//RSAParameters Key = pro.ExportParameters(true);

			// Sign
			//signedData = pro.SignData(originalData, new SHA1CryptoServiceProvider());
			//=> Sign(content, Convert.FromBase64String(key));
		}
		public bool VerifySignature(string key)
		{
			var pro = new RSACryptoServiceProvider();
			pro.FromXmlString(key);
			return pro.VerifyData(
				Encoding.Unicode.GetBytes(RawLicense.ToJson(Formatting.None)),
				new SHA1CryptoServiceProvider(),
				Convert.FromBase64String(Signature));
			//=> VerifySignature(Convert.FromBase64String(key));
		}
	}
	public static class LicenseContainerExtensions
	{
		public static IQueryable<LicenseContainer> WithValidSignature(this IQueryable<LicenseContainer> me, string publicKey)
			=> me.Where(l => l.VerifySignature(publicKey));
		public static IQueryable<LicenseContainer> OfType<TLicense>(this IQueryable<LicenseContainer> me) where TLicense : License
			=> me.Where(l => l.Is<TLicense>());
		public static IQueryable<LicenseContainer> OfType(this IQueryable<LicenseContainer> me, Type type)
			=> me.Where(l => l.Is(type));
		public static IQueryable<LicenseContainer> OnlyValidOfType<TLicense>(this IQueryable<LicenseContainer> me, string publicKey) where TLicense : License
		{
			string _;
			return me.WithValidSignature(publicKey).OfType<TLicense>().Where(l => l.As<TLicense>().Validate(out _));
		}
		public static IQueryable<LicenseContainer> OnlyValidOfType(this IQueryable<LicenseContainer> me, string publicKey, Type type)
		{
			string _;
			return me.WithValidSignature(publicKey).OfType(type).Where(l => l.As(type).Validate(out _));
		}
	}
}
