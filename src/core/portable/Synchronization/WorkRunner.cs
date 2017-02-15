using Fuxion.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    public class WorkRunner
    {
        public WorkRunner(WorkDefinition definition)
        {
            this.definition = definition;
            Sides = definition.Sides.Select(d => d.CreateRunner()).ToList();
            Comparators = definition.Comparators.Select(c => c.CreateRunner()).ToList();
        }
        WorkDefinition definition;
        internal Guid Id { get; } = Guid.NewGuid();
        internal ICollection<ISideRunner> Sides { get; set; }
        internal ICollection<IItemRunner> Items { get; set; } = new List<IItemRunner>();
        internal ICollection<IComparatorRunner> Comparators { get; set; }
        internal ISideRunner MasterSide { get; set; }
        internal Task<WorkPreview> PreviewAsync()
        {
            return Printer.IndentAsync($"Work '{definition.Name}'", async () =>
            {
                // Get master side
                var masters = Sides.Where(s => s.Definition.IsMaster);
                if (masters.Count() != 1) throw new ArgumentException($"One, and only one '{nameof(ISideDefinition)}' must be the master side");
                MasterSide = masters.Single();
                // Determine what sides are sub-sides of others
                Action<ISideRunner> populateSubSides = null;
                populateSubSides = new Action<ISideRunner>(side =>
                {
                    side.SubSides = Sides.Where(s => s.GetSourceType() == side.GetItemType()).ToList();
                    foreach (var s in side.SubSides)
                    {
                        if (side.Definition.IsMaster) s.Definition.IsMaster = true;
                        populateSubSides(s);
                    }
                });
                var rootSides = Sides.Where(s => !Sides.Any(s2 => s2.GetItemType() == s.GetSourceType())).OrderByDescending(s => s.Definition.IsMaster).ThenBy(s => s.Definition.Name).ToList();
                foreach (var side in rootSides)
                    populateSubSides(side);
                Printer.Foreach("Sides tree:", rootSides, side =>
                {
                    Action<ISideRunner> printSide = null;
                    printSide = new Action<ISideRunner>(si =>
                    {
                        //if (si.Definition.IsMaster) Printer.WriteLine("--- MASTER SIDE -----------------------");
                        Printer.Foreach($"Side '{si.Definition.Name}' {(si.Definition.IsMaster ? "(MASTER)" : "")}:", si.SubSides, s => printSide(s));
                        //if (si.Definition.IsMaster) Printer.WriteLine("---------------------------------------");
                    });
                    printSide(side);
                });
                // Search for comparators for any side with master side
                Action<ISideRunner> searchComparator = null;
                searchComparator = new Action<ISideRunner>(side =>
                {
                    //Printer.WriteLine($"Search comparator for side '{side.Name}'");
                    var cc = Comparators.Where(c =>
                    {
                        var mts = MasterSide.GetAllItemsType();
                        var st = side.GetItemType();
                        var ct = c.GetItemTypes();
                        return (mts.Contains(ct.Item1) && ct.Item2 == st) || (mts.Contains(ct.Item2) && ct.Item1 == st);
                    }).Cast<IComparatorRunner>();
                    if (cc.Count() != 1) throw new ArgumentException($"One, and only one '{nameof(ISideDefinition)}' must be added for master side '{MasterSide.Definition.Name}' and each side");
                    side.Comparator = cc.Single();
                    Printer.WriteLine($"Comparator for side '{side.Definition.Name}' is '{side.Comparator.GetItemTypes().Item1.Name}' <> '{side.Comparator.GetItemTypes().Item2.Name}'");
                    foreach (var subSide in side.SubSides)
                    {
                        Sides.Where(s => s.GetSourceType() == subSide.GetItemType());
                        searchComparator(subSide);
                    }
                });
                // Iterate non master sides to search for a comparator for they
                Printer.Foreach("Comparators: ", rootSides.Where(s => !s.Definition.IsMaster), side => searchComparator(side));
                await Printer.IndentAsync("Loading sides "+
#if DEBUG
                    "sequentially"
#else
                    "in parallel"
#endif
                    + " ...", () =>
                {
#if DEBUG
                    // Load all sides sequentially
                    return TaskManager.StartNew(async () =>
                    {
                        foreach (var side in rootSides)
                        {
                            await side.Load();
                        }
                    });
#else
                    Printer.WriteLine("|| Loading in parallel");
                    // Load all sides in parallel
                    return Task.WhenAll(rootSides.Select(s => s.Load()));
#endif
                });
                Printer.Foreach("Comparing each side with master side ...", rootSides.Where(s => !s.Definition.IsMaster), sid=> { 
                    if (sid.Comparator.GetItemTypes().Item1 == MasterSide.GetItemType())
                    {
                        // Master is A in this comparer
                        sid.Results = sid.Comparator.CompareSides(MasterSide, sid);
                    }
                    else
                    {
                        // Master is B in this comparer
                        sid.Results = sid.Comparator.CompareSides(sid, MasterSide, true);
                    }
                });
                Printer.WriteLine("Analyzing results ...");
                Func<ICollection<ISideRunner>, ICollection<IItemRunner>> analyzeResults = null;
                analyzeResults = new Func<ICollection<ISideRunner>, ICollection<IItemRunner>>(sides =>
                {
                    var res = new List<IItemRunner>();
                    // Group side results by item key and populate with sides results
                    foreach (var gro in sides
                        .SelectMany(side => side.Results.Select(result => new
                        {
                            Key = result.Key,
                            MasterItemType = side.SearchMasterSubSide(MasterSide).GetItemType(),
                            MasterItem = result.MasterItem,
                            MasterName = side.SearchMasterSubSide(MasterSide).GetItemName(result.MasterItem),
                            //SideId = side.Id,
                            Side = side,
                            SideItemType = side.GetItemType(),
                            SideItem = result.SideItem,
                            SideItemName = side.GetItemName(result.SideItem),
                            SideName = side.Definition.Name,
                            SideSubSides = result.SubSides,
                            Properties = result.Properties.ToArray()
                        }))
                        .GroupBy(r => r.Key))
                    {
                        // Create item
                        var fir = gro.First(); // Use first element to get master info, all items in this group has the same master item
                        var itemType = typeof(ItemRunner<>).MakeGenericType(fir.MasterItemType);
                        var item = (IItemRunner)Activator.CreateInstance(itemType, fir.MasterItem, fir.MasterName);
                        foreach (var i in gro)
                        {
                            // Create side
                            var sideItemType = typeof(ItemSideRunner<,>).MakeGenericType(i.SideItemType, i.Key.GetType());
                            var sideItem = (IItemSideRunner)Activator.CreateInstance(sideItemType, i.Side, i.SideName, i.Key, i.SideItem, i.SideItemName);
                            foreach (var pro in i.Properties)
                                ((IList<IPropertyRunner>)sideItem.Properties).Add(pro);
                            sideItem.SubItems = analyzeResults(i.SideSubSides);
                            // Add side to item
                            ((IList<IItemSideRunner>)item.Sides).Add(sideItem);
                        }
                        // Add item to work
                        res.Add(item);
                    }
                    return res;
                });
                foreach (var item in analyzeResults(rootSides.Where(side => !side.Definition.IsMaster).ToList())) Items.Add(item);
                Printer.WriteLine("Creating preview result ...");
                // Create preview response
                var preWork = new WorkPreview(Id);
                var preItems = new List<ItemPreview>();
                foreach (var item in Items)
                {
                    var preItem = new ItemPreview(item.Id);
                    preItem.MasterItemExist = item.MasterItem != null;
                    preItem.MasterItemName = item.MasterName;
                    var preSides = new List<ItemSidePreview>();
                    foreach (var side in item.Sides)
                    {
                        var preSide = new ItemSidePreview(side.Side.Id);
                        preSide.Key = side.Key.ToString();
                        preSide.SideItemExist = side.SideItem != null;
                        preSide.SideItemName = side.SideItemName;
                        preSide.SideName = side.Name;
                        //var prePros = new List<SynchronizationPropertyPreview>();
                        foreach (var pro in side.Properties)
                        {
                            var prePro = new PropertyPreview();
                            prePro.MasterValue = pro.MasterValue?.ToString();
                            prePro.SideValue = pro.SideValue?.ToString();
                            prePro.PropertyName = pro.PropertyName;
                            preSide.Properties.Add(prePro);
                            //prePros.Add(prePro);
                        }
                        //preSide.Properties = prePros;
                        Func<ICollection<IItemRunner>, ICollection<ItemRelationPreview>> processSubItems = null;
                        processSubItems = new Func<ICollection<IItemRunner>, ICollection<ItemRelationPreview>>(items => {
                            var res = new List<ItemRelationPreview>();
                            foreach (var i in items)
                            {
                                var rel = new ItemRelationPreview(i.Id);
                                rel.MasterItemExist = i.MasterItem != null;
                                rel.MasterItemName = i.MasterName;
                                var subSide = i.Sides.Single();
                                rel.Key = subSide.Key.ToString();
                                rel.SideItemExist = subSide.SideItem != null;
                                rel.SideItemName = subSide.SideItemName;
                                rel.SideName = subSide.Name;
                                foreach (var pro in subSide.Properties)
                                {
                                    var prePro = new PropertyPreview();
                                    prePro.MasterValue = pro.MasterValue?.ToString();
                                    prePro.SideValue = pro.SideValue?.ToString();
                                    prePro.PropertyName = pro.PropertyName;
                                    rel.Properties.Add(prePro);
                                }
                                rel.Relations = processSubItems(subSide.SubItems);
                                res.Add(rel);
                            }
                            return res;
                        });
                        preSide.Relations = processSubItems(side.SubItems);
                        preSides.Add(preSide);
                    }
                    preItem.Sides = preSides;
                    preItems.Add(preItem);
                }
                preWork.Items = preItems;
                Printer.WriteLine("Determining default actions ...");
                // Check result and suggest an action
                foreach (var item in preWork.Items)
                {
                    foreach (var side in item.Sides)
                    {
                        if (!item.MasterItemExist)
                            side.Action = SynchronizationAction.Delete;
                        else if (!side.SideItemExist)
                            side.Action = SynchronizationAction.Insert;
                        else if (side.Properties.Count() > 0)
                            side.Action = SynchronizationAction.Update;
                        Action<ICollection<ItemRelationPreview>> act = null;
                        act = new Action<ICollection<ItemRelationPreview>>(relations =>
                        {
                            foreach (var rel in relations)
                            {
                                if (!rel.MasterItemExist)
                                    rel.Action = SynchronizationAction.Delete;
                                else if (!rel.SideItemExist)
                                    rel.Action = SynchronizationAction.Insert;
                                else if (rel.Properties.Count() > 0)
                                    rel.Action = SynchronizationAction.Update;
                                act(rel.Relations);
                            }
                        });
                        act(side.Relations);
                    }
                }
                return preWork;
            });
        }
    }
}
