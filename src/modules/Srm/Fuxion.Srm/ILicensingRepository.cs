using System.Collections.Generic;
using System.Linq;

namespace Fuxion.Srm
{
    public interface ILicensingRepository
    {
        IQueryable<License> Query();
        void Add(License license);
        bool Remove(License license);
    }
}