using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Factories
{
    public class InstanceFactory<T> : IFactory
    {
        public InstanceFactory(T instance) { this.instance = instance; instanceSetted = true; }
        public InstanceFactory(IEnumerable<T> instances) { this.instances = instances; instancesSetted = true; }
        bool instanceSetted = false;
        T instance;
        bool instancesSetted = false;
        IEnumerable<T> instances;
        public object Get(Type type)
        {
            if (typeof(T) != type) throw new FactoryCreationException();
            if (!instanceSetted) throw new FactoryCreationException();
            return instance;
        }
        public IEnumerable<object> GetMany(Type type)
        {
            if (typeof(T) != type) throw new FactoryCreationException();
            if (!instancesSetted) throw new FactoryCreationException();
            return (IEnumerable<object>)instances;
        }
    }
}
