using Fuxion.Web;
using Microsoft.CSharp.RuntimeBinder;
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
        [Fact(DisplayName = "Patchable - Cast")]
        public void Cast()
        {
            dynamic dyn = new Patchable<ToPatch>();
            //dyn.Id = Guid.Parse("{7F27735C-FDE1-4141-985A-214502599C63}");
            dyn.Id = "{7F27735C-FDE1-4141-985A-214502599C63}";

            var delta = dyn as Patchable<ToPatch>;
            var id = delta.Get<Guid>("Id");

            Assert.Equal(Guid.Parse("{7F27735C-FDE1-4141-985A-214502599C63}"), id);
        }
        [Fact(DisplayName = "Patchable - Allow non existing properties")]
        public void NonExistingProperties()
        {
            Patchable<ToPatch>.NonExistingPropertiesMode = NonExistingPropertiesMode.NotAllowed;

            // Create a Patchable
            dynamic dyn = new Patchable<ToPatch>();
            // Set non existing property
            Assert.Throws<RuntimeBinderException>(() =>
            {
                dyn.DerivedInteger = 123;
            });
            Patchable<ToPatch>.NonExistingPropertiesMode = NonExistingPropertiesMode.OnlySet;
            dyn.DerivedInteger = 123;

            // Path a derived class
            var derived = new DerivedToPatch();
            (dyn as Patchable<ToPatch>).ToPatchable<DerivedToPatch>().Patch(derived);
            Assert.Equal(123, derived.DerivedInteger);

            // Get non existing property
            var delta = dyn as Patchable<ToPatch>;
            int res;
            Assert.Throws<RuntimeBinderException>(() =>
            {
                res = delta.Get<int>("DerivedInteger");
            });
            Patchable<ToPatch>.NonExistingPropertiesMode = NonExistingPropertiesMode.GetAndSet;
            res = delta.Get<int>("DerivedInteger");

            Assert.Equal(123, res);
        }
    }
    public class ToPatch
    {
        public int Integer { get; set; }
        public string String { get; set; }
        public Guid Id { get; set; }
    }
    public class DerivedToPatch
    {
        public int DerivedInteger { get; set; }
    }
}
