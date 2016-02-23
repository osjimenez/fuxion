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
        protected override string GetTypeName() { return nameof(Category); }

        protected override IList<Discriminator> Exclusions { get { return null; } }
        protected override IList<Discriminator> Inclusions { get { return null; } }
    }
}
