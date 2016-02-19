using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.DatabaseTest.Entity
{
    public class Country : Location
    {
        public IEnumerable<State> States { get; set; }

        protected override IEnumerable<Location> GetExclusions() { return new Location[] { }; }
        protected override IEnumerable<Location> GetInclusions() { return States; }
    }
}
