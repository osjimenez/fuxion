using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Test
{
    public class JsonTest
    {
        public JsonTest(ITestOutputHelper output)
        {
            this.output = output;
        }
        ITestOutputHelper output;
        [Fact]
        public void Json_First()
        {
            Base b = new Derived();
            var res = b.CloneWithJson();
            output.WriteLine("res.GetType() =" + res.GetType().Name);
        }
        class Base
        {

        }
        class Derived :Base
        {

        }
    }
}
