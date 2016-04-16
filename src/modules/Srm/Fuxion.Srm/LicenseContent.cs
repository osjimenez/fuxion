using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Srm
{
    public abstract class LicenseContent
    {
        protected internal abstract bool Validate(out string validationMessage);
    }
}
