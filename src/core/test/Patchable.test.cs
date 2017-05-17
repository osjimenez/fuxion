using Fuxion.Web;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Fuxion.Test
{
    public class PatchableTest
    {
        [Fact(DisplayName = "Patchable - Patch")]
        public void Patch()
        {
            var toPatch = new ToPatch
            {
                Integer = 123,
                String = "TEST"
            };
            dynamic pat = new Patchable<ToPatch>();
            //pat.SetMember("Integer", 123);
            pat.Integer = 111;

            pat.Patch(toPatch);

            Assert.Equal(111, toPatch.Integer);
        }
        [Fact(DisplayName = "Patchable - GetMemeber")]
        public void GetMemeber()
        {
            var toPatch = new ToPatch
            {
                Integer = 123,
                String = "TEST"
            };
            dynamic pat = new Patchable<ToPatch>();
            pat.Integer = 111;
            var patt = pat as Patchable<ToPatch>;
            
            var value = (int)patt.GetMember(nameof(toPatch.Integer));

            Assert.Equal(111, value);
        }
    }
    public class ToPatch
    {
        public int Integer { get; set; }
        public string String { get; set; }
    }
}
