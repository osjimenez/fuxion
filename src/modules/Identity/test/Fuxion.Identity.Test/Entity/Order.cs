using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    [Table(nameof(Order))]
    public abstract class Order : Base
    {
        public Department Department { get; set; }
        [DiscriminatedBy(typeof(Department))]
        public string DepartmentId { get; set; }

        public City ShipmentCity { get; set; }
        [DiscriminatedBy(typeof(Location))]
        public string ShipmentCityId { get; set; }

        public City ReceptionCity { get; set; }
        [DiscriminatedBy(typeof(Location))]
        public string ReceptionCityId { get; set; }
    }
}
