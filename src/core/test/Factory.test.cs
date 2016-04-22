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
            var instance = new DefaultImplementation { Value = 123 };
            Factory.AddToPipe(new InstanceFactory<DefaultImplementation>(instance));
            var res = Factory.Get<DefaultImplementation>();
            Assert.Same(res, instance);
            Assert.Equal(res, instance);
        }
        [Fact]
        public void FunctionFactory_First()
        {
            var instance = new DefaultImplementation { Value = 123 };
            Factory.AddToPipe(new FunctionFactory<DefaultImplementation>(() => new DefaultImplementation { Value = 123 }));
            var res = Factory.Get<DefaultImplementation>();
            Assert.NotSame(res, instance);
            Assert.Equal(res, instance);
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
    public class DefaultImplementation : IDefaultImplemented {
        public int Value { get; set; }
        public override bool Equals(object obj)
        {
            if(obj is DefaultImplementation)
            {
                return (obj as DefaultImplementation).Value == Value;
            }
            return base.Equals(obj);
        }
        public override int GetHashCode() { return Value.GetHashCode(); }
    }
}
