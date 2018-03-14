#if (NET471)
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Data
{
    public class TrackingConvention : Convention
    {
        public TrackingConvention(IEnumerable<Type> trackAttributes = null)
        {
            ColumnAnnotationAttribute.Annotate(this, trackAttributes);
        }
    }
}
#endif