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
        public void Factory_InstanceFactory_Same()
        {
            Factory.RemoveAllInjectors();
            var instance = new DefaultImplementation { Value = 123 };
            Factory.AddInjector(new InstanceInjector<DefaultImplementation>(instance));
            var res = Factory.Get<DefaultImplementation>();
            Assert.Same(res, instance);
            Assert.Equal(res, instance);
        }
        [Fact]
        public void Factory_FunctionFactory_NotSame()
        {
            Factory.RemoveAllInjectors();
            var instance = new DefaultImplementation { Value = 123 };
            Factory.AddInjector(new FunctionInjector<DefaultImplementation>(() => new DefaultImplementation { Value = 123 }));
            var res = Factory.Get<DefaultImplementation>();
            Assert.NotSame(res, instance);
            Assert.Equal(res, instance);
        }
        [Fact]
        public void Factory_DefaultImplementationAttribute()
        {
            var res = Factory.Get<IDefaultImplemented>(false);
            Assert.NotNull(res);
        }
    }
    [FactoryDefaultImplementation(typeof(DefaultImplementation))]
    public interface IDefaultImplemented { }
    public class DefaultImplementation : IDefaultImplemented {
        public Guid InstanceId { get; set; } = Guid.NewGuid();
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
