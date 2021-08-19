namespace Fuxion.Test.Collections.Generic;

using Fuxion.Collections.Generic;

public class GenericEqualityComparerTest
{
	[Fact(DisplayName = "GenericEqualityComparer - Do")]
	public void Do()
	{
		Assert.True(new GenericEqualityComparer<int>((a, b) => a.Equals(b), a => a.GetHashCode()).Equals(1, 1));
		Assert.False(new GenericEqualityComparer<int>((a, b) => a.Equals(b), a => a.GetHashCode()).Equals(1, 2));
	}
}