using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Factories
{
    public interface ICheckableInjector : IFactoryInjector
    {
        bool CheckGet(Type type);
        bool CheckGetMany(Type type);
    }
}
