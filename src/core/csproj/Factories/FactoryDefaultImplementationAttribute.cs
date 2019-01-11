using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Factories
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class FactoryDefaultImplementationAttribute : Attribute
    {
        public FactoryDefaultImplementationAttribute(Type type) { Type = type; }
        public Type Type { get; set; }
    }
}
