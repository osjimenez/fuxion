using System.Linq;
using System.Security.Cryptography;
using System.Xml.Linq;
namespace Fuxion.Licensing
{
	public class LicensingManager
	{
		public LicensingManager(ILicenseStore store) => Store = store;
		public ILicenseStore Store { get; set; }
		public ILicenseProvider GetProvider() => Singleton.Get<ILicenseProvider>(false);
		public static void GenerateKey(out string fullKey, out string publicKey)
		{
			var pro = new RSACryptoServiceProvider();
			fullKey = XDocument.Parse(pro.ToXmlString(true)).ToString();
			publicKey = XDocument.Parse(pro.ToXmlString(false)).ToString();
		}
		// TODO - Oscar - Implement server-side validation and try to avoid public key hardcoding violation
		public bool Validate<T>(string key, bool throwExceptionIfNotValidate = false) where T : License
		{
			try
			{
				string validationMessage = null;
				var cons = Store.Query()
					.Where(c =>
						c.VerifySignature(key)
						&&
						c.Is<T>());
				if (!cons.Any())
					throw new LicenseValidationException($"Couldn't find any license of type '{typeof(T).Name}'");
				if (!Store.Query().Any(l => l.Is<T>() && l.As<T>().Validate(out validationMessage)))
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
