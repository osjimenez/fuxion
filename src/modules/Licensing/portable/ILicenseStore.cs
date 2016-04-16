using System.Collections.Generic;
using System.Linq;

namespace Fuxion.Licensing
{
    public interface ILicenseStore
    {
        IQueryable<LicenseContainer> Query();
        void Add(LicenseContainer license);
        bool Remove(LicenseContainer license);
    }
}