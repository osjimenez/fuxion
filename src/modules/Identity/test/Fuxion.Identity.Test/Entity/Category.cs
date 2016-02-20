using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    [Discriminator(TypeId)]
    [Table(nameof(Category))]
    public partial class Category : Discriminator
    {
        const string TypeId = "CAT";
        protected override object GetTypeId() { return TypeId; }
    }
}
