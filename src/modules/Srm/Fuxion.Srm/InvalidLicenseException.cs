using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Srm
{
    public class InvalidLicenseException : FuxionException
    {
        public InvalidLicenseException(string message) : base(message) { }
    }
}
