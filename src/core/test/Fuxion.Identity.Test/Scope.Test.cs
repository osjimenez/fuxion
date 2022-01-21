namespace Fuxion.Identity.Test;

using Fuxion.Identity.Test.Dao;
using Fuxion.Testing;
using Xunit;
using Xunit.Abstractions;

public class ScopeTest : BaseTest
{
	public ScopeTest(ITestOutputHelper helper) : base(helper) { }
	[Fact(DisplayName = "Scope - Null discriminator validation")]
	public void Validate_WrongName() => Assert.False(new ScopeDao("", "", null!, ScopePropagation.ToMe).IsValid());//Assert.IsTrue(new Scope(null, ScopePropagation.ToMe).IsValid());
}