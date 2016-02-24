using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    public class Demo : Base
    {
        public City ShipmentCity { get; set; }
        [DiscriminatedBy(typeof(Location))]
        public string ShipmentCityId { get; set; }

        public City ReceptionCity { get; set; }
        [DiscriminatedBy(typeof(Location))]
        public string ReceptionCityId { get; set; }
    }
}
