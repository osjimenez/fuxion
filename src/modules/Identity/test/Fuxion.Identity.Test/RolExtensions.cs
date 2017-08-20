using Fuxion.Identity.Test.Dao;
using Fuxion.Identity.Test.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test
{
    public static class RolExtensions
    {
        //public static bool AllLocations(this IRolCan me, params LocationDto[] locations) => me.ByAll(locations);
        //public static bool AnyLocations(this IRolCan me, params LocationDto[] locations) => me.ByAny(locations);
        //public static bool AllLocations(this IRolCan me, params LocationDao[] locations) => me.ByAll(locations);
        //public static bool AnyLocations(this IRolCan me, params LocationDao[] locations) => me.ByAny(locations);
        public static bool AllLocations(this IRolCan me, params LocationDto[] locations) => me.AllInstances(locations);
        public static bool AnyLocations(this IRolCan me, params LocationDto[] locations) => me.AnyInstance(locations);
        public static bool AllLocations(this IRolCan me, params LocationDao[] locations) => me.AllInstances(locations);
        public static bool AllLocations2(this IRolCan me, params LocationDao[] locations) => me.AllInstances2(locations);
        public static bool AnyLocations(this IRolCan me, params LocationDao[] locations) => me.AnyInstance(locations);
    }
}
