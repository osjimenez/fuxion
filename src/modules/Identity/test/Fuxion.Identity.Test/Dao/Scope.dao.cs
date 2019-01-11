using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Dao
{
    [Table(nameof(ScopeDao))]
    public class ScopeDao : BaseDao, IScope
    {
        public IDiscriminator Discriminator { get; set; }
        public ScopePropagation Propagation { get; set; }
        public override string ToString()
        {
            return this.ToOneLineString();
        }

        IDiscriminator IScope.Discriminator { get { return Discriminator; } }
    }
}
