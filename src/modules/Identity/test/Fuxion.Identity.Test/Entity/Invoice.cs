using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    [Table(nameof(Invoice))]
    public class Invoice : Base
    {
        public Department Department { get; set; }
        [DiscriminatedBy(typeof(Department))]
        public string DepartmentId { get; set; }
    }
}
