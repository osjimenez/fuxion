using Fuxion.Identity.Helpers;
using Fuxion.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Identity.Test.Helpers;

public class PasswordProviderTest : BaseTest<PasswordProviderTest>
{
	public PasswordProviderTest(ITestOutputHelper output) : base(output) { }
	[Fact(DisplayName = "PasswordProvider - Generate")]
	public void Generate()
	{
		var pwd = "123";
		var gen = new PasswordProvider
		{
			SaltBytesLenght = 3, Algorithm = PasswordHashAlgorithm.SHA1
		};
		Output.WriteLine("Algorithm: " + gen.Algorithm);
		gen.Generate(pwd, out var salt, out var hash);
		Output.WriteLine($"Generated SALT: {BitConverter.ToString(salt)} - Length {salt.Length}");
		Assert.Equal(3, salt.Length);
		Output.WriteLine($"Generated HASH: {BitConverter.ToString(hash)} - Length {hash.Length}");
		Assert.Equal(20, hash.Length);
		Assert.True(gen.Verify(pwd, hash, salt));
		Output.WriteLine("");
		gen.SaltBytesLenght = 4;
		gen.Algorithm       = PasswordHashAlgorithm.SHA256;
		Output.WriteLine("Algorithm: " + gen.Algorithm);
		gen.Generate(pwd, out salt, out hash);
		Output.WriteLine($"Generated SALT: {BitConverter.ToString(salt)} - Length {salt.Length}");
		Assert.Equal(4, salt.Length);
		Output.WriteLine($"Generated HASH: {BitConverter.ToString(hash)} - Length {hash.Length}");
		Assert.Equal(32, hash.Length);
		Assert.True(gen.Verify(pwd, hash, salt));
		Output.WriteLine("");
		gen.SaltBytesLenght = 8;
		gen.Algorithm       = PasswordHashAlgorithm.SHA384;
		Output.WriteLine("Algorithm: " + gen.Algorithm);
		gen.Generate(pwd, out salt, out hash);
		Output.WriteLine($"Generated SALT: {BitConverter.ToString(salt)} - Length {salt.Length}");
		Assert.Equal(8, salt.Length);
		Output.WriteLine($"Generated HASH: {BitConverter.ToString(hash)} - Length {hash.Length}");
		Assert.Equal(48, hash.Length);
		Assert.True(gen.Verify(pwd, hash, salt));
		Output.WriteLine("");
		gen.SaltBytesLenght = 8;
		gen.Algorithm       = PasswordHashAlgorithm.SHA512;
		Output.WriteLine("Algorithm: " + gen.Algorithm);
		gen.Generate(pwd, out salt, out hash);
		Output.WriteLine($"Generated SALT: {BitConverter.ToString(salt)} - Length {salt.Length}");
		Assert.Equal(8, salt.Length);
		Output.WriteLine($"Generated HASH: {BitConverter.ToString(hash)} - Length {hash.Length}");
		Assert.Equal(64, hash.Length);
		Assert.True(gen.Verify(pwd, hash, salt));
	}
	[Fact(DisplayName = "PasswordProvider - HashIteration")]
	public void HashIteration()
	{
		var pwd = "123";
		var gen = new PasswordProvider
		{
			HashIterations = 10
		};
		gen.Generate(pwd, out var salt, out var hash1);
		gen.Generate(pwd, salt,         out var hash2);
		Assert.Equal(hash1, hash2);
		gen.HashIterations = 20;
		gen.Generate(pwd, salt, out hash2);
		Assert.NotEqual(hash1, hash2);
	}
}