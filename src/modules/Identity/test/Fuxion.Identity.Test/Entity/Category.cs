using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    [Discriminator("CAT")]
    [Table(nameof(Category))]
    public class Category : Discriminator
    {
        public Category Parent { get; set; }
        public IEnumerable<Category> Children { get; set; }

        protected sealed override string GetTypeId() => "CAT";
        protected sealed override string GetTypeName() => nameof(Category);

        protected override IEnumerable<Discriminator> GetInclusions() => Children;
        protected override IEnumerable<Discriminator> GetExclusions() => new[] { Parent };
    }
}
