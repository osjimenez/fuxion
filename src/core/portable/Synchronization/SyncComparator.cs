using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    public interface ISyncComparator
    {
        IEnumerable<ISyncComparatorResult> CompareItems(IEnumerable<object> itemsA, IEnumerable<object> itemsB);
        ISyncComparatorResult CompareItem(object itemA, object itemB);
        object MapAToB(object itemA, object itemB);
        object MapBToA(object itemB, object itemA);
    }
    public class SyncComparator<TItemA, TItemB, TKey> : ISyncComparator
        where TItemA : class
        where TItemB : class
    {
        public Func<TItemA, TKey> KeyASelector { get; set; }
        public Func<TItemB, TKey> KeyBSelector { get; set; }

        //public Func<TItemA,TItemB,ISyncItemPreview> Function { get; set; }
        public Action<TItemA, TItemB, SyncComparatorResult<TItemA, TItemB, TKey>> Function { get; set; }


        public IEnumerable<ISyncComparatorResult> CompareItems(IEnumerable<object> itemsA, IEnumerable<object> itemsB)
        {
            Dictionary<TKey, Tuple<TItemA, TItemB>> dic = new Dictionary<TKey, Tuple<TItemA, TItemB>>();
            foreach (var item in itemsA)
                dic.Add(KeyASelector.Invoke((TItemA)item), new Tuple<TItemA, TItemB>((TItemA)item, null));
            foreach (var item in itemsB)
            {
                var key = KeyBSelector.Invoke((TItemB)item);
                if (dic.ContainsKey(key))
                    dic[key] = new Tuple<TItemA, TItemB>(dic[key].Item1, (TItemB)item);
                else
                    dic.Add(key, new Tuple<TItemA, TItemB>(null, (TItemB)item));
            }
            foreach (var tup in dic.Values)
            {
                var res = CompareItem(tup.Item1, tup.Item2);
                if (res != null)
                    yield return res;
            }
        }
        public ISyncComparatorResult CompareItem(object itemA, object itemB)
        {
            if (itemA == null || itemB == null) return new SyncComparatorResult<TItemA, TItemB, TKey>()
            {
                Key = itemA != null ? KeyASelector((TItemA)itemA) : KeyBSelector((TItemB)itemB),
                MasterItem = itemA != null ? (TItemA)itemA : null,
                SideItem = itemB != null ? (TItemB)itemB : null,
            };
            if (itemA != null && !(itemA is TItemA)) throw new InvalidCastException($"Parameter '{nameof(itemA)}' must be of type '{typeof(TItemA).FullName}'");
            if (itemB != null && !(itemB is TItemB)) throw new InvalidCastException($"Parameter '{nameof(itemB)}' must be of type '{typeof(TItemB).FullName}'");
            var res = new SyncComparatorResult<TItemA, TItemB, TKey>
            {
                MasterItem = (TItemA)itemA,
                SideItem = (TItemB)itemB,
                Key = itemA != null ? KeyASelector((TItemA)itemA) : KeyBSelector((TItemB)itemB)
            };
            if (itemA != null && itemB != null)
                Function((TItemA)itemA, (TItemB)itemB, res);
            return res.Count > 0 ? res : null;
        }

        public Func<TItemA, TItemB, TItemB> MapAToB { get; set; }
        public Func<TItemB, TItemA, TItemA> MapBToA { get; set; }
        object ISyncComparator.MapAToB(object itemA, object itemB) => MapAToB((TItemA)itemA, (TItemB)itemB);
        object ISyncComparator.MapBToA(object itemB, object itemA) => MapBToA((TItemB)itemB, (TItemA)itemA);
    }
    public interface ISyncComparatorResult : IEnumerable<ISyncPropertyPreview>
    {
        object Key { get; }
        object MasterItem { get; }
        object SideItem { get; }
    }
    public class SyncComparatorResult<TItemA, TItemB, TKey> : List<ISyncPropertyPreview>, ISyncComparatorResult
    {
        public TKey Key { get; set; }
        object ISyncComparatorResult.Key { get { return Key; } }
        public TItemA MasterItem { get; set; }
        object ISyncComparatorResult.MasterItem { get { return MasterItem; } }
        public TItemB SideItem { get; set; }
        object ISyncComparatorResult.SideItem { get { return SideItem; } }

        public void AddProperty<TPropertyA, TPropertyB>(string propertyName, TPropertyA aValue, TPropertyB bValue)
        {
            Add(new SyncPropertyPreview<TPropertyA, TPropertyB>(propertyName, aValue, bValue));
        }
    }
}
