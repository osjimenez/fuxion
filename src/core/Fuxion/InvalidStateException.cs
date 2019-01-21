using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion
{
    public class InvalidStateException : FuxionException
    {
        public InvalidStateException() : base() { }
        public InvalidStateException(string message) : base(message) { }
        public InvalidStateException(string message, Exception innerException) : base(message, innerException) { }
    }
}
