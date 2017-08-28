using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Reflection
{
    // TODO - Implement a Reflection cached system to improve performance in reflection operations
    // Based in static methods as 'bool : Type<SomeType>().Is<OtherType>()'
    // All operations must be automatically cached and in next calls will search into the cache before make reflection operation
    // The cache is injected by the Factory, if no cache injected, the Reflection operation always will be done
    class Reflect
    {
    }
}
