using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    [Table(nameof(City))]
    public class City : Location
    {
        public State State { get; set; }

        protected override IEnumerable<Location> GetLocationExclusions() { return new[] { State }; }

        protected override IEnumerable<Location> GetLocationInclusions() { return new Location[] { }; }
    }
}
