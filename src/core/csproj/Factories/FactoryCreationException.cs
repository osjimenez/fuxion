using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Factories
{
    public class FactoryCreationException : FuxionException
    {
        public FactoryCreationException(string message) : base(message) { }
        public FactoryCreationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
