using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Fuxion.Test.Collections.Generic
{
    public class IEnumerableExtensionsTest
    {
        [Fact(DisplayName = "IEnumerableExtensions - IsNullOrEmpty")]
        public void IsNullOrEmpty()
        {
            var col = new[] { "uno", "dos" };
            Assert.False(col.IsNullOrEmpty(), "Collection is not null or empty");
            col = new string[] { };
            Assert.True(col.IsNullOrEmpty(), "Collection is empty");
            col = null;
            Assert.True(col.IsNullOrEmpty(), "Collection is null");
        }
        [Fact(DisplayName = "IEnumerableExtensions - RemoveNulls")]
        public void RemoveNulls()
        {
            var col = new[] { "uno", "dos", null };
            col = col.RemoveNulls();
            Assert.Equal(2, col.Count());
        }
    }
}
