using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Fuxion.Synchronization
{
    public class WorkDefinition
    {
        public string Name { get; set; }
        public IEnumerable<ISideDefinition> Sides { get; set; }
        public IEnumerable<IComparatorDefinition> Comparators { get; set; }
        //internal Guid Id { get; } = Guid.NewGuid();
        //internal IEnumerable<ISideRunner> InternalSides { get; set; }
        //internal IList<IItem> Items { get; set; } = new List<IItem>();
        //internal ISideRunner MasterSide { get; set; }
        //internal Task<WorkPreview> PreviewAsync()
        //{
        //    return Printer.IndentAsync($"Work '{Name}'", async () =>
        //    {
        //        // Get master side
        //        var masters = InternalSides.Where(s => s.IsMaster);
        //        if (masters.Count() != 1) throw new ArgumentException($"One, and only one '{nameof(ISideDefinition)}' must be the master side");
        //        MasterSide = masters.Single();
        //        // Determine what sides are sub-sides of others
        //        Action<ISideRunner> populateSubSides = null;
        //        populateSubSides = new Action<ISideRunner>(side =>
        //        {
        //            side.SubSides = InternalSides.Where(s => s.GetSourceType() == side.GetItemType()).ToList();
        //            foreach (var s in side.SubSides)
        //                populateSubSides(s);
        //        });
        //        var rootSides = InternalSides.Where(s => !InternalSides.Any(s2 => s2.GetItemType() == s.GetSourceType())).OrderByDescending(s => s.IsMaster).ThenBy(s => s.Name).ToList();
        //        foreach (var side in rootSides)
        //            populateSubSides(side);
        //        Printer.Foreach("Sides tree:", rootSides, side =>
        //        {
        //            Action<ISideRunner> printSide = null;
        //            printSide = new Action<ISideRunner>(si =>
        //            {
        //                if (si.IsMaster) Printer.WriteLine("--- MASTER SIDE -----------------------");
        //                Printer.Foreach($"Side '{si.Name}':", si.SubSides, s => printSide(s));
        //                if (si.IsMaster) Printer.WriteLine("---------------------------------------");
        //            });
        //            printSide(side);
        //        });
        //        // Search for comparators for any side with master side
        //        Action<ISideRunner> searchComparator = null;
        //        searchComparator = new Action<ISideRunner>(side =>
        //        {
        //            //Printer.WriteLine($"Search comparator for side '{side.Name}'");
        //            var cc = Comparators.Where(c =>
        //            {
        //                var mts = MasterSide.GetAllItemsType();
        //                var st = side.GetItemType();
        //                var ct = c.GetItemTypes();
        //                return (mts.Contains(ct.Item1) && ct.Item2 == st) || (mts.Contains(ct.Item2) && ct.Item1 == st);
        //            }).Cast<IComparatorRunner>();
        //            if (cc.Count() != 1) throw new ArgumentException($"One, and only one '{nameof(ISideDefinition)}' must be added for master side '{MasterSide.Name}' and each side");
        //            side.Comparator = cc.Single();
        //            Printer.WriteLine($"Comparator for side '{side.Name}' is '{side.Comparator.GetItemTypes().Item1.Name}' <> '{side.Comparator.GetItemTypes().Item2.Name}'");
        //            foreach (var subSide in side.SubSides)
        //            {
        //                InternalSides.Where(s => s.GetSourceType() == subSide.GetItemType());
        //                searchComparator(subSide);
        //            }
        //        });
        //        // Iterate non master sides to search for a comparator for they
        //        Printer.Foreach("Comparators: ", rootSides.Where(s => !s.IsMaster), side => searchComparator(side));
        //        await Printer.IndentAsync("Loading in parallel ...", () =>
        //        {
        //            // Load all sides in parallel
        //            return Task.WhenAll(rootSides.Select(s => s.Load()));
        //        });
        //        Printer.WriteLine("Comparing each side with master side ...");
        //        // Compare each side with master side
        //        foreach (var sid in rootSides.Where(s => !s.IsMaster))
        //        {
        //            if (sid.Comparator.GetItemTypes().Item1 == MasterSide.GetItemType())
        //            {
        //                // Master is A in this comparer
        //                sid.Results = sid.Comparator.CompareItems(MasterSide, sid);
        //            }
        //            else
        //            {
        //                // Master is B in this comparer
        //                sid.Results = sid.Comparator.CompareItems(sid, MasterSide, true);
        //            }
        //        }
        //        Printer.WriteLine("Analyzing results ...");
        //        Func<ICollection<ISideRunner>, ICollection<IItem>> analyzeResults = null;
        //        analyzeResults = new Func<ICollection<ISideRunner>, ICollection<IItem>>(sides =>
        //        {
        //            var res = new List<IItem>();
        //            // Group side results by item key and populate with sides results
        //            foreach (var gro in sides
        //                .SelectMany(side => side.Results.Select(result => new
        //                {
        //                    Key = result.Key,
        //                    MasterItemType = side.SearchMasterSubSide(MasterSide).GetItemType(),
        //                    MasterItem = result.MasterItem,
        //                    MasterName = side.SearchMasterSubSide(MasterSide).GetItemName(result.MasterItem),
        //                    //SideId = side.Id,
        //                    Side = side,
        //                    SideItemType = side.GetItemType(),
        //                    SideItem = result.SideItem,
        //                    SideItemName = side.GetItemName(result.SideItem),
        //                    SideName = side.Name,
        //                    SideSubSides = result.SubSides,
        //                    Properties = result.Properties.ToArray()
        //                }))
        //                .GroupBy(r => r.Key))
        //            {
        //                // Create item
        //                var fir = gro.First(); // Use first element to get master info, all items in this group has the same master item
        //                var itemType = typeof(Item<>).MakeGenericType(fir.MasterItemType);
        //                var item = (IItem)Activator.CreateInstance(itemType, fir.MasterItem, fir.MasterName);
        //                foreach (var i in gro)
        //                {
        //                    // Create side
        //                    var sideItemType = typeof(ItemSide<,>).MakeGenericType(i.SideItemType, i.Key.GetType());
        //                    var sideItem = (IItemSide)Activator.CreateInstance(sideItemType, i.Side, i.SideName, i.Key, i.SideItem, i.SideItemName);
        //                    foreach (var pro in i.Properties)
        //                        ((IList<IProperty>)sideItem.Properties).Add(pro);
        //                    sideItem.SubItems = analyzeResults(i.SideSubSides);
        //                    // Add side to item
        //                    ((IList<IItemSide>)item.Sides).Add(sideItem);
        //                }
        //                // Add item to work
        //                res.Add(item);
        //            }
        //            return res;
        //        });
        //        foreach (var item in analyzeResults(rootSides.Where(side => !side.IsMaster).ToList())) Items.Add(item);
        //        Printer.WriteLine("Creating preview result ...");
        //        // Create preview response
        //        var preWork = new WorkPreview(Id);
        //        var preItems = new List<ItemPreview>();
        //        foreach (var item in Items)
        //        {
        //            var preItem = new ItemPreview(item.Id);
        //            preItem.MasterItemExist = item.MasterItem != null;
        //            preItem.MasterItemName = item.MasterName;
        //            var preSides = new List<ItemSidePreview>();
        //            foreach (var side in item.Sides)
        //            {
        //                var preSide = new ItemSidePreview(side.Side.Id);
        //                preSide.Key = side.Key.ToString();
        //                preSide.SideItemExist = side.SideItem != null;
        //                preSide.SideItemName = side.SideItemName;
        //                preSide.SideName = side.Name;
        //                //var prePros = new List<SynchronizationPropertyPreview>();
        //                foreach (var pro in side.Properties)
        //                {
        //                    var prePro = new PropertyPreview();
        //                    prePro.MasterValue = pro.MasterValue?.ToString();
        //                    prePro.SideValue = pro.SideValue?.ToString();
        //                    prePro.PropertyName = pro.PropertyName;
        //                    preSide.Properties.Add(prePro);
        //                    //prePros.Add(prePro);
        //                }
        //                //preSide.Properties = prePros;
        //                Func<ICollection<IItem>, ICollection<ItemRelationPreview>> processSubItems = null;
        //                processSubItems = new Func<ICollection<IItem>, ICollection<ItemRelationPreview>>(items => {
        //                    var res = new List<ItemRelationPreview>();
        //                    foreach (var s in items)
        //                    {
        //                        var rel = new ItemRelationPreview(s.Id);
        //                        rel.MasterItemExist = s.MasterItem != null;
        //                        rel.MasterItemName = s.MasterName;
        //                        var subSide = s.Sides.Single();
        //                        rel.Key = subSide.Key.ToString();
        //                        rel.SideItemExist = subSide.SideItem != null;
        //                        rel.SideItemName = subSide.SideItemName;
        //                        rel.SideName = subSide.Name;
        //                        foreach (var pro in subSide.Properties)
        //                        {
        //                            var prePro = new PropertyPreview();
        //                            prePro.MasterValue = pro.MasterValue?.ToString();
        //                            prePro.SideValue = pro.SideValue?.ToString();
        //                            prePro.PropertyName = pro.PropertyName;
        //                            rel.Properties.Add(prePro);
        //                        }
        //                        rel.Relations = processSubItems(subSide.SubItems);
        //                        res.Add(rel);
        //                    }
        //                    return res;
        //                });
        //                preSide.Relations = processSubItems(side.SubItems);
        //                preSides.Add(preSide);
        //            }
        //            preItem.Sides = preSides;
        //            preItems.Add(preItem);
        //        }
        //        preWork.Items = preItems;
        //        Printer.WriteLine("Determining default actions ...");
        //        // Check result and suggest an action
        //        foreach (var item in preWork.Items)
        //        {
        //            foreach (var side in item.Sides)
        //            {
        //                if (!item.MasterItemExist)
        //                    side.Action = SynchronizationAction.Delete;
        //                else if (!side.SideItemExist)
        //                    side.Action = SynchronizationAction.Insert;
        //                else if (side.Properties.Count() > 0)
        //                    side.Action = SynchronizationAction.Update;
        //                Action<ICollection<ItemRelationPreview>> act = null;
        //                act = new Action<ICollection<ItemRelationPreview>>(relations =>
        //                {
        //                    foreach (var rel in relations)
        //                    {
        //                        if (!rel.MasterItemExist)
        //                            rel.Action = SynchronizationAction.Delete;
        //                        else if (!rel.SideItemExist)
        //                            rel.Action = SynchronizationAction.Insert;
        //                        else if (rel.Properties.Count() > 0)
        //                            rel.Action = SynchronizationAction.Update;
        //                        act(rel.Relations);
        //                    }
        //                });
        //                act(side.Relations);
        //            }
        //        }
        //        return preWork;
        //    });
        //}
        //internal Task RunAsync(WorkPreview preview, Action action)
        //{
        //    return Printer.IndentAsync($"Work '{Name}'", async () =>
        //    {
        //        preview.Items.SelectMany(item => item.Sides.Select(s => s.Relations));

        //        foreach (var item in preview.Items)
        //        {
        //            var runItem = Items.Single(i => i.Id == item.Id);
        //            await Printer.ForeachAsync($"Item '{item.MasterItemName ?? item.Sides.First(s => s.SideItemName != null).SideItemName}'", item.Sides, 
        //                async side =>
        //            {
        //                Printer.Write($"Side '{side.SideName}' ");
        //                var runSide = InternalSides.Single(e => e.Id == side.Id);
        //                var runItemSide = runItem.Sides.Single(s => s.Side.Id == side.Id);
        //                var map = new Func<IItem, IItemSide, object>((i, s) =>
        //                {
        //                    if (runSide.Comparator.GetItemTypes().Item1 == MasterSide.GetItemType())
        //                    {
        //                        // Master is A in this comparator
        //                        if (item.MasterItemExist)
        //                            return runSide.Comparator.MapAToB(i.MasterItem, s.SideItem);
        //                    }
        //                    else
        //                    {
        //                        // Master is B in this comparator
        //                        if (item.MasterItemExist)
        //                            return runSide.Comparator.MapBToA(i.MasterItem, s.SideItem);
        //                    }
        //                    return null;
        //                });

        //                switch (side.Action)
        //                {
        //                    case SynchronizationAction.Insert:
        //                        await runSide.InsertAsync(map(runItem, runItemSide));
        //                        Printer.WriteLine("Inserted");
        //                        break;
        //                    case SynchronizationAction.Delete:
        //                        await runSide.DeleteAsync(runItemSide.SideItem);
        //                        Printer.WriteLine("Deleted");
        //                        break;
        //                    case SynchronizationAction.Update:
        //                        await runSide.UpdateAsync(map(runItem, runItemSide));
        //                        Printer.Foreach("Updated:", side.Properties, pro => {
        //                            Printer.WriteLine($"Changed '{pro.PropertyName}' property from '{pro.SideValue}' to '{pro.MasterValue}'");
        //                        });
        //                        break;
        //                    case SynchronizationAction.None:
        //                        Printer.WriteLine("None");
        //                        break;
        //                }
        //            });
        //        }
        //    });
        //}
    }
}