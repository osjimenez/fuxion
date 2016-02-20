using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    [Table(nameof(State))]
    public class State : Location
    {
        public IList<City> Cities { get; set; }
        public Country Country { get; set; }

        protected override IList<Location> GetExclusions() { return new[] { Country }; }
        protected override IList<Location> GetInclusions() { return Cities.Cast<Location>().ToList(); }
    }
}
