using Fuxion.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Fuxion.Test
{
    public class Patchable
    {
        [Fact]
        public void Patchable_First()
        {
            dynamic pat = new Patchable<ToPatch>();
            //pat.SetMember("Integer", 123);
            pat.Integer = 123;
            pat.String = "test";

            Assert.True(true);
        }
    }
    public class ToPatch
    {
        public int Integer { get; set; }
        public string String { get; set; }
    }
}
