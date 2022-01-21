namespace Fuxion.Licensing; 

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

public class LicenseContainer
{
#nullable disable
	public LicenseContainer() { }
#nullable enable
	public LicenseContainer(string signature, License license)
	{
		Signature = signature;
		RawLicense = new JRaw(license.ToJson(Newtonsoft.Json.Formatting.None));
	}
	public string? Comment { get; set; }
	public string Signature { get; set; }
	[JsonProperty(PropertyName = "License")]
	public JRaw RawLicense { get; set; }
	public LicenseContainer Set(License license) { RawLicense = new JRaw(license.ToJson(Newtonsoft.Json.Formatting.None)); return this; }
	public T? As<T>() where T : License => RawLicense.Value?.ToString()?.FromJson<T>();
	public License? As(Type type) => (License?)RawLicense.Value?.ToString()?.FromJson(type);
	public bool Is<T>() where T : License => Is(typeof(T));
	public bool Is(Type type)
	{
		try
		{
			return RawLicense.Value?.ToString()?.FromJson(type) != null;
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
		var originalData = Encoding.Unicode.GetBytes(license.ToJson(Newtonsoft.Json.Formatting.None));
		byte[] signedData;
		var pro = new RSACryptoServiceProvider();
		//pro.FromXmlString(key);
		FromXmlString(pro, key);
		signedData = pro.SignData(originalData, SHA1.Create());
		return new LicenseContainer(Convert.ToBase64String(signedData), license);


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
		//pro.FromXmlString(key);
		FromXmlString(pro, key);
		return pro.VerifyData(
			Encoding.Unicode.GetBytes(RawLicense.ToJson(Newtonsoft.Json.Formatting.None)),
			SHA1.Create(),
			Convert.FromBase64String(Signature));
		//=> VerifySignature(Convert.FromBase64String(key));
	}
	// TODO - Must be done in framework when solved issue https://github.com/dotnet/corefx/pull/37593
	private static void FromXmlString(RSACryptoServiceProvider rsa, string xmlString)
	{
		var parameters = new RSAParameters();

		var xmlDoc = new XmlDocument();
		xmlDoc.LoadXml(xmlString);

		if (xmlDoc.DocumentElement?.Name.Equals("RSAKeyValue") ?? false)
		{
			foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
			{
				switch (node.Name)
				{
					case "Modulus": parameters.Modulus = Convert.FromBase64String(node.InnerText); break;
					case "Exponent": parameters.Exponent = Convert.FromBase64String(node.InnerText); break;
					case "P": parameters.P = Convert.FromBase64String(node.InnerText); break;
					case "Q": parameters.Q = Convert.FromBase64String(node.InnerText); break;
					case "DP": parameters.DP = Convert.FromBase64String(node.InnerText); break;
					case "DQ": parameters.DQ = Convert.FromBase64String(node.InnerText); break;
					case "InverseQ": parameters.InverseQ = Convert.FromBase64String(node.InnerText); break;
					case "D": parameters.D = Convert.FromBase64String(node.InnerText); break;
				}
			}
		}
		else
		{
			throw new Exception("Invalid XML RSA key.");
		}

		rsa.ImportParameters(parameters);
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
		return me.WithValidSignature(publicKey).OfType<TLicense>()
			.Where(l => l.Is<TLicense>())
			.Where(l => l.As<TLicense>()!.Validate(out _));
	}
	public static IQueryable<LicenseContainer> OnlyValidOfType(this IQueryable<LicenseContainer> me, string publicKey, Type type)
	{
		string _;
		return me.WithValidSignature(publicKey).OfType(type)
			.Where(l => l.Is(type))
			.Where(l => l.As(type)!.Validate(out _));
	}
}