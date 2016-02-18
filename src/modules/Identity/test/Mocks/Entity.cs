using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Mocks
{
    class Entity : BaseEntity
    {
        public DiscriminatorEntity Dis { get; set; }
        [DiscriminatedBy(typeof(DiscriminatorEntity))]
        public string DisId { get; set; }
    }
}
