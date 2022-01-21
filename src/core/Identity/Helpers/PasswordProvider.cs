namespace Fuxion.Identity.Helpers; 

using System.Security.Cryptography;
using System.Text;

public class PasswordProvider : IPasswordProvider
{
	public int SaltBytesLenght { get; set; } = 8;
	public int HashIterations { get; set; } = 10137;
	public PasswordHashAlgorithm Algorithm { get; set; } = PasswordHashAlgorithm.SHA256;

	private HashAlgorithm GetAlgorithm()
		=> Algorithm switch
		{
			PasswordHashAlgorithm.SHA1 => SHA1.Create(),
			PasswordHashAlgorithm.SHA256 => SHA256.Create(),
			PasswordHashAlgorithm.SHA384 => SHA384.Create(),
			PasswordHashAlgorithm.SHA512 => SHA512.Create(),
			_ => throw new ArgumentException($"Value '{Algorithm}' for '{nameof(Algorithm)}' is not valid"),
		};
	internal void Generate(string password, byte[] salt, out byte[] hash)
	{
		var saltAndPass = Encoding.UTF8.GetBytes(password).Concat(salt).ToArray();
		var hashProvider = GetAlgorithm();
		hash = hashProvider.ComputeHash(saltAndPass);
		for (var i = 0; i < HashIterations; i++)
			hash = hashProvider.ComputeHash(hash);
	}
	public void Generate(string password, out byte[] salt, out byte[] hash)
	{
		salt = RandomNumberGenerator.GetBytes(SaltBytesLenght);
		Generate(password, salt, out hash);
	}
	public bool Verify(string password, byte[] hash, byte[] salt)
	{
		var saltAndPass = Encoding.UTF8.GetBytes(password).Concat(salt).ToArray();
		var hashProvider = GetAlgorithm();
		var computeHash = hashProvider.ComputeHash(saltAndPass);
		for (var i = 0; i < HashIterations; i++)
			computeHash = hashProvider.ComputeHash(computeHash);
		return computeHash.SequenceEqual(hash);
	}
}
public enum PasswordHashAlgorithm
{
	SHA1 = 20,
	SHA256 = 32,
	SHA384 = 48,
	SHA512 = 64
}