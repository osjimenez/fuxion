using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.ComponentModel
{
    public class NotifierException : FuxionException
    {
        internal NotifierException(string message) : base(message) { }
    }
}
