using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Factories
{
    public class FunctionFactory<T> : IFactory
    {
        public FunctionFactory(Func<T> createInstanceFunction) { this.createInstanceFunction = createInstanceFunction; instanceSetted = true; }
        public FunctionFactory(Func<IEnumerable<T>> createInstancesFunction) { this.createInstancesFunction = createInstancesFunction; instancesSetted = true; }
        bool instanceSetted = false;
        Func<T> createInstanceFunction;
        bool instancesSetted = false;
        Func<IEnumerable<T>> createInstancesFunction;
        public object Get(Type type)
        {
            if (typeof(T) != type) throw new FactoryCreationException($"FunctionFactory<{typeof(T).Name}> only give instances of type '{typeof(T).Name}'");
            if (!instanceSetted) throw new FactoryCreationException($"This FunctionFactory<> only create single objects. Use '{nameof(GetMany)}' method to obtain multiple instances.");
            return createInstanceFunction();
        }

        public IEnumerable<object> GetMany(Type type)
        {
            if (typeof(T) != type) throw new FactoryCreationException($"FunctionFactory<{typeof(T).Name}> only give instances of type '{typeof(T).Name}'");
            if (!instancesSetted) throw new FactoryCreationException($"This FunctionFactory<> only create multiple objects. Use '{nameof(Get)}' method to obtain an instance.");
            return (IEnumerable<object>)createInstancesFunction();
        }
    }

}
