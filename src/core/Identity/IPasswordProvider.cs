namespace Fuxion.Identity;

public interface IPasswordProvider
{
	bool Verify(string   password, byte[]     hash, byte[]     salt);
	void Generate(string password, out byte[] salt, out byte[] hash);
}