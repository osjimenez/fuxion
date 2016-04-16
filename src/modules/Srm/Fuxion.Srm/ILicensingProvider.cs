using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Srm
{
    public interface ILicensingProvider
    {
        License Create(LicenseContent content);
        License Refresh(License oldLicense);
    }
}
