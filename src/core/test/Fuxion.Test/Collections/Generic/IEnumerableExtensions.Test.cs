using Fuxion.Testing;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Test.Collections.Generic
{
	public class IEnumerableExtensionsTest : BaseTest
	{
		public IEnumerableExtensionsTest(ITestOutputHelper output) : base(output) { }
		[Fact(DisplayName = "IEnumerableExtensions - IsNullOrEmpty")]
		public void IsNullOrEmpty()
		{
			string[] col = new[] { "uno", "dos" };
			Assert.False(col.IsNullOrEmpty(), "Collection is not null or empty");
			col = new string[] { };
			Assert.True(col.IsNullOrEmpty(), "Collection is empty");
			col = null!;
			Assert.True(col.IsNullOrEmpty(), "Collection is null");
		}
		[Fact(DisplayName = "IEnumerableExtensions - RemoveNulls")]
		public void RemoveNulls()
		{
			var col = new[] { "uno", "dos", null };
			col = col.RemoveNulls();
			Assert.Equal(2, col.Count());
		}
		[Fact(DisplayName = "IEnumerableExtensions - RemoveOutliers")]
		public void RemoveOutliersTest()
		{
			var list = new int[] { 165, 165, 166, 167, 168, 169, 170, 170, 171, 172, 172, 174, 175, 176, 177, 178, 181 };
			//var list = new int[] { 41, 50, 29, 33, 40, 42, 53, 35, 28, 39, 37, 43, 34, 31, 44, 57, 32, 45, 46, 48};
			//var list = new int[] { 2, 3, 3, 3, 4, 5, 5, 15 };
			var res = list.RemoveOutliers(m => Printer.WriteLine(m));
			Printer.WriteLine("");
		}
	}
}
