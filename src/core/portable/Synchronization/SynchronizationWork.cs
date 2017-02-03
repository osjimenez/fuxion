using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Fuxion.Synchronization
{
    public class SynchronizationWork
    {
        public string Name { get; set; }
        public IEnumerable<ISynchronizationSide> Sides { get { return InternalSides; } set { InternalSides = value.Cast<ISynchronizationSideInternal>(); } }
        public IEnumerable<ISynchronizationComparator> Comparators { get; set; }
        //public IEnumerable<SyncWork> SubWorks { get; set; }

        internal Guid Id { get; } = Guid.NewGuid();
        internal IEnumerable<ISynchronizationSideInternal> InternalSides { get; set; }
        internal IList<ISynchronizationItem> Items { get; set; } = new List<ISynchronizationItem>();
        ISynchronizationSide MasterSide { get { return InternalMasterSide; } set { InternalMasterSide = (ISynchronizationSideInternal)MasterSide; } }
        ISynchronizationSideInternal InternalMasterSide { get; set; }
        internal Task<SynchronizationWorkPreview> PreviewAsync()
        {
            return Printer.IndentAsync($"Work '{Name}'", async () =>
            {
                Printer.WriteLine("Determining master side ...");
                // Get master side
                var masters = InternalSides.Where(s => s.IsMaster);
                if (masters.Count() != 1) throw new ArgumentException("One, and only one SyncSide must be the master side");
                InternalMasterSide = masters.Single();

                // Search for comparators for any side with master side
                foreach (var side in InternalSides.Where(s => !s.IsMaster))
                {
                    var cc = Comparators.Where(c =>
                    {
                        var mt = InternalMasterSide.GetItemType();
                        var st = side.GetItemType();
                        var ct = c.GetItemTypes();
                        return (ct.Item1 == mt && ct.Item2 == st) || (ct.Item2 == mt && ct.Item1 == st);
                    });
                    if (cc.Count() != 1) throw new ArgumentException("One, and only one ISyncComparator must be added for master side and each side");
                    side.Comparator = cc.Single();
                }
                Printer.WriteLine("Loading in parallel ...");
                // Load all sides in parallel
                await Task.WhenAll(InternalSides.Select(s => s.Load()));
                Printer.WriteLine("Comparing each side with master side ...");
                // Compare each side with master side
                foreach (var sid in InternalSides.Where(s => !s.IsMaster))
                {
                    if (sid.Comparator.GetItemTypes().Item1 == InternalMasterSide.GetItemType())
                    {
                        // Master is A in this comparer
                        sid.Results = ((ISynchronizationComparatorInternal)sid.Comparator).CompareItems(InternalMasterSide.Entries, sid.Entries);
                    }
                    else
                    {
                        // Master is B in this comparer
                        sid.Results = ((ISynchronizationComparatorInternal)sid.Comparator).CompareItems(sid.Entries, InternalMasterSide.Entries).Select(p => p.Invert());
                    }
                }
                Printer.WriteLine("Analyzing results ...");
                // Group side results by item key and populate with sides results
                foreach (var gro in InternalSides
                    .Where(e => !e.IsMaster)
                    .SelectMany(e => e.Results.Select(r => new
                    {
                        Key = r.Key,
                        MasterItemType = InternalMasterSide.GetItemType(),
                        MasterItem = r.MasterItem,
                        MasterName = InternalMasterSide.GetItemName(r.MasterItem),
                        SideSyncId = e.Id,
                        SideItemType = e.GetItemType(),
                        SideItem = r.SideItem,
                        SideItemName = e.GetItemName(r.SideItem),
                        SideName = e.Name,
                        Properties = r.Properties.ToArray()
                    }))
                    .GroupBy(r => r.MasterItem))
                {
                    // Create item preview
                    var fir = gro.First(); // Use first element to get master info, all items in this group has the same master item
                    var itemPreviewType = typeof(SynchronizationItem<>).MakeGenericType(fir.MasterItemType);
                    var itemPreview = (ISynchronizationItem)Activator.CreateInstance(itemPreviewType, fir.MasterItem, fir.MasterName);
                    var sides = new List<ISynchronizationItemSide>();
                    foreach (var i in gro)
                    {
                        // Create side preview
                        var sidePreviewType = typeof(SynchronizationItemSide<,>).MakeGenericType(i.SideItemType, i.Key.GetType());
                        var sidePreview = (ISynchronizationItemSide)Activator.CreateInstance(sidePreviewType, i.SideSyncId, i.SideName, i.Key, i.SideItem, i.SideItemName);
                        foreach (var pro in i.Properties)
                            ((IList<ISynchronizationProperty>)sidePreview.Properties).Add(pro);
                        // Add side to item
                        ((IList<ISynchronizationItemSide>)itemPreview.Sides).Add(sidePreview);
                    }
                    // Add item to work
                    Items.Add(itemPreview);
                }
                Printer.WriteLine("Creating preview result ...");
                // Create preview response
                var preWork = new SynchronizationWorkPreview(Id);
                var preItems = new List<SynchronizationItemPreview>();
                foreach (var item in Items)
                {
                    var preItem = new SynchronizationItemPreview(item.SyncId);
                    preItem.MasterItemExist = item.MasterItem != null;
                    preItem.MasterItemName = item.MasterName;
                    var preSides = new List<SynchronizationItemSidePreview>();
                    foreach (var side in item.Sides)
                    {
                        var preSide = new SynchronizationItemSidePreview(side.SyncId);
                        preSide.Key = side.Key.ToString();
                        preSide.SideItemExist = side.SideItem != null;
                        preSide.SideItemName = side.SideItemName;
                        preSide.SideName = side.Name;
                        var prePros = new List<SynchronizationPropertyPreview>();
                        foreach (var pro in side.Properties)
                        {
                            var prePro = new SynchronizationPropertyPreview();
                            prePro.MasterValue = pro.MasterValue.ToString();
                            prePro.SideValue = pro.SideValue.ToString();
                            prePro.PropertyName = pro.PropertyName;
                            prePros.Add(prePro);
                        }
                        preSide.Properties = prePros;
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
                    }
                }

                return preWork;
            });
        }
        internal Task RunAsync(SynchronizationWorkPreview preview)
        {
            return Printer.IndentAsync($"Work '{Name}'", async () =>
            {
                foreach (var item in preview.Items)
                {
                    var runItem = Items.Single(i => i.SyncId == item.Id);
                    foreach (var side in item.Sides)
                    {
                        var runSide = InternalSides.Single(e => e.Id == side.Id);
                        var runItemSide = runItem.Sides.Single(s => s.SyncId == side.Id);
                        var map = new Func<ISynchronizationItem, ISynchronizationItemSide, object>((i, s) =>
                        {
                            if (runSide.Comparator.GetItemTypes().Item1 == InternalMasterSide.GetItemType())
                            {
                                // Master is A in this comparator
                                if (item.MasterItemExist)
                                    return ((ISynchronizationComparatorInternal)runSide.Comparator).MapAToB(i.MasterItem, s.SideItem);
                            }
                            else
                            {
                                // Master is B in this comparator
                                if (item.MasterItemExist)
                                    return ((ISynchronizationComparatorInternal)runSide.Comparator).MapBToA(i.MasterItem, s.SideItem);
                            }
                            return null;
                        });

                        switch (side.Action)
                        {
                            case SynchronizationAction.Insert:
                                await runSide.InsertAsync(map(runItem, runItemSide));
                                break;
                            case SynchronizationAction.Delete:
                                await runSide.DeleteAsync(runItemSide.SideItem);
                                break;
                            case SynchronizationAction.Update:
                                await runSide.UpdateAsync(map(runItem, runItemSide));
                                break;
                        }
                    }
                }
            });
        }
    }
}