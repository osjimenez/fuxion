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
        public Func<TItemA, TItemB, TItemB> OnMapAToB { get; set; }
        public Func<TItemB, TItemA, TItemA> OnMapBToA { get; set; }
        public PropertiesComparator<TItemA, TItemB> PropertiesComparator { get; set; } = PropertiesComparator<TItemA, TItemB>.WithAutoDiscoverProperties();
    }
}