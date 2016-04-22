using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Factories
{
    public class FunctionFactory<T> : ICheckableFactory
    {
        public FunctionFactory(Func<T> createInstanceFunction) { this.createInstanceFunction = createInstanceFunction; }
        public FunctionFactory(Func<IEnumerable<T>> createInstancesFunction) { this.createInstancesFunction = createInstancesFunction; }
        Func<T> createInstanceFunction;
        Func<IEnumerable<T>> createInstancesFunction;
        public object Get(Type type)
        {
            if (typeof(T) != type) throw new FactoryCreationException($"FunctionFactory<{typeof(T).Name}> only give instances of type '{typeof(T).Name}'");
            if (createInstanceFunction == null) throw new FactoryCreationException($"This FunctionFactory<> only create single objects. Use '{nameof(GetMany)}' method to obtain multiple instances.");
            return createInstanceFunction();
        }
        public IEnumerable<object> GetMany(Type type)
        {
            if (typeof(T) != type) throw new FactoryCreationException($"FunctionFactory<{typeof(T).Name}> only give instances of type '{typeof(T).Name}'");
            if (createInstancesFunction == null) throw new FactoryCreationException($"This FunctionFactory<> only create multiple objects. Use '{nameof(Get)}' method to obtain an instance.");
            return (IEnumerable<object>)createInstancesFunction();
        }
        public bool CheckGet(Type type)
        {
            return typeof(T) == type && createInstanceFunction != null;
        }
        public bool CheckGetMany(Type type)
        {
            return typeof(T) == type && createInstancesFunction != null;
        }
    }

}
