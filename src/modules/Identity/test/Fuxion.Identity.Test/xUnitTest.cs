using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Fuxion.Identity.Test
{
    public class xUnitTest
    {
        [Theory(DisplayName = "MyFirstTheory")]
        [InlineData(3, true)]
        [InlineData(5, true)]
        [InlineData(6, false)]
        public void MyFirstTheory(int value, bool expected)
        {
            Assert.True(IsOdd(value) == expected, $"Value {value} isn't odd");
        }

        bool IsOdd(int value)
        {
            return value % 2 == 1;
        }
    }
}
