using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    class Order : Base
    {
        public Department Domain { get; set; }
        [DiscriminatedBy(typeof(Department))]
        public string DomainId { get; set; }

        public City ShipmentCity { get; set; }
        [DiscriminatedBy(typeof(City))]
        public string ShipmentCityId { get; set; }
    }
}
