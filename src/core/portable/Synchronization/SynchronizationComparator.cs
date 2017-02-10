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
        ICollection<ISynchronizationComparatorResultInternal> CompareItems(ISynchronizationSideInternal sideA, ISynchronizationSideInternal sideB, bool runInverted = false);
        //ICollection<ISynchronizationComparatorResultInternal> CompareItems(IEnumerable<SynchronizationLoadedItem> itemsA, IEnumerable<SynchronizationLoadedItem> itemsB, bool runInverted = false);
        ISynchronizationComparatorResultInternal CompareItem(SynchronizationLoadedItem itemA, SynchronizationLoadedItem itemB, bool runInverted = false);
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

        ICollection<ISynchronizationComparatorResultInternal> ISynchronizationComparatorInternal.CompareItems(ISynchronizationSideInternal sideA, ISynchronizationSideInternal sideB, bool runInverted)
        {
            var res = new List<ISynchronizationComparatorResultInternal>();
            Dictionary<TKey, Tuple<SynchronizationLoadedItem, SynchronizationLoadedItem>> dic = new Dictionary<TKey, Tuple<SynchronizationLoadedItem, SynchronizationLoadedItem>>();
            foreach (var item in sideA.Entries)
                dic.Add(OnSelectKeyA.Invoke((TItemA)item.Item), new Tuple<SynchronizationLoadedItem, SynchronizationLoadedItem>(item, new SynchronizationLoadedItem
                {
                    Item = null,
                    Sides = sideB.SubSides
                }));
            foreach (var item in sideB.Entries)
            {
                var key = OnSelectKeyB.Invoke((TItemB)item.Item);
                if (dic.ContainsKey(key))
                    dic[key] = new Tuple<SynchronizationLoadedItem, SynchronizationLoadedItem>(dic[key].Item1, item);
                else
                    dic.Add(key, new Tuple<SynchronizationLoadedItem, SynchronizationLoadedItem>(new SynchronizationLoadedItem
                    {
                        Item = null,
                        Sides = sideA.SubSides
                    }, item));
            }
            foreach (var tup in dic.Values)
            {
                var r = ((ISynchronizationComparatorInternal)this).CompareItem(tup.Item1, tup.Item2, runInverted);
                if (r != null)
                    res.Add(r);
            }
            return res;
        }

        //ICollection<ISynchronizationComparatorResultInternal> ISynchronizationComparatorInternal.CompareItems(IEnumerable<SynchronizationLoadedItem> itemsA, IEnumerable<SynchronizationLoadedItem> itemsB, bool runInverted)
        //{
        //    var res = new List<ISynchronizationComparatorResultInternal>();
        //    Dictionary<TKey, Tuple<SynchronizationLoadedItem, SynchronizationLoadedItem>> dic = new Dictionary<TKey, Tuple<SynchronizationLoadedItem, SynchronizationLoadedItem>>();
        //    foreach (var item in itemsA)
        //        dic.Add(OnSelectKeyA.Invoke((TItemA)item.Item), new Tuple<SynchronizationLoadedItem, SynchronizationLoadedItem>(item, null));
        //    foreach (var item in itemsB)
        //    {
        //        var key = OnSelectKeyB.Invoke((TItemB)item.Item);
        //        if (dic.ContainsKey(key))
        //            dic[key] = new Tuple<SynchronizationLoadedItem, SynchronizationLoadedItem>(dic[key].Item1, item);
        //        else
        //            dic.Add(key, new Tuple<SynchronizationLoadedItem, SynchronizationLoadedItem>(null, item));
        //    }
        //    foreach (var tup in dic.Values)
        //    {
        //        var r = ((ISynchronizationComparatorInternal)this).CompareItem(tup.Item1, tup.Item2, runInverted);
        //        if (r != null)
        //            res.Add(r);
        //    }
        //    return res;
        //}
        ISynchronizationComparatorResultInternal ISynchronizationComparatorInternal.CompareItem(SynchronizationLoadedItem itemA, SynchronizationLoadedItem itemB, bool runInverted)
        {
            //if (runInverted)
            //{
            //    var aux = itemA;
            //    itemA = itemB;
            //    itemB = aux;
            //}
            ISynchronizationComparatorResultInternal res;
            if (itemA?.Item == null || itemB?.Item == null)
            {
                if (runInverted)
                    res = new SynchronizationComparatorResult<TItemB, TItemA, TKey>()
                    {
                        Key = itemB?.Item != null ? OnSelectKeyB((TItemB)itemB.Item) : OnSelectKeyA((TItemA)itemA.Item),
                        MasterItem = itemB?.Item != null ? (TItemB)itemB.Item : null,
                        SideItem = itemA?.Item != null ? (TItemA)itemA.Item : null,
                    };
                else
                    res = new SynchronizationComparatorResult<TItemA, TItemB, TKey>()
                    {
                        Key = itemA?.Item != null ? OnSelectKeyA((TItemA)itemA.Item) : OnSelectKeyB((TItemB)itemB.Item),
                        MasterItem = itemA?.Item != null ? (TItemA)itemA.Item : null,
                        SideItem = itemB?.Item != null ? (TItemB)itemB.Item : null,
                    };
            }
            else
            {
                if (itemA?.Item != null && !(itemA?.Item is TItemA)) throw new InvalidCastException($"Parameter '{nameof(itemA)}' must be of type '{typeof(TItemA).FullName}'");
                if (itemB?.Item != null && !(itemB?.Item is TItemB)) throw new InvalidCastException($"Parameter '{nameof(itemB)}' must be of type '{typeof(TItemB).FullName}'");

                if (runInverted)
                {
                    res = new SynchronizationComparatorResult<TItemB, TItemA, TKey>
                    {
                        MasterItem = (TItemB)itemB.Item,
                        SideItem = (TItemA)itemA.Item,
                        Key = itemB?.Item != null ? OnSelectKeyB((TItemB)itemB.Item) : OnSelectKeyA((TItemA)itemA.Item)
                    };
                }
                else
                {
                    res = new SynchronizationComparatorResult<TItemA, TItemB, TKey>
                    {
                        MasterItem = (TItemA)itemA.Item,
                        SideItem = (TItemB)itemB.Item,
                        Key = itemA?.Item != null ? OnSelectKeyA((TItemA)itemA.Item) : OnSelectKeyB((TItemB)itemB.Item)
                    };
                }
                if (itemA?.Item != null && itemB?.Item != null)
                    OnCompare((TItemA)itemA.Item, (TItemB)itemB.Item, res);
            }
            if (runInverted)
            {
                // Comparator is in ItemA that correspond with SideItem, not MasterItem
                foreach(var side in itemA.Sides)
                {
                    if(side.GetItemType() == side.Comparator.GetItemTypes().Item1)
                    {
                        var side2 = itemB.Sides.SingleOrDefault(s => s.GetItemType() == side.Comparator.GetItemTypes().Item2);
                        side.Results = side.Comparator.CompareItems(side, side2, true);
                    }
                    else
                    {
                        var side2 = itemB.Sides.SingleOrDefault(s => s.GetItemType() == side.Comparator.GetItemTypes().Item1);
                        side.Results = side.Comparator.CompareItems(side2, side, false);
                    }
                    res.SubSides.Add(side);
                }
            }else
            {
                // Comparator is in ItemB that correspond with SideItem, not MasterItem
                foreach (var side in itemB.Sides)
                {
                    if (side.GetItemType() == side.Comparator.GetItemTypes().Item1)
                    {
                        var side2 = itemA.Sides.SingleOrDefault(s => s.GetItemType() == side.Comparator.GetItemTypes().Item2);
                        side.Results = side.Comparator.CompareItems(side, side2, true);
                    }
                    else
                    {
                        var side2 = itemA.Sides.SingleOrDefault(s => s.GetItemType() == side.Comparator.GetItemTypes().Item1);
                        side.Results = side.Comparator.CompareItems(side2, side, false);
                    }
                    res.SubSides.Add(side);
                }
            }
            return res;
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
        ICollection<ISynchronizationProperty> Properties { get; }
        ICollection<ISynchronizationSideInternal> SubSides { get; set; }
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
        public ICollection<ISynchronizationProperty> Properties { get; } = new List<ISynchronizationProperty>();
        public ICollection<ISynchronizationSideInternal> SubSides { get; set; } = new List<ISynchronizationSideInternal>();
        public void AddProperty<TPropertyA, TPropertyB>(string propertyName, TPropertyA aValue, TPropertyB bValue)
        {
            Properties.Add(new SynchronizationProperty<TPropertyA, TPropertyB>(propertyName, aValue, bValue));
        }
    }
}
