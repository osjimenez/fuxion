using Fuxion.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Fuxion.Test.Collections.Generic
{
    public class GenericEqualityComparerTest
    {
        [Fact]
        public void Do()
        {
            Assert.True(new GenericEqualityComparer<int>((a, b) => a.Equals(b), a => a.GetHashCode()).Equals(1, 1));
            Assert.False(new GenericEqualityComparer<int>((a, b) => a.Equals(b), a => a.GetHashCode()).Equals(1, 2));
        }
    }
}
