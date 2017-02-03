using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    public interface ISynchronizationComparator { }
    internal interface ISynchronizationComparatorInternal : ISynchronizationComparator
    {
        IEnumerable<ISynchronizationComparatorResultInternal> CompareItems(IEnumerable<object> itemsA, IEnumerable<object> itemsB);
        ISynchronizationComparatorResultInternal CompareItem(object itemA, object itemB);
        object MapAToB(object itemA, object itemB);
        object MapBToA(object itemB, object itemA);
    }
    public class SynchronizationComparator<TItemA, TItemB, TKey> : ISynchronizationComparatorInternal
        where TItemA : class
        where TItemB : class
    {
        public Func<TItemA, TKey> OnSelectKeyA { get; set; }
        public Func<TItemB, TKey> OnSelectKeyB { get; set; }
        public Action<TItemA, TItemB, ISynchronizationComparatorResult> OnCompare { get; set; }
        public Func<TItemA, TItemB, TItemB> OnMapAToB { get; set; }
        public Func<TItemB, TItemA, TItemA> OnMapBToA { get; set; }

        IEnumerable<ISynchronizationComparatorResultInternal> ISynchronizationComparatorInternal.CompareItems(IEnumerable<object> itemsA, IEnumerable<object> itemsB)
        {
            Dictionary<TKey, Tuple<TItemA, TItemB>> dic = new Dictionary<TKey, Tuple<TItemA, TItemB>>();
            foreach (var item in itemsA)
                dic.Add(OnSelectKeyA.Invoke((TItemA)item), new Tuple<TItemA, TItemB>((TItemA)item, null));
            foreach (var item in itemsB)
            {
                var key = OnSelectKeyB.Invoke((TItemB)item);
                if (dic.ContainsKey(key))
                    dic[key] = new Tuple<TItemA, TItemB>(dic[key].Item1, (TItemB)item);
                else
                    dic.Add(key, new Tuple<TItemA, TItemB>(null, (TItemB)item));
            }
            foreach (var tup in dic.Values)
            {
                var res = ((ISynchronizationComparatorInternal)this).CompareItem(tup.Item1, tup.Item2);
                if (res != null)
                    yield return res;
            }
        }
        ISynchronizationComparatorResultInternal ISynchronizationComparatorInternal.CompareItem(object itemA, object itemB)
        {
            if (itemA == null || itemB == null) return new SynchronizationComparatorResult<TItemA, TItemB, TKey>()
            {
                Key = itemA != null ? OnSelectKeyA((TItemA)itemA) : OnSelectKeyB((TItemB)itemB),
                MasterItem = itemA != null ? (TItemA)itemA : null,
                SideItem = itemB != null ? (TItemB)itemB : null,
            };
            if (itemA != null && !(itemA is TItemA)) throw new InvalidCastException($"Parameter '{nameof(itemA)}' must be of type '{typeof(TItemA).FullName}'");
            if (itemB != null && !(itemB is TItemB)) throw new InvalidCastException($"Parameter '{nameof(itemB)}' must be of type '{typeof(TItemB).FullName}'");
            var res = new SynchronizationComparatorResult<TItemA, TItemB, TKey>
            {
                MasterItem = (TItemA)itemA,
                SideItem = (TItemB)itemB,
                Key = itemA != null ? OnSelectKeyA((TItemA)itemA) : OnSelectKeyB((TItemB)itemB)
            };
            if (itemA != null && itemB != null)
                OnCompare((TItemA)itemA, (TItemB)itemB, res);
            return res.Properties.Count > 0 ? res : null;
        }
        object ISynchronizationComparatorInternal.MapAToB(object itemA, object itemB) => OnMapAToB((TItemA)itemA, (TItemB)itemB);
        object ISynchronizationComparatorInternal.MapBToA(object itemB, object itemA) => OnMapBToA((TItemB)itemB, (TItemA)itemA);
    }
    public interface ISynchronizationComparatorResult
    {
        void AddProperty<TPropertyA, TPropertyB>(string propertyName, TPropertyA aValue, TPropertyB bValue);
    }
    internal interface ISynchronizationComparatorResultInternal : ISynchronizationComparatorResult
    {
        object Key { get; }
        object MasterItem { get; }
        object SideItem { get; }
        IList<ISynchronizationProperty> Properties { get; }
    }
    //internal interface ISyncComparatorResultInternal<TItemA, TItemB, TKey> : ISyncComparatorResultInternal
    //{
    //    TKey Key { get; }
    //    TItemA MasterItem { get; }
    //    TItemB SideItem { get; }
    //}
    internal class SynchronizationComparatorResult<TItemA, TItemB, TKey> : ISynchronizationComparatorResultInternal//<TItemA,TItemB,TKey>
    {
        public TKey Key { get; set; }
        object ISynchronizationComparatorResultInternal.Key { get { return Key; } }
        public TItemA MasterItem { get; set; }
        object ISynchronizationComparatorResultInternal.MasterItem { get { return MasterItem; } }
        public TItemB SideItem { get; set; }
        object ISynchronizationComparatorResultInternal.SideItem { get { return SideItem; } }
        public IList<ISynchronizationProperty> Properties { get; } = new List<ISynchronizationProperty>();

        public void AddProperty<TPropertyA, TPropertyB>(string propertyName, TPropertyA aValue, TPropertyB bValue)
        {
            Properties.Add(new SynchronizationProperty<TPropertyA, TPropertyB>(propertyName, aValue, bValue));
        }
    }
}
