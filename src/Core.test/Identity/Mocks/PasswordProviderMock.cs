using System.Text;

namespace Fuxion.Identity.Test.Mocks;

public class PasswordProviderMock : IPasswordProvider
{
	public void Generate(string password, out byte[] salt, out byte[] hash)
	{
		salt = new byte[]
			{ };
		hash = Encoding.Default.GetBytes(password);
	}
	public bool Verify(string password, byte[] hash, byte[] salt) => Encoding.Default.GetString(hash) == password;
}