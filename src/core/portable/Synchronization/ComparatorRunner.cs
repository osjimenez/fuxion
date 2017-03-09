using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    internal class ComparatorRunner<TItemA, TItemB, TKey> : IComparatorRunner
        where TItemA : class
        where TItemB : class
    {
        public ComparatorRunner(Comparator<TItemA, TItemB, TKey> definition)
        {
            this.definition = definition;
        }
        Comparator<TItemA, TItemB, TKey> definition;
        public ICollection<IComparatorResultInternal> CompareSides(ISideRunner sideA, ISideRunner sideB, bool runInverted)
        {
            return Printer.Indent($"Comparing sides '{sideA?.Definition.Name}' == '{sideB?.Definition.Name}' {(runInverted ? "INVERTED" : "")}", () =>
            {
                var res = new List<IComparatorResultInternal>();
                Dictionary<TKey, Tuple<LoadedItem, LoadedItem>> dic = new Dictionary<TKey, Tuple<LoadedItem, LoadedItem>>();
                if (sideA?.Entries != null)
                    foreach (var item in sideA?.Entries)
                    {
                        var tup = new Tuple<LoadedItem, LoadedItem>(item, new LoadedItem
                        {
                            Item = null,
                            //Sides = sideB.SubSides?.Select(side =>
                            //{
                            //    var clon = side.Clone();
                            //    clon.Source = null;
                            //    clon.Definition.Name = $"{clon.Definition.Name} ({sideA.GetItemName(item.Item)})";
                            //    return clon;
                            //}).ToList() ?? Enumerable.Empty<ISideRunner>().ToList()
                            Sides = Enumerable.Empty<ISideRunner>().ToList()
                        });
                        if(sideB?.SubSides != null)
                            foreach(var subSide in (runInverted ? sideA : sideB).SubSides)
                            {
                                var clon = subSide.Clone();
                                clon.Source = null;
                                //clon.Definition.Name = $"{clon.Definition.Name} ({sideA.GetItemName(item.Item)})";
                                clon.Definition.Name = $"{clon.Definition.Name.Replace("%sourceName%", sideA.GetItemName(item.Item))}";
                                var sides = runInverted ? tup.Item1.Sides : tup.Item2.Sides;
                                if (!sides.Any(s => s.Definition.Name == clon.Definition.Name))
                                {
                                    Printer.WriteLine($"Creating side-clon '{clon.Definition.Name}-{clon.Definition.IsMaster}' with source '{sideA.GetItemName(item.Item)}'");
                                    if (runInverted) tup.Item1.Sides.Add(clon);
                                    else tup.Item2.Sides.Add(clon);
                                }
                            }
                        dic.Add(definition.OnSelectKeyA.Invoke((TItemA)item.Item), tup);
                    }
                if (sideB?.Entries != null)
                    foreach (var item in sideB?.Entries)
                    {
                        var key = definition.OnSelectKeyB.Invoke((TItemB)item.Item);
                        if (dic.ContainsKey(key))
                            dic[key] = new Tuple<LoadedItem, LoadedItem>(dic[key].Item1, item);
                        else
                        {
                            var tup = new Tuple<LoadedItem, LoadedItem>(new LoadedItem
                            {
                                Item = null,
                                //Sides = sideA.SubSides
                                Sides = Enumerable.Empty<ISideRunner>().ToList()
                            }, item);
                            if(sideA?.SubSides != null)
                                foreach(var subSide in (runInverted ? sideA : sideB).SubSides)
                                {
                                    var clon = subSide.Clone();
                                    clon.Source = null;
                                    clon.Definition.Name = $"{clon.Definition.Name} ({sideB.GetItemName(item.Item)})";
                                    Printer.WriteLine($"Creating side-clon '{clon.Definition.Name}-{clon.Definition.IsMaster}' with source '{sideB.GetItemName(item.Item)}'");
                                    if(runInverted) tup.Item1.Sides.Add(clon);
                                    else tup.Item2.Sides.Add(clon);
                                }
                            dic.Add(key, tup);
                        }
                    }
                foreach (var tup in dic.Values)
                {
                    var r = CompareItems(tup.Item1, tup.Item2, runInverted);//tup.Item2.Sides.Any(s => s.Definition.IsMaster));
                    if (r != null)
                        res.Add(r);
                }
                return res;
            });
        }
        public IComparatorResultInternal CompareItems(LoadedItem itemA, LoadedItem itemB, bool runInverted)
        {
            IComparatorResultInternal res = runInverted
                ? (IComparatorResultInternal)new ComparatorResult<TItemB, TItemA, TKey>()
                {
                    Key = itemB?.Item != null ? definition.OnSelectKeyB((TItemB)itemB.Item) : definition.OnSelectKeyA((TItemA)itemA.Item),
                    MasterItem = itemB?.Item != null ? (TItemB)itemB.Item : null,
                    SideItem = itemA?.Item != null ? (TItemA)itemA.Item : null,
                }
                : new ComparatorResult<TItemA, TItemB, TKey>()
                {
                    Key = itemA?.Item != null ? definition.OnSelectKeyA((TItemA)itemA.Item) : definition.OnSelectKeyB((TItemB)itemB.Item),
                    MasterItem = itemA?.Item != null ? (TItemA)itemA.Item : null,
                    SideItem = itemB?.Item != null ? (TItemB)itemB.Item : null,
                };
            if (itemA?.Item != null && itemB?.Item != null)
            {
                if (itemA?.Item != null && !(itemA?.Item is TItemA)) throw new InvalidCastException($"Parameter '{nameof(itemA)}' must be of type '{typeof(TItemA).FullName}'");
                if (itemB?.Item != null && !(itemB?.Item is TItemB)) throw new InvalidCastException($"Parameter '{nameof(itemB)}' must be of type '{typeof(TItemB).FullName}'");
                //if (definition.OnCompare == null && definition.PropertiesComparator == null) throw new ArgumentException($"You must define '{nameof(definition.OnCompare)}' or '{nameof(definition.PropertiesComparator)}' to make it works");
                //definition.OnCompare?.Invoke((TItemA)itemA.Item, (TItemB)itemB.Item, res);
                if (definition.PropertiesComparator != null)
                    foreach (var pro in definition.PropertiesComparator.Compare((TItemA)itemA.Item, (TItemB)itemB.Item, runInverted))
                        res.Properties.Add(pro);
            }
            var processSubSides = new Action<LoadedItem, LoadedItem>((a, b) =>
            {
                //if (a.Item == null || b.Item == null) return;
                foreach (var side in b.Sides)
                {
                    if (side.GetItemType() == side.Comparator.GetItemTypes().Item1)
                    {
                        var side2 = a.Sides.SingleOrDefault(s => s.GetItemType() == side.Comparator.GetItemTypes().Item2);
                        side.Results = side.Comparator.CompareSides(side, side2, true);
                    }
                    else
                    {
                        var side2 = a.Sides.SingleOrDefault(s => s.GetItemType() == side.Comparator.GetItemTypes().Item1);
                        side.Results = side.Comparator.CompareSides(side2, side, false);
                    }
                    res.SubSides.Add(side);
                }
            });
            if (runInverted)
                //CompareItems(itemA, itemB, true);
                processSubSides(itemB, itemA);
            else
                //CompareItems(itemB, itemA, false);
                processSubSides(itemA, itemB);
                return res;
        }
        public object MapAToB(object itemA, object itemB) => definition.OnMapAToB((TItemA)itemA, (TItemB)itemB);
        public object MapBToA(object itemB, object itemA) => definition.OnMapBToA((TItemB)itemB, (TItemA)itemA);
    }
}
