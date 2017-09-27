using Fuxion.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    internal class WorkRunner
    {
        public WorkRunner(Work definition, IPrinter printer)
        {
            Definition = definition;
            Sides = definition.Sides.Select(d => d.CreateRunner(printer)).ToList();
            Comparators = definition.Comparators.Select(c => c.CreateRunner()).ToList();
        }
        internal Work Definition { get; }
        internal ICollection<ISideRunner> Sides { get; set; }
        internal ICollection<IItemRunner> Items { get; set; } = new List<IItemRunner>();
        internal ICollection<IComparatorRunner> Comparators { get; set; }
        internal ISideRunner MasterSide { get; set; }
        internal async Task<WorkPreview> PreviewAsync(IPrinter printer) 
        {
            void PrintSide(ISideRunner si)
            {
                printer.Foreach($"Side '{si.Definition.Name}' {(si.Definition.IsMaster ? "(MASTER)" : "")}:", si.SubSides, s => PrintSide(s));
            }
            using (printer.Indent($"Work '{Definition.Name}'"))
            {
                // Get master side
                var masters = Sides.Where(s => s.Definition.IsMaster);
                if (masters.Count() != 1) throw new ArgumentException($"One, and only one '{nameof(ISide)}' must be the master side");
                MasterSide = masters.Single();

                // Determine what sides are sub-sides of others
                void PopulateSubSides(ISideRunner side)
                {
                    side.SubSides = Sides.Where(s => s.GetSourceType() == side.GetItemType()).ToList();
                    foreach (var s in side.SubSides)
                    {
                        if (side.Definition.IsMaster) s.Definition.IsMaster = true;
                        PopulateSubSides(s);
                    }
                }
                var rootSides = Sides.Where(s => !Sides.Any(s2 => s2.GetItemType() == s.GetSourceType())).OrderByDescending(s => s.Definition.IsMaster).ThenBy(s => s.Definition.Name).ToList();
                foreach (var side in rootSides)
                    PopulateSubSides(side);
                printer.Foreach("Sides tree:", rootSides, side =>
                {
                    PrintSide(side);
                });

                // Search for comparators for any side with master side
                void SearchComparator(ISideRunner side)
                {
                    var cc = Comparators.Where(c =>
                    {
                        var mts = MasterSide.GetAllItemsType();
                        var st = side.GetItemType();
                        var ct = c.GetItemTypes();
                        return (mts.Contains(ct.Item1) && ct.Item2 == st) || (mts.Contains(ct.Item2) && ct.Item1 == st);
                    }).Cast<IComparatorRunner>();
                    if (cc.Count() != 1) throw new ArgumentException($"One, and only one '{nameof(ISide)}' must be added for master side '{MasterSide.Definition.Name}' and each side");
                    side.Comparator = cc.Single();
                    printer.WriteLine($"Comparator for side '{side.Definition.Name}' is '{side.Comparator.GetItemTypes().Item1.Name}' <> '{side.Comparator.GetItemTypes().Item2.Name}'");
                    foreach (var subSide in side.SubSides)
                    {
                        Sides.Where(s => s.GetSourceType() == subSide.GetItemType());
                        SearchComparator(subSide);
                    }
                }
                // Iterate non master sides to search for a comparator for they
                printer.Foreach("Comparators: ", rootSides.Where(s => !s.Definition.IsMaster), side => SearchComparator(side));

                // Load sides
                using (printer.Indent($"Loading sides {(Definition.LoadSidesInParallel ? "in parallel" : "sequentially")} ..."))
                {
                    if (Definition.LoadSidesInParallel)
                    {
                        await Task.WhenAll(rootSides.Select(s => s.Load()));
                    }
                    else
                    {
                        await TaskManager.StartNew(async () =>
                        {
                            foreach (var side in rootSides)
                            {
                                await side.Load();
                            }
                        });
                    }
                }

                // Comparing sides
                printer.Foreach("Comparing each side with master side ...", rootSides.Where(s => !s.Definition.IsMaster), sid => {
                    if (sid.Comparator.GetItemTypes().Item1 == MasterSide.GetItemType())
                    {
                        // Master is A in this comparer
                        sid.Results = sid.Comparator.CompareSides(MasterSide, sid, false, printer);
                    }
                    else
                    {
                        // Master is B in this comparer
                        sid.Results = sid.Comparator.CompareSides(sid, MasterSide, true, printer);
                    }
                });
                printer.WriteLine("Analyzing results ...");
                ICollection<IItemRunner> AnalyzeResults(ICollection<ISideRunner> sides)
                {
                    var res = new List<IItemRunner>();
                    // Group side results by item key and populate with sides results
                    foreach (var gro in sides
                        .SelectMany(side => side.Results.Select(result => new
                        {
                            Key = result.Key,
                            MasterRunner = side.SearchMasterSubSide(MasterSide),
                            MasterItemType = side.SearchMasterSubSide(MasterSide).GetItemType(),
                            MasterItem = result.MasterItem,
                            MasterItemName = side.SearchMasterSubSide(MasterSide).GetItemName(result.MasterItem),
                            MasterItemTag = side.SearchMasterSubSide(MasterSide).GetItemTag(result.MasterItem),
                            //SideId = side.Id,
                            Side = side,
                            SideItemType = side.GetItemType(),
                            SideItem = result.SideItem,
                            SideItemName = side.GetItemName(result.SideItem),
                            SideItemTag = side.GetItemTag(result.SideItem),
                            SideName = side.Definition.Name,
                            SideSubSides = result.SubSides,
                            Properties = result.Properties.ToArray()
                        }))
                        .GroupBy(r => r.Key))
                    {
                        // Create item
                        var fir = gro.First(); // Use first element to get master info, all items in this group has the same master item
                        var itemType = typeof(ItemRunner<>).MakeGenericType(fir.MasterItemType);
                        var item = (IItemRunner)Activator.CreateInstance(itemType, fir.MasterRunner, fir.MasterItem, fir.MasterItemName, fir.MasterItemTag);
                        foreach (var i in gro)
                        {
                            // Create side
                            var sideItemType = typeof(ItemSideRunner<,>).MakeGenericType(i.SideItemType, i.Key.GetType());
                            var sideItem = (IItemSideRunner)Activator.CreateInstance(sideItemType, i.Side, i.SideName, i.Key, i.SideItem, i.SideItemName, i.SideItemTag);
                            foreach (var pro in i.Properties)
                                ((IList<IPropertyRunner>)sideItem.Properties).Add(pro);
                            sideItem.SubItems = AnalyzeResults(i.SideSubSides);
                            // Add side to item
                            ((IList<IItemSideRunner>)item.SideRunners).Add(sideItem);
                        }
                        // Add item to work
                        res.Add(item);
                    }
                    return res;
                }
                foreach (var item in AnalyzeResults(rootSides.Where(side => !side.Definition.IsMaster).ToList())) Items.Add(item);

                // Create preview response
                printer.WriteLine("Creating preview result ...");
                var preWork = new WorkPreview(Definition.Id);
                preWork.Name = Definition.Name;
                preWork.MasterSideName = MasterSide.Definition.Name;
                var preItems = new List<ItemPreview>();
                foreach (var item in Items)
                {
                    var preItem = new ItemPreview(preWork, item.Id);
                    preItem.MasterItemExist = item.MasterItem != null;
                    preItem.MasterItemName = item.MasterItemName;
                    preItem.MasterItemTag = item.MasterItemTag;
                    preItem.SingularMasterTypeName = MasterSide.Definition.SingularItemTypeName;
                    preItem.PluralMasterTypeName = MasterSide.Definition.PluralItemTypeName;
                    preItem.MasterTypeIsMale = MasterSide.Definition.ItemTypeIsMale;

                    var preSides = new List<ItemSidePreview>();
                    foreach (var side in item.SideRunners)
                    {
                        var preSide = new ItemSidePreview(preItem, side.Side.Id);
                        preSide.Key = side.Key.ToString();
                        preSide.SideAllowInsert = side.Side.Definition.AllowInsert;
                        preSide.SideAllowDelete = side.Side.Definition.AllowDelete;
                        preSide.SideAllowUpdate = side.Side.Definition.AllowUpdate;
                        preSide.SideItemExist = side.SideItem != null;
                        preSide.SideItemName = side.SideItemName;
                        preSide.SideItemTag = side.SideItemTag;
                        preSide.SingularSideTypeName = side.Side.Definition.SingularItemTypeName;
                        preSide.PluralSideTypeName = side.Side.Definition.PluralItemTypeName;
                        preSide.ItemTypeIsMale = side.Side.Definition.ItemTypeIsMale;
                        preSide.SideName = side.Name;
                        foreach (var pro in side.Properties)
                        {
                            var prePro = new PropertyPreview(preSide);
                            prePro.MasterValue = pro.MasterNamingFunction(pro.MasterValue);
                            prePro.SideValue = pro.SideNamingFunction(pro.SideValue);
                            prePro.PropertyName = pro.PropertyName;
                            preSide.Properties.Add(prePro);
                        }
                        ICollection<ItemRelationPreview> ProcessSubItems(object parent, ICollection<IItemRunner> items)
                        {
                            var res = new List<ItemRelationPreview>();
                            foreach (var i in items)
                            {
                                ItemRelationPreview rel;
                                if (parent is ItemSidePreview)
                                    rel = new ItemRelationPreview(parent as ItemSidePreview, i.Id);
                                else
                                    rel = new ItemRelationPreview(parent as ItemRelationPreview, i.Id);
                                rel.SideAllowInsert = i.SideRunners.Single().Side.Definition.AllowInsert;
                                rel.SideAllowDelete = i.SideRunners.Single().Side.Definition.AllowDelete;
                                rel.SideAllowUpdate = i.SideRunners.Single().Side.Definition.AllowUpdate;
                                rel.MasterItemExist = i.MasterItem != null;
                                rel.MasterItemName = i.MasterItemName;
                                rel.MasterItemTag = i.MasterItemTag;
                                rel.SingularMasterTypeName = i.MasterRunner.Definition.SingularItemTypeName;
                                rel.PluralMasterTypeName = i.MasterRunner.Definition.PluralItemTypeName;
                                rel.SingularSideTypeName = i.SideRunners.Single().Side.Definition.SingularItemTypeName;
                                rel.PluralSideTypeName = i.SideRunners.Single().Side.Definition.PluralItemTypeName;
                                var subSide = i.SideRunners.Single();
                                rel.Key = subSide.Key.ToString();
                                rel.SideItemExist = subSide.SideItem != null;
                                rel.SideItemName = subSide.SideItemName;
                                rel.SideItemTag = subSide.SideItemTag;
                                rel.SideName = subSide.Name;
                                foreach (var pro in subSide.Properties)
                                {
                                    var prePro = new PropertyPreview(rel);
                                    prePro.MasterValue = pro.MasterNamingFunction(pro.MasterValue);
                                    prePro.SideValue = pro.SideNamingFunction(pro.SideValue);
                                    prePro.PropertyName = pro.PropertyName;
                                    rel.Properties.Add(prePro);
                                }
                                rel.Relations = ProcessSubItems(rel, subSide.SubItems);
                                res.Add(rel);
                            }
                            return res;
                        }
                        preSide.Relations = ProcessSubItems(preSide, side.SubItems);
                        preSides.Add(preSide);
                    }
                    preItem.Sides = preSides;
                    preItems.Add(preItem);
                }
                preWork.Items = preItems;

                // Check result and suggest an action
                printer.WriteLine("Determining default actions ...");
                foreach (var item in preWork.Items)
                {
                    foreach (var side in item.Sides)
                    {
                        if (!item.MasterItemExist && side.SideAllowDelete)
                            side.Action = SynchronizationAction.Delete;
                        else if (!side.SideItemExist && side.SideAllowInsert)
                            side.Action = SynchronizationAction.Insert;
                        else if (side.Properties.Count() > 0 && side.SideAllowUpdate)
                            side.Action = SynchronizationAction.Update;
                        void ProcessRelations(ICollection<ItemRelationPreview> relations)
                        {
                            foreach (var rel in relations)
                            {
                                if (!rel.MasterItemExist && rel.SideAllowDelete)
                                    rel.Action = SynchronizationAction.Delete;
                                else if (!rel.SideItemExist && rel.SideAllowInsert)
                                    rel.Action = SynchronizationAction.Insert;
                                else if (rel.Properties.Count() > 0 && rel.SideAllowUpdate)
                                    rel.Action = SynchronizationAction.Update;
                                ProcessRelations(rel.Relations);
                            }
                        }
                        ProcessRelations(side.Relations);
                    }
                }
                return preWork;
            }
            //return printer.IndentAsync($"Work '{Definition.Name}'", async () =>
            //{
            //    // Get master side
            //    var masters = Sides.Where(s => s.Definition.IsMaster);
            //    if (masters.Count() != 1) throw new ArgumentException($"One, and only one '{nameof(ISide)}' must be the master side");
            //    MasterSide = masters.Single();

            //    // Determine what sides are sub-sides of others
            //    void PopulateSubSides(ISideRunner side)
            //    {
            //        side.SubSides = Sides.Where(s => s.GetSourceType() == side.GetItemType()).ToList();
            //        foreach (var s in side.SubSides)
            //        {
            //            if (side.Definition.IsMaster) s.Definition.IsMaster = true;
            //            PopulateSubSides(s);
            //        }
            //    }
            //    var rootSides = Sides.Where(s => !Sides.Any(s2 => s2.GetItemType() == s.GetSourceType())).OrderByDescending(s => s.Definition.IsMaster).ThenBy(s => s.Definition.Name).ToList();
            //    foreach (var side in rootSides)
            //        PopulateSubSides(side);
            //    printer.Foreach("Sides tree:", rootSides, side =>
            //    {
            //        PrintSide(side);
            //    });

            //    // Search for comparators for any side with master side
            //    void SearchComparator(ISideRunner side)
            //    {
            //        var cc = Comparators.Where(c =>
            //        {
            //            var mts = MasterSide.GetAllItemsType();
            //            var st = side.GetItemType();
            //            var ct = c.GetItemTypes();
            //            return (mts.Contains(ct.Item1) && ct.Item2 == st) || (mts.Contains(ct.Item2) && ct.Item1 == st);
            //        }).Cast<IComparatorRunner>();
            //        if (cc.Count() != 1) throw new ArgumentException($"One, and only one '{nameof(ISide)}' must be added for master side '{MasterSide.Definition.Name}' and each side");
            //        side.Comparator = cc.Single();
            //        printer.WriteLine($"Comparator for side '{side.Definition.Name}' is '{side.Comparator.GetItemTypes().Item1.Name}' <> '{side.Comparator.GetItemTypes().Item2.Name}'");
            //        foreach (var subSide in side.SubSides)
            //        {
            //            Sides.Where(s => s.GetSourceType() == subSide.GetItemType());
            //            SearchComparator(subSide);
            //        }
            //    }
            //    // Iterate non master sides to search for a comparator for they
            //    printer.Foreach("Comparators: ", rootSides.Where(s => !s.Definition.IsMaster), side => SearchComparator(side));

            //    // Load sides
            //    await printer.IndentAsync($"Loading sides {(Definition.LoadSidesInParallel ? "in parallel" : "sequentially")} ...", () =>
            //    {
            //        if (Definition.LoadSidesInParallel)
            //        {
            //            return Task.WhenAll(rootSides.Select(s => s.Load()));
            //        }else
            //        {
            //            return TaskManager.StartNew(async () =>
            //            {
            //                foreach (var side in rootSides)
            //                {
            //                    await side.Load();
            //                }
            //            });
            //        }
            //    });

            //    // Comparing sides
            //    printer.Foreach("Comparing each side with master side ...", rootSides.Where(s => !s.Definition.IsMaster), sid=> { 
            //        if (sid.Comparator.GetItemTypes().Item1 == MasterSide.GetItemType())
            //        {
            //            // Master is A in this comparer
            //            sid.Results = sid.Comparator.CompareSides(MasterSide, sid, false, printer);
            //        }
            //        else
            //        {
            //            // Master is B in this comparer
            //            sid.Results = sid.Comparator.CompareSides(sid, MasterSide, true, printer);
            //        }
            //    });
            //    printer.WriteLine("Analyzing results ...");
            //    ICollection<IItemRunner> AnalyzeResults(ICollection<ISideRunner> sides)
            //    {
            //        var res = new List<IItemRunner>();
            //        // Group side results by item key and populate with sides results
            //        foreach (var gro in sides
            //            .SelectMany(side => side.Results.Select(result => new
            //            {
            //                Key = result.Key,
            //                MasterRunner = side.SearchMasterSubSide(MasterSide),
            //                MasterItemType = side.SearchMasterSubSide(MasterSide).GetItemType(),
            //                MasterItem = result.MasterItem,
            //                MasterItemName = side.SearchMasterSubSide(MasterSide).GetItemName(result.MasterItem),
            //                MasterItemTag = side.SearchMasterSubSide(MasterSide).GetItemTag(result.MasterItem),
            //                //SideId = side.Id,
            //                Side = side,
            //                SideItemType = side.GetItemType(),
            //                SideItem = result.SideItem,
            //                SideItemName = side.GetItemName(result.SideItem),
            //                SideItemTag = side.GetItemTag(result.SideItem),
            //                SideName = side.Definition.Name,
            //                SideSubSides = result.SubSides,
            //                Properties = result.Properties.ToArray()
            //            }))
            //            .GroupBy(r => r.Key))
            //        {
            //            // Create item
            //            var fir = gro.First(); // Use first element to get master info, all items in this group has the same master item
            //            var itemType = typeof(ItemRunner<>).MakeGenericType(fir.MasterItemType);
            //            var item = (IItemRunner)Activator.CreateInstance(itemType, fir.MasterRunner, fir.MasterItem, fir.MasterItemName, fir.MasterItemTag);
            //            foreach (var i in gro)
            //            {
            //                // Create side
            //                var sideItemType = typeof(ItemSideRunner<,>).MakeGenericType(i.SideItemType, i.Key.GetType());
            //                var sideItem = (IItemSideRunner)Activator.CreateInstance(sideItemType, i.Side, i.SideName, i.Key, i.SideItem, i.SideItemName, i.SideItemTag);
            //                foreach (var pro in i.Properties)
            //                    ((IList<IPropertyRunner>)sideItem.Properties).Add(pro);
            //                sideItem.SubItems = AnalyzeResults(i.SideSubSides);
            //                // Add side to item
            //                ((IList<IItemSideRunner>)item.SideRunners).Add(sideItem);
            //            }
            //            // Add item to work
            //            res.Add(item);
            //        }
            //        return res;
            //    }
            //    foreach (var item in AnalyzeResults(rootSides.Where(side => !side.Definition.IsMaster).ToList())) Items.Add(item);

            //    // Create preview response
            //    printer.WriteLine("Creating preview result ...");
            //    var preWork = new WorkPreview(Definition.Id);
            //    preWork.Name = Definition.Name;
            //    preWork.MasterSideName = MasterSide.Definition.Name;
            //    var preItems = new List<ItemPreview>();
            //    foreach (var item in Items)
            //    {
            //        var preItem = new ItemPreview(preWork, item.Id);
            //        preItem.MasterItemExist = item.MasterItem != null;
            //        preItem.MasterItemName = item.MasterItemName;
            //        preItem.MasterItemTag = item.MasterItemTag;
            //        preItem.SingularMasterTypeName = MasterSide.Definition.SingularItemTypeName;
            //        preItem.PluralMasterTypeName = MasterSide.Definition.PluralItemTypeName;
            //        preItem.MasterTypeIsMale = MasterSide.Definition.ItemTypeIsMale;

            //        var preSides = new List<ItemSidePreview>();
            //        foreach (var side in item.SideRunners)
            //        {
            //            var preSide = new ItemSidePreview(preItem, side.Side.Id);
            //            preSide.Key = side.Key.ToString();
            //            preSide.SideAllowInsert = side.Side.Definition.AllowInsert;
            //            preSide.SideAllowDelete = side.Side.Definition.AllowDelete;
            //            preSide.SideAllowUpdate = side.Side.Definition.AllowUpdate;
            //            preSide.SideItemExist = side.SideItem != null;
            //            preSide.SideItemName = side.SideItemName;
            //            preSide.SideItemTag = side.SideItemTag;
            //            preSide.SingularSideTypeName = side.Side.Definition.SingularItemTypeName;
            //            preSide.PluralSideTypeName = side.Side.Definition.PluralItemTypeName;
            //            preSide.ItemTypeIsMale = side.Side.Definition.ItemTypeIsMale;
            //            preSide.SideName = side.Name;
            //            foreach (var pro in side.Properties)
            //            {
            //                var prePro = new PropertyPreview(preSide);
            //                prePro.MasterValue = pro.MasterNamingFunction(pro.MasterValue);
            //                prePro.SideValue = pro.SideNamingFunction(pro.SideValue);
            //                prePro.PropertyName = pro.PropertyName;
            //                preSide.Properties.Add(prePro);
            //            }
            //            ICollection<ItemRelationPreview> ProcessSubItems(object parent, ICollection<IItemRunner> items)
            //            {
            //                var res = new List<ItemRelationPreview>();
            //                foreach (var i in items)
            //                {
            //                    ItemRelationPreview rel;
            //                    if (parent is ItemSidePreview)
            //                        rel = new ItemRelationPreview(parent as ItemSidePreview, i.Id);
            //                    else
            //                        rel = new ItemRelationPreview(parent as ItemRelationPreview, i.Id);
            //                    rel.SideAllowInsert = i.SideRunners.Single().Side.Definition.AllowInsert;
            //                    rel.SideAllowDelete = i.SideRunners.Single().Side.Definition.AllowDelete;
            //                    rel.SideAllowUpdate = i.SideRunners.Single().Side.Definition.AllowUpdate;
            //                    rel.MasterItemExist = i.MasterItem != null;
            //                    rel.MasterItemName = i.MasterItemName;
            //                    rel.MasterItemTag = i.MasterItemTag;
            //                    rel.SingularMasterTypeName = i.MasterRunner.Definition.SingularItemTypeName;
            //                    rel.PluralMasterTypeName = i.MasterRunner.Definition.PluralItemTypeName;
            //                    rel.SingularSideTypeName = i.SideRunners.Single().Side.Definition.SingularItemTypeName;
            //                    rel.PluralSideTypeName = i.SideRunners.Single().Side.Definition.PluralItemTypeName;
            //                    var subSide = i.SideRunners.Single();
            //                    rel.Key = subSide.Key.ToString();
            //                    rel.SideItemExist = subSide.SideItem != null;
            //                    rel.SideItemName = subSide.SideItemName;
            //                    rel.SideItemTag = subSide.SideItemTag;
            //                    rel.SideName = subSide.Name;
            //                    foreach (var pro in subSide.Properties)
            //                    {
            //                        var prePro = new PropertyPreview(rel);
            //                        prePro.MasterValue = pro.MasterNamingFunction(pro.MasterValue);
            //                        prePro.SideValue = pro.SideNamingFunction(pro.SideValue);
            //                        prePro.PropertyName = pro.PropertyName;
            //                        rel.Properties.Add(prePro);
            //                    }
            //                    rel.Relations = ProcessSubItems(rel, subSide.SubItems);
            //                    res.Add(rel);
            //                }
            //                return res;
            //            }
            //            preSide.Relations = ProcessSubItems(preSide, side.SubItems);
            //            preSides.Add(preSide);
            //        }
            //        preItem.Sides = preSides;
            //        preItems.Add(preItem);
            //    }
            //    preWork.Items = preItems;

            //    // Check result and suggest an action
            //    printer.WriteLine("Determining default actions ...");
            //    foreach (var item in preWork.Items)
            //    {
            //        foreach (var side in item.Sides)
            //        {
            //            if (!item.MasterItemExist && side.SideAllowDelete)
            //                side.Action = SynchronizationAction.Delete;
            //            else if (!side.SideItemExist && side.SideAllowInsert)
            //                side.Action = SynchronizationAction.Insert;
            //            else if (side.Properties.Count() > 0 && side.SideAllowUpdate)
            //                side.Action = SynchronizationAction.Update;
            //            void ProcessRelations(ICollection<ItemRelationPreview> relations)
            //            {
            //                foreach (var rel in relations)
            //                {
            //                    if (!rel.MasterItemExist && rel.SideAllowDelete)
            //                        rel.Action = SynchronizationAction.Delete;
            //                    else if (!rel.SideItemExist && rel.SideAllowInsert)
            //                        rel.Action = SynchronizationAction.Insert;
            //                    else if (rel.Properties.Count() > 0 && rel.SideAllowUpdate)
            //                        rel.Action = SynchronizationAction.Update;
            //                    ProcessRelations(rel.Relations);
            //                }
            //            }
            //            ProcessRelations(side.Relations);
            //        }
            //    }
            //    return preWork;
            //});
        }
    }
}
