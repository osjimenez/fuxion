using Fuxion.Factories;
using Fuxion.Identity.Helpers;

namespace Fuxion.Identity
{
	[FactoryDefaultImplementation(typeof(PasswordProvider))]
	public interface IPasswordProvider
	{
		bool Verify(string password, byte[] hash, byte[] salt);
		void Generate(string password, out byte[] salt, out byte[] hash);
	}
}
