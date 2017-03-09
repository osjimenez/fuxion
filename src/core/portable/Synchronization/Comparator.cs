using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
namespace Fuxion.Synchronization
{
    public class Comparator<TItemA, TItemB, TKey> : IComparator
            where TItemA : class
            where TItemB : class
    {
        public Func<TItemA, TKey> OnSelectKeyA { get; set; }
        public Func<TItemB, TKey> OnSelectKeyB { get; set; }
        //public Action<TItemA, TItemB, IComparatorResult> OnCompare { get; set; }
        public Func<TItemA, TItemB, TItemB> OnMapAToB { get; set; }
        public Func<TItemB, TItemA, TItemA> OnMapBToA { get; set; }
        //public IEnumerable<Tuple<Expression<Func<TItemA, object>>, Expression<Func<TItemB, object>>>> OnCompare2 { get; set; }
        public PropertiesComparator<TItemA, TItemB> PropertiesComparator { get; set; } = new PropertiesComparator<TItemA, TItemB>();
    }
}