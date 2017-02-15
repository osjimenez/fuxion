﻿using Fuxion.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    internal class SessionRunner
    {
        public SessionRunner(SessionDefinition definition) { this.definition = definition; }
        SessionDefinition definition;
        ICollection<WorkRunner> works = new List<WorkRunner>();
        public Guid Id { get { return definition.Id; } }
        public Task<SessionPreview> PreviewAsync()
        {
            return Printer.IndentAsync($"Previewing synchronization session '{definition.Name}'",
                async () =>
                {
                    var res = new SessionPreview(definition.Id);
                    works = definition.Works.Select(d => new WorkRunner(d)).ToList();
                    var tasks = works.Select(w => w.PreviewAsync());
                    var resTask = await Task.WhenAll(tasks);
                    res.Works = resTask;
                    return res;
                });
        }
        public Task RunAsync(SessionPreview preview)
        {
            return Printer.IndentAsync("Running session:", async () =>
            {
                // 1 - Insert 1º level
                // 2 - Update 1º level

                // 3 - Insert 2º level
                // 4 - Update 2º level
                // . - ...............
                // 5 - Insert nº level
                // 6 - Update nº level

                // 7 - Delete nº level
                // . - ...............
                // 8 - Delete 2º level
                // 9 - Delete 1º level

                List<Task> tasks = new List<Task>();
                List<Tuple<ICollection<ItemPreview>, WorkRunner>> main = new List<Tuple<ICollection<ItemPreview>, WorkRunner>>();
                List<Tuple<ICollection<ItemRelationPreview>, IItemSideRunner>> levels = new List<Tuple<ICollection<ItemRelationPreview>, IItemSideRunner>>();

                foreach (var work in preview.Works)
                {
                    var runWork = works.Single(w => w.Id == work.Id);
                    main.Add(new Tuple<ICollection<ItemPreview>, WorkRunner>(work.Items, runWork));
                }
                await Printer.ForeachAsync("Inserting level 0", main, async m =>
                {
                    levels.AddRange(await ProcessWork(m.Item1, m.Item2, SynchronizationAction.Insert));
                }, false);
                await Printer.ForeachAsync("Updating level 0",main, async m =>
                {
                    levels.AddRange(await ProcessWork(m.Item1, m.Item2, SynchronizationAction.Update));
                }, false);
                levels = levels.Distinct().ToList();
                int level = 1;
                while (levels.Any())
                {
                    var aux = levels.ToList();
                    levels.Clear();
                    await Printer.ForeachAsync($"Inserting level {level}", aux, async lev =>
                    {
                        levels.AddRange(await ProcessRelations(lev.Item1, lev.Item2, SynchronizationAction.Insert, level));
                    }, false);
                    await Printer.ForeachAsync($"Updating level {level}", aux, async lev =>
                    {
                        levels.AddRange(await ProcessRelations(lev.Item1, lev.Item2, SynchronizationAction.Update, level));
                    }, false);
                    levels = levels.Distinct().ToList();
                    level++;
                }
                await Printer.ForeachAsync("Deleting level 0", main, async m =>
                {
                    levels.AddRange(await ProcessWork(m.Item1, m.Item2, SynchronizationAction.Delete));
                }, false);

                level = 1;
                while (levels.Any())
                {
                    var aux = levels.ToList();
                    levels.Clear();
                    await Printer.ForeachAsync($"Deleting level {level}", aux, async lev =>
                    {
                        levels.AddRange(await ProcessRelations(lev.Item1, lev.Item2, SynchronizationAction.Delete, level));
                    }, false);
                    level++;
                }
            });
        }
        private static async Task<ICollection<Tuple<ICollection<ItemRelationPreview>, IItemSideRunner>>> ProcessWork(ICollection<ItemPreview> items, WorkRunner runner, SynchronizationAction action)
        {
            ICollection<Tuple<ICollection<ItemRelationPreview>, IItemSideRunner>> levels = new List<Tuple<ICollection<ItemRelationPreview>, IItemSideRunner>>();
            foreach (var item in items)
            {
                var runItem = runner.Items.Single(i => i.Id == item.Id);

                foreach (var side in item.Sides)
                {
                    var runSide = runner.Sides.Single(s => s.Id == side.Id);
                    var runItemSide = runItem.Sides.Single(s => s.Side.Id == side.Id);
                    var map = new Func<IItemRunner, IItemSideRunner, object>((i, s) =>
                    {
                        if (runSide.Comparator.GetItemTypes().Item1 == runner.MasterSide.GetItemType())
                        {
                            // Master is A in this comparator
                            if (item.MasterItemExist)
                                return runSide.Comparator.MapAToB(i.MasterItem, s.SideItem);
                        }
                        else
                        {
                            // Master is B in this comparator
                            if (item.MasterItemExist)
                                return runSide.Comparator.MapBToA(i.MasterItem, s.SideItem);
                        }
                        return null;
                    });

                    if (side.Action == action && side.Action == SynchronizationAction.Insert)
                    {
                        var newItem = map(runItem, runItemSide);
                        await runSide.InsertAsync(newItem);
                        foreach (var subItem in runItemSide.SubItems)
                            subItem.Sides.First().Side.Source = newItem;
                    }
                    else if (side.Action == action && side.Action == SynchronizationAction.Update)
                    {
                        var newItem = map(runItem, runItemSide);
                        await runSide.UpdateAsync(newItem);
                        foreach (var subItem in runItemSide.SubItems)
                            subItem.Sides.First().Side.Source = newItem;
                    }
                    else if (side.Action == action && side.Action == SynchronizationAction.Delete)
                    {
                        await runSide.DeleteAsync(runItemSide.SideItem);
                    }
                    levels.Add(new Tuple<ICollection<ItemRelationPreview>, IItemSideRunner>(side.Relations, runItemSide));
                }
            }
            return levels;
        }
        private static async Task<ICollection<Tuple<ICollection<ItemRelationPreview>, IItemSideRunner>>> ProcessRelations(ICollection<ItemRelationPreview> relations, IItemSideRunner runItemSide, SynchronizationAction action, int level = 1)
        {
            if (!relations.Any()) return Enumerable.Empty<Tuple<ICollection<ItemRelationPreview>, IItemSideRunner>>().ToList();
            List<Tuple<ICollection<ItemRelationPreview>, IItemSideRunner>> nextLevels = new List<Tuple<ICollection<ItemRelationPreview>, IItemSideRunner>>();
            foreach (var rel in relations)
            {
                var runSubItem = runItemSide.SubItems.Single(si => si.Id == rel.Id);
                var runSubItemSide = runSubItem.Sides.First();
                var runSubSide = runSubItemSide.Side;
                var subMap = new Func<IItemRunner, IItemSideRunner, object>((i, s) =>
                {
                    if (runSubSide.Comparator.GetItemTypes().Item1 == runSubItem.MasterItem.GetType())
                    {
                        // Master is A in this comparator
                        if (runSubItem.MasterItem != null)
                            return runSubSide.Comparator.MapAToB(i.MasterItem, s.SideItem);
                    }
                    else
                    {
                        // Master is B in this comparator
                        if (runSubItem.MasterItem != null)
                            return runSubSide.Comparator.MapBToA(i.MasterItem, s.SideItem);
                    }
                    return null;
                });
                if (rel.Action == action && rel.Action == SynchronizationAction.Insert)
                {
                    var newItem = subMap(runSubItem, runSubItemSide);
                    await runSubSide.InsertAsync(newItem);
                    foreach (var subItem in runSubItemSide.SubItems)
                        subItem.Sides.First().Side.Source = newItem;
                }
                else if (rel.Action == action && rel.Action == SynchronizationAction.Update)
                {
                    var newItem = subMap(runSubItem, runSubItemSide);
                    await runSubSide.UpdateAsync(newItem);
                    foreach (var subItem in runSubItemSide.SubItems)
                        subItem.Sides.First().Side.Source = newItem;
                }
                else if (rel.Action == action && rel.Action == SynchronizationAction.Delete)
                {
                    await runSubSide.DeleteAsync(runSubItemSide.SideItem);
                }
                if (rel.Relations.Any())
                    nextLevels.Add(new Tuple<ICollection<ItemRelationPreview>, IItemSideRunner>(rel.Relations, runSubItemSide));
            }
            return nextLevels;
        }
    }
}