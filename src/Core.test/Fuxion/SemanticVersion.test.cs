using System.Text.RegularExpressions;

namespace Fuxion.Test;

// https://github.com/maxhauser/semver
public class SemanticVersionTest(ITestOutputHelper output) : BaseTest<SemanticVersionTest>(output)
{
	[Fact]
	public void CompareIdentifierCollections()
	{
		SemanticVersionIdentifierCollection num1 = new("1".Split('.')
			.Select(i => new SemanticVersionIdentifier(i))
			.ToArray());
		SemanticVersionIdentifierCollection num1_1 = new("1.1".Split('.')
			.Select(i => new SemanticVersionIdentifier(i))
			.ToArray());
		SemanticVersionIdentifierCollection num1_2 = new("1.2".Split('.')
			.Select(i => new SemanticVersionIdentifier(i))
			.ToArray());
		SemanticVersionIdentifierCollection num2 = new("2".Split('.')
			.Select(i => new SemanticVersionIdentifier(i))
			.ToArray());
		SemanticVersionIdentifierCollection num2_1 = new("2.1".Split('.')
			.Select(i => new SemanticVersionIdentifier(i))
			.ToArray());
		SemanticVersionIdentifierCollection alpha = new("alpha".Split('.')
			.Select(i => new SemanticVersionIdentifier(i))
			.ToArray());
		SemanticVersionIdentifierCollection alpha_1 = new("alpha.1".Split('.')
			.Select(i => new SemanticVersionIdentifier(i))
			.ToArray());
		SemanticVersionIdentifierCollection beta = new("beta".Split('.')
			.Select(i => new SemanticVersionIdentifier(i))
			.ToArray());
		SemanticVersionIdentifierCollection beta_1 = new("beta.1".Split('.')
			.Select(i => new SemanticVersionIdentifier(i))
			.ToArray());
		IsTrue(num1 < num1_1);
		IsTrue(num1_1 < num1_2);
		IsTrue(num1_2 < num2);
		IsTrue(num2 < num2_1);
		IsTrue(num2_1 < alpha);
		IsTrue(alpha < alpha_1);
		IsTrue(alpha_1 < beta);
		IsTrue(beta < beta_1);
	}
	[Fact]
	public void CompareIdentifiers()
	{
		SemanticVersionIdentifier num1 = new("1");
		SemanticVersionIdentifier num2 = new("2");
		SemanticVersionIdentifier alpha = new("alpha");
		SemanticVersionIdentifier beta = new("beta");
		IsTrue(num1 < num2);
		IsTrue(num2 < alpha);
		IsTrue(alpha < beta);
	}
	[Fact]
	public void MajorMinorPatch()
	{
		var v0_0_0 = new SemanticVersion("0.0.0");
		var v1_0_0 = new SemanticVersion("1.0.0");
		var v1_1_0 = new SemanticVersion("1.1.0");
		var v1_2_0 = new SemanticVersion("1.2.0");
		var v1_2_1 = new SemanticVersion("1.2.1");
		var v1_2_2 = new SemanticVersion("1.2.2");
		IsTrue(v0_0_0 < v1_0_0);
		IsTrue(v1_0_0 < v1_1_0);
		IsTrue(v1_1_0 < v1_2_0);
		IsTrue(v1_2_0 < v1_2_1);
		IsTrue(v1_2_1 < v1_2_2);
	}
	[Fact]
	public void PreRelease()
	{
		// Pre release
		var _1 = new SemanticVersion("1.0.0-1");
		var alpha = new SemanticVersion("1.0.0-alpha");
		var alpha_1 = new SemanticVersion("1.0.0-alpha.1");
		var alpha_2 = new SemanticVersion("1.0.0-alpha.2");
		var alpha_2_1 = new SemanticVersion("1.0.0-alpha.2.1");
		var alpha_2_2 = new SemanticVersion("1.0.0-alpha.2.2");
		var alpha_beta = new SemanticVersion("1.0.0-alpha.beta");
		var beta = new SemanticVersion("1.0.0-beta");
		var beta_1 = new SemanticVersion("1.0.0-beta.1");
		var v1_0_0 = new SemanticVersion("1.0.0");
		IsTrue(_1 < alpha);
		IsTrue(alpha < alpha_1);
		IsTrue(alpha < alpha_1);
		IsTrue(alpha_1 < alpha_2);
		IsTrue(alpha_2 < alpha_2_1);
		IsTrue(alpha_2_1 < alpha_2_2);
		IsTrue(alpha_2_2 < alpha_beta);
		IsTrue(alpha_beta < beta);
		IsTrue(beta < beta_1);
		IsTrue(beta_1 < v1_0_0);
		IsTrue(beta_1 != v1_0_0);
		var alpha_bis = new SemanticVersion("1.0.0-alpha");
		IsTrue(alpha == alpha_bis);
		var alpha_meta = new SemanticVersion("1.0.0-alpha+metadata");
		IsTrue(alpha == alpha_meta);
	}
	[Fact]
	public void Regex()
	{
		var reg = SemanticVersion.SemanticVersionRegex();
		const string ver = "1.2.3-pre.4.5.6+meta.7.8.9";
		PrintVariable(ver);
		var m = reg.Match(ver);
		PrintVariable(m.Success);
		PrintVariable(m.Groups["major"].Value);
		PrintVariable(m.Groups["minor"].Value);
		PrintVariable(m.Groups["patch"].Value);
		PrintVariable(m.Groups["prerelease"].Value);
		PrintVariable(m.Groups["buildmetadata"].Value);
	}
	[Fact]
	public void TryParse()
	{
		IsTrue(SemanticVersion.TryParse("1.0.0-alpha", out var res));
		PrintVariable(res);
		IsFalse(SemanticVersion.TryParse("1.0.0-alpha-beta+rc+metadata", out res));
		PrintVariable(res);
		IsFalse(SemanticVersion.TryParse("1.2.3.4", out res));
		PrintVariable(res);
	}
}