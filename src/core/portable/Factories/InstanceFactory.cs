using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Factories
{
    public class InstanceFactory<T> : ICheckableFactory
    {
        public InstanceFactory(T instance) { this.instance = instance; }
        public InstanceFactory(IEnumerable<T> instances) { this.instances = instances; }
        T instance;
        IEnumerable<T> instances;
        public object Get(Type type)
        {
            if (typeof(T) != type) throw new FactoryCreationException($"InstanceFactory<{typeof(T).Name}> only give instances of type '{typeof(T).Name}'");
            if (instance == null) throw new FactoryCreationException($"This InstanceFactory<> only create single objects. Use '{nameof(GetMany)}' method to obtain multiple instances.");
            return instance;
        }
        public IEnumerable<object> GetMany(Type type)
        {
            if (typeof(T) != type) throw new FactoryCreationException($"InstanceFactory<{typeof(T).Name}> only give instances of type '{typeof(T).Name}'");
            if (instances == null) throw new FactoryCreationException($"This InstanceFactory<> only create multiple objects. Use '{nameof(Get)}' method to obtain an instance.");
            return (IEnumerable<object>)instances;
        }
        public bool CheckGet(Type type)
        {
            return typeof(T) == type && instance != null;
        }
        public bool CheckGetMany(Type type)
        {
            return typeof(T) == type && instances != null;
        }
    }
}
