using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    class City : Location
    {
        public State State { get; set; }
        protected override IEnumerable<Location> GetExclusions() { return new[] { State }; }

        protected override IEnumerable<Location> GetInclusions() { return new Location[] { }; }
    }
}
