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
        protected override IList<Location> GetExclusions() { return new[] { State }; }

        protected override IList<Location> GetInclusions() { return new Location[] { }; }
    }
}
