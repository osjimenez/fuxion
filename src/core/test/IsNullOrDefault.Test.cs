using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
namespace Fuxion.Test
{
    public class IsNullOrDefaultTest
    {
        [Fact]
		public void IsNullOrDefault_First()
        {
            string s = null;
            Assert.True(s.IsNullOrDefault());
            s = "";
            Assert.False(s.IsNullOrDefault());
            int i = 0;
            Assert.True(i.IsNullOrDefault());
            i = 1;
            Assert.False(i.IsNullOrDefault());
            Guid g = Guid.Empty;
            Assert.True(g.IsNullOrDefault());
            g = Guid.NewGuid();
            Assert.False(g.IsNullOrDefault());
            int? i2 = null;
            Assert.True(i2.IsNullOrDefault());
            i2 = null;
            Assert.True(i2.IsNullOrDefault());
            i2 = 1;
            Assert.False(i2.IsNullOrDefault());
        }
    }
}
