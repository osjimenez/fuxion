using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    [Table(nameof(Country))]
    public class Country : Location
    {
        public IList<State> States { get; set; }

        protected override IEnumerable<Location> GetLocationExclusions() { return new Location[] { }; }
        protected override IEnumerable<Location> GetLocationInclusions() { return States.Cast<Location>().ToList(); }
    }
}
