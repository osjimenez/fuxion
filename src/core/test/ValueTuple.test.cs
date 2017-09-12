using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Fuxion.Test
{
    public class ValueTupleTest 
    {
        [Fact]
        public void ValueTupleBasicTest()
        {
            var tup = new ValueTupleForTest().GetTuple();
            Assert.Equal(1, tup.uno);
            Assert.Equal(2, tup.dos);
        }
    }
}
