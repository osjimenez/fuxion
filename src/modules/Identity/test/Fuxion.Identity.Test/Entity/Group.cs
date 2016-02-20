using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    [Table(nameof(Group))]
    public partial class Group : Rol
    {
        public IList<Rol> Rols{ get; set; }
    }
}
