using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    public class ComparatorDefinition<TItemA, TItemB, TKey> : IComparatorDefinition
            where TItemA : class
            where TItemB : class
    {
        public Func<TItemA, TKey> OnSelectKeyA { get; set; }
        public Func<TItemB, TKey> OnSelectKeyB { get; set; }
        public Action<TItemA, TItemB, IComparatorResult> OnCompare { get; set; }
        public Func<TItemA, TItemB, TItemB> OnMapAToB { get; set; }
        public Func<TItemB, TItemA, TItemA> OnMapBToA { get; set; }
    }
}