using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    class State : Location
    {
        public IEnumerable<City> Cities { get; set; }
        public Country Country { get; set; }

        protected override IEnumerable<Location> GetExclusions() { return new[] { Country }; }
        protected override IEnumerable<Location> GetInclusions() { return Cities; }
    }
}
