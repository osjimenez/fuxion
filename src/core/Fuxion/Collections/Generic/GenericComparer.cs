using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Collections.Generic
{
    public class GenericComparer<T> : Comparer<T>
    {
        public GenericComparer(Func<T?, T?, int> comparisonFunction)
        {
            this.comparisonFunction = comparisonFunction;
        }
        Func<T?, T?, int> comparisonFunction;
        public override int Compare(T? x, T? y)
        {
            return comparisonFunction(x, y);
        }
    }
}
