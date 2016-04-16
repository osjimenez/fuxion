using Fuxion.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Fuxion.Test
{
    public class FactoryTest
    {
        [Fact]
        public void InstanceFactory_First()
        {
            Factory.AddToPipe(new InstanceFactory<string>("oka"));
            var res = Factory.Get<string>();
            Assert.Equal(res, "oka");
        }
        [Fact]
        public void FactoryDefaultImplementationAttribute()
        {
            var res = Factory.Get<IDefaultImplemented>(false);
            Assert.NotNull(res);
        }
    }
    [FactoryDefaultImplementation(typeof(DefaultImplementation))]
    public interface IDefaultImplemented { }
    public class DefaultImplementation : IDefaultImplemented { }
}
