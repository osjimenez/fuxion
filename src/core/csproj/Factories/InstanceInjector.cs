using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Factories
{
    public class InstanceInjector<T> : ICheckableInjector
    {
        public InstanceInjector(T instance) { this.instance = instance; }
        public InstanceInjector(Func<T> instanceCreationFunction) { this.instanceCreationFunction = instanceCreationFunction; }
        public InstanceInjector(IEnumerable<T> instances) { this.instances = instances; }
        public InstanceInjector(Func<IEnumerable<T>> instancesCreationFunction) { this.instancesCreationFunction = instancesCreationFunction; }
        Func<T> instanceCreationFunction;
        T instance;
        Func<IEnumerable<T>> instancesCreationFunction;
        IEnumerable<T> instances;
        public object Get(Type type)
        {
            if (typeof(T) != type) throw new FactoryCreationException($"InstanceFactory<{typeof(T).Name}> only give instances of type '{typeof(T).Name}'");
            if (instance == null && instanceCreationFunction == null) throw new FactoryCreationException($"This InstanceFactory<> only create multiple objects. Use '{nameof(GetMany)}' method to obtain multiple instances.");
            if (instance == null) instance = instanceCreationFunction();
            return instance;
        }
        public IEnumerable<object> GetMany(Type type)
        {
            if (typeof(T) != type) throw new FactoryCreationException($"InstanceFactory<{typeof(T).Name}> only give instances of type '{typeof(T).Name}'");
            if (instances == null && instancesCreationFunction == null) throw new FactoryCreationException($"This InstanceFactory<> only create a single object. Use '{nameof(Get)}' method to obtain an instance.");
            if (instances == null) instances = instancesCreationFunction();
            return (IEnumerable<object>)instances;
        }
        public bool CheckGet(Type type)
        {
            return typeof(T) == type && (instance != null || instanceCreationFunction != null);
        }
        public bool CheckGetMany(Type type)
        {
            return typeof(T) == type && (instances != null || instancesCreationFunction != null);
        }
    }
}
