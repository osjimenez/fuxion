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
            dynamic dyn = new Patchable<ToPatch>();
            dyn.Integer = 111;

            dyn.Patch(toPatch);

            Assert.Equal(111, toPatch.Integer);
        }
        [Fact(DisplayName = "Patchable - Get")]
        public void Get()
        {
            dynamic dyn = new Patchable<ToPatch>();
            dyn.Integer = 111;

            var delta = dyn as Patchable<ToPatch>;

            Assert.Equal(111, delta.Get<int>("Integer"));
        }
        [Fact(DisplayName = "Patchable - Indexer")]
        public void Indexer()
        {
            dynamic dyn = new Patchable<ToPatch>();
            dyn.Integer = 111;

            var delta = dyn as Patchable<ToPatch>;

            Assert.True(delta.Has("Integer"));
            Assert.False(delta.Has("Integer2"));
            Assert.Equal(111, delta.Get<int>("Integer"));
        }
    }
    public class ToPatch
    {
        public int Integer { get; set; }
        public string String { get; set; }
    }
}
