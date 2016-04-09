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
            if (typeof(T) != type) throw new NotImplementedException();
            if (!instanceSetted) throw new NotImplementedException();
            return instance;
        }
        public IEnumerable<object> GetMany(Type type)
        {
            if (typeof(T) != type) throw new NotImplementedException();
            if (!instancesSetted) throw new NotImplementedException();
            return (IEnumerable<object>)instances;
        }
    }
}
