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

        protected override IList<Location> GetExclusions() { return new Location[] { }; }
        protected override IList<Location> GetInclusions() { return States.Cast<Location>().ToList(); }
    }
}
