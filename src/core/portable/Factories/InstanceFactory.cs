using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Factories
{
    public class InstanceFactory<T> : IFactory
    {
        public InstanceFactory(T instance) { this.instance = instance; }
        public InstanceFactory(IEnumerable<T> instances) { this.instances = instances; }
        bool instanceSetted = false;
        T instance;
        bool instancesSetted = false;
        IEnumerable<T> instances;
        public object Get(Type type)
        {
            if (!instanceSetted) throw new NotImplementedException();
            return instance;
        }
        public IEnumerable<object> GetMany(Type type)
        {
            if (!instancesSetted) throw new NotImplementedException();
            return (IEnumerable<object>)instances;
        }
    }
}
