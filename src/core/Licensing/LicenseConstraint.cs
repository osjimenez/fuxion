using Fuxion.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Licensing
{
    public class LicenseConstraint
    {
        protected virtual bool Validate(out string validationMessage)
        {
            validationMessage = "Success";
            return true;
        }
    }
}
