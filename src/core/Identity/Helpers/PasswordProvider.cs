using System.Security.Cryptography;
using System.Text;

namespace Fuxion.Identity.Helpers;

public class PasswordProvider : IPasswordProvider
{
	public int SaltBytesLenght { get; set; } = 8;
	public int HashIterations { get; set; } = 10137;
	public PasswordHashAlgorithm Algorithm { get; set; } = PasswordHashAlgorithm.SHA256;
	public void Generate(string password, out byte[] salt, out byte[] hash)
	{
#if NETSTANDARD2_0 || NET462
		var data = new byte[SaltBytesLenght];
		Random ran = new(Guid.NewGuid()
			.GetHashCode());
		ran.NextBytes(data);
		salt = data;
#else
		salt = RandomNumberGenerator.GetBytes(SaltBytesLenght);
#endif
		Generate(password, salt, out hash);
	}
	public bool Verify(string password, byte[] hash, byte[] salt)
	{
		var saltAndPass = Encoding.UTF8.GetBytes(password).Concat(salt).ToArray();
		var hashProvider = GetAlgorithm();
		var computeHash = hashProvider.ComputeHash(saltAndPass);
		for (var i = 0; i < HashIterations; i++) computeHash = hashProvider.ComputeHash(computeHash);
		return computeHash.SequenceEqual(hash);
	}
	HashAlgorithm GetAlgorithm() =>
		Algorithm switch {
			PasswordHashAlgorithm.SHA1   => SHA1.Create(),
			PasswordHashAlgorithm.SHA256 => SHA256.Create(),
			PasswordHashAlgorithm.SHA384 => SHA384.Create(),
			PasswordHashAlgorithm.SHA512 => SHA512.Create(),
			_                            => throw new ArgumentException($"Value '{Algorithm}' for '{nameof(Algorithm)}' is not valid")
		};
	internal void Generate(string password, byte[] salt, out byte[] hash)
	{
		var saltAndPass = Encoding.UTF8.GetBytes(password).Concat(salt).ToArray();
		var hashProvider = GetAlgorithm();
		hash = hashProvider.ComputeHash(saltAndPass);
		for (var i = 0; i < HashIterations; i++) hash = hashProvider.ComputeHash(hash);
	}
}

public enum PasswordHashAlgorithm
{
	SHA1 = 20,
	SHA256 = 32,
	SHA384 = 48,
	SHA512 = 64
}