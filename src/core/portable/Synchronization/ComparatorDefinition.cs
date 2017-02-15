using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    public class ComparatorDefinition<TItemA, TItemB, TKey> : IComparatorRunner
            where TItemA : class
            where TItemB : class
    {
        public Func<TItemA, TKey> OnSelectKeyA { get; set; }
        public Func<TItemB, TKey> OnSelectKeyB { get; set; }
        public Action<TItemA, TItemB, IComparatorResult> OnCompare { get; set; }
        public Func<TItemA, TItemB, TItemB> OnMapAToB { get; set; }
        public Func<TItemB, TItemA, TItemA> OnMapBToA { get; set; }

        ICollection<IComparatorResultInternal> IComparatorRunner.CompareItems(ISideRunner sideA, ISideRunner sideB, bool runInverted)
        {
            var res = new List<IComparatorResultInternal>();
            Dictionary<TKey, Tuple<LoadedItem, LoadedItem>> dic = new Dictionary<TKey, Tuple<LoadedItem, LoadedItem>>();
            foreach (var item in sideA?.Entries)
                dic.Add(OnSelectKeyA.Invoke((TItemA)item.Item), new Tuple<LoadedItem, LoadedItem>(item, new LoadedItem
                {
                    Item = null,
                    //Sides = Enumerable.Empty<ISynchronizationSideInternal>().ToList()
                    Sides = sideB.SubSides?.Select(side =>
                    {
                        var clon = side.Clone();
                        clon.Source = null;
                        clon.Name = $"{clon.Name} ({sideA.GetItemName(item.Item)})";
                        //clon.Load().Wait();
                        return clon;
                    }).ToList() ?? Enumerable.Empty<ISideRunner>().ToList()
                }));
            foreach (var item in sideB?.Entries)
            {
                var key = OnSelectKeyB.Invoke((TItemB)item.Item);
                if (dic.ContainsKey(key))
                    dic[key] = new Tuple<LoadedItem, LoadedItem>(dic[key].Item1, item);
                else
                    dic.Add(key, new Tuple<LoadedItem, LoadedItem>(new LoadedItem
                    {
                        Item = null,
                        //Sides = Enumerable.Empty<ISynchronizationSideInternal>().ToList()
                        Sides = sideA.SubSides
                    }, item));
            }
            foreach (var tup in dic.Values)
            {
                var r = ((IComparatorRunner)this).CompareItem(tup.Item1, tup.Item2, runInverted);
                if (r != null)
                    res.Add(r);
            }
            return res;
        }
        IComparatorResultInternal IComparatorRunner.CompareItem(LoadedItem itemA, LoadedItem itemB, bool runInverted)
        {
            IComparatorResultInternal res;
            if (itemA?.Item == null || itemB?.Item == null)
            {
                if (runInverted)
                    res = new ComparatorResult<TItemB, TItemA, TKey>()
                    {
                        Key = itemB?.Item != null ? OnSelectKeyB((TItemB)itemB.Item) : OnSelectKeyA((TItemA)itemA.Item),
                        MasterItem = itemB?.Item != null ? (TItemB)itemB.Item : null,
                        SideItem = itemA?.Item != null ? (TItemA)itemA.Item : null,
                    };
                else
                    res = new ComparatorResult<TItemA, TItemB, TKey>()
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
                    res = new ComparatorResult<TItemB, TItemA, TKey>
                    {
                        MasterItem = (TItemB)itemB.Item,
                        SideItem = (TItemA)itemA.Item,
                        Key = itemB?.Item != null ? OnSelectKeyB((TItemB)itemB.Item) : OnSelectKeyA((TItemA)itemA.Item)
                    };
                }
                else
                {
                    res = new ComparatorResult<TItemA, TItemB, TKey>
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
                foreach (var side in itemA.Sides)
                {
                    if (side.GetItemType() == side.Comparator.GetItemTypes().Item1)
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
            }
            else
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
        object IComparatorRunner.MapAToB(object itemA, object itemB) => OnMapAToB((TItemA)itemA, (TItemB)itemB);
        object IComparatorRunner.MapBToA(object itemB, object itemA) => OnMapBToA((TItemB)itemB, (TItemA)itemA);
    }
}