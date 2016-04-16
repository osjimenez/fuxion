using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Srm
{
    public class InvalidKeyException : FuxionException
    {
        public InvalidKeyException(string message) : base(message) { }
    }
}
