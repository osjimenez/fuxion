using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Graph
{
    public class GraphCyclicException : FuxionException
    {
        public GraphCyclicException() : base() { }
        public GraphCyclicException(string message) : base(message) { }
        public GraphCyclicException(string message, Exception innerException) : base(message, innerException) { }
    }
}
