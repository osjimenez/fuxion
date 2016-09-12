using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Test
{
    public class RemoveOutliersTest
    {
        public RemoveOutliersTest(ITestOutputHelper output)
        {
            this.output = output;
        }
        ITestOutputHelper output;
        [Fact]
        public void Init()
        {
            var list = new int[] { 165, 165, 166, 167, 168, 169, 170, 170, 171, 172, 172, 174, 175, 176, 177, 178, 181 };
            //var list = new int[] { 41, 50, 29, 33, 40, 42, 53, 35, 28, 39, 37, 43, 34, 31, 44, 57, 32, 45, 46, 48};
            //var list = new int[] { 2, 3, 3, 3, 4, 5, 5, 15 };
            var res = list.RemoveOutliers(m => output.WriteLine(m));
            output.WriteLine("");
        }
    }
}
