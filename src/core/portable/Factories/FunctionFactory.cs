using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Factories
{
    public class FunctionFactory<T> : IFactory
    {
        public FunctionFactory(Func<T> createInstanceFunction) { this.createInstanceFunction = createInstanceFunction; }
        public FunctionFactory(Func<IEnumerable<T>> createInstancesFunction) { this.createInstancesFunction = createInstancesFunction; }
        bool instanceSetted = false;
        Func<T> createInstanceFunction;
        bool instancesSetted = false;
        Func<IEnumerable<T>> createInstancesFunction;
        public object Get(Type type)
        {
            if(!instanceSetted) throw new NotImplementedException();
            return createInstanceFunction();
        }

        public IEnumerable<object> GetMany(Type type)
        {
            if (!instancesSetted) throw new NotImplementedException();
            return (IEnumerable<object>)createInstancesFunction();
        }
    }
}
