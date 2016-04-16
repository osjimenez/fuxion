using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Licensing
{
    public interface ILicenseProvider
    {
        //License Request(LicenseContent content);
        LicenseContainer Request(LicenseRequest request);
        LicenseContainer Refresh(LicenseContainer oldLicense);
    }
}
