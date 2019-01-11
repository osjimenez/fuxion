using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Licensing
{
    public class LicenseValidationException : FuxionException
    {
        public LicenseValidationException(string message) : base(message) { }
    }
}
