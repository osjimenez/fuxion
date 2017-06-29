using Fuxion.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
namespace Fuxion.Synchronization
{
    internal class SessionRunner
    {
        public SessionRunner(Session definition, IPrinter printer)
        {
            this.printer = printer;
            this.Session = definition;
        }
        IPrinter printer;
        internal Session Session { get; set; }
        ICollection<WorkRunner> works = new List<WorkRunner>();
        public Guid Id { get { return Session.Id; } }
        public Task<SessionPreview> PreviewAsync(bool includeNoneActionItems, IPrinter printer)
        {
            return printer.IndentAsync($"Previewing synchronization session '{Session.Name}' {(Session.MakePreviewInParallel ? "in parallel" : "sequentially")}",
                async () =>
                {
                    var res = new SessionPreview(Session.Id);
                    works = Session.Works.Select(w => new WorkRunner(w, printer)).ToList();
                    List<WorkPreview> resTask = new List<WorkPreview>();
                    if (Session.MakePreviewInParallel)
                    {
                        var tasks = works.Select(w => w.PreviewAsync(printer));
                        resTask = (await Task.WhenAll(tasks)).ToList();
                    }else
                    {
                        foreach (var work in works)
                            resTask.Add(await work.PreviewAsync(printer));
                    }
                    res.Works = resTask.Select(w =>
                    {
                        w.Session = res;
                        return w;
                    }).ToList();
                    // Run post preview actions
                    foreach (var work in works.Where(w => w.Definition.PostPreviewAction != null))
                        work.Definition.PostPreviewAction(res);
                    // Clean results
                    if (!includeNoneActionItems)
                        res.CleanNoneActions();
                    return res;
                });
        }
        public Task RunAsync(SessionPreview preview, IPrinter printer)
        {
            return printer.IndentAsync("Running session:", async () =>
            {
                // Run order:

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

                var main = new List<(IList<ItemPreview> Items, WorkRunner Runner)>(
                    preview.Works.Select(work => (work.Items, works.Single(w => w.Definition.Id == work.Id))));
                var nextLevels = new List<(ICollection<ItemRelationPreview> Items, IItemSideRunner Runner)>();

                await printer.ForeachAsync("Inserting level 0", main,
                    async m => nextLevels.AddRange(await ProcessWork(m.Items, m.Runner, SynchronizationAction.Insert)
                        .Transform(async item =>
                        {
                            await item.ProcessAction();
                            return item.NextLevels;
                        })),
                    false);
                await printer.ForeachAsync("Updating level 0", main,
                    async m => nextLevels.AddRange(await ProcessWork(m.Items, m.Runner, SynchronizationAction.Update)
                        .Transform(async item =>
                        {
                            await item.ProcessAction();
                            return item.NextLevels;
                        })),
                    false);
                nextLevels = nextLevels.Distinct().ToList();
                int level = 1;
                while (nextLevels.Any())
                {
                    var aux = nextLevels.ToList();
                    nextLevels.Clear();
                    await printer.ForeachAsync($"Inserting level {level}", aux,
                        async lev => nextLevels.AddRange(await ProcessRelations(lev.Items, lev.Runner, SynchronizationAction.Insert)
                            .Transform(async item =>
                            {
                                await item.ProcessAction();
                                return item.NextLevels;
                            })),
                        false);
                    await printer.ForeachAsync($"Updating level {level}", aux, 
                        async lev => nextLevels.AddRange(await ProcessRelations(lev.Items, lev.Runner, SynchronizationAction.Update)
                            .Transform(async item =>
                            {
                                await item.ProcessAction();
                                return item.NextLevels;
                            })),
                        false);
                    nextLevels = nextLevels.Distinct().ToList();
                    level++;
                }
                level = 0;
                var processActions = new List<(int Level, Func<Task> Action)>();
                foreach(var m in main)
                {
                    nextLevels.AddRange(ProcessWork(m.Items, m.Runner, SynchronizationAction.Delete).Transform(item =>
                    {
                        processActions.Add((level, item.ProcessAction));
                        return item.NextLevels;
                    }));
                }
                level++;
                while (nextLevels.Any())
                {
                    var aux = nextLevels.ToList();
                    nextLevels.Clear();
                    foreach(var lev in aux)
                    {
                        nextLevels.AddRange(ProcessRelations(lev.Items, lev.Runner, SynchronizationAction.Delete).Transform(item =>
                        {
                            processActions.Add((level, item.ProcessAction));
                            return item.NextLevels;
                        }));
                    }
                    level++;
                }
                var lastLevel = -1;
                foreach (var act in processActions.OrderByDescending(a => a.Level))
                {
                    if (act.Level != lastLevel)
                        await printer.IndentAsync($"Deleting level {act.Level}", () => act.Action());
                    else
                        await act.Action();
                    lastLevel = act.Level;
                }
                // Run post run actions
                foreach (var work in works.Where(w => w.Definition.PostRunAction != null))
                    work.Definition.PostRunAction(preview);
            });
        }
        private static (ICollection<(
            ICollection<ItemRelationPreview> Items, 
            IItemSideRunner Runner)> NextLevels,
            Func<Task> ProcessAction)
            ProcessWork(ICollection<ItemPreview> items, WorkRunner runner, SynchronizationAction action)
        {
            var levels = new List<(ICollection<ItemRelationPreview>, IItemSideRunner)>();
            var processActions = new List<Func<Task>>();
            foreach (var item in items)
            {
                var runItem = runner.Items.Single(i => i.Id == item.Id);
                foreach (var side in item.Sides)
                {
                    var runSide = runner.Sides.Single(s => s.Id == side.Id);
                    var runItemSide = runItem.SideRunners.Single(s => s.Side.Id == side.Id);
                    // ----
                    processActions.Add(new Func<Task>(async () =>
                    {
                        var map = new Func<IItemRunner, IItemSideRunner, object>((i, s) =>
                        {
                            if (runSide.Comparator.GetItemTypes().typeA == runner.MasterSide.GetItemType())
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

                        if (side.IsEnabled && side.Action == action && side.Action == SynchronizationAction.Insert)
                        {
                            var newItem = map(runItem, runItemSide);
                            await runSide.InsertAsync(newItem);
                            foreach (var subItem in runItemSide.SubItems)
                                subItem.SideRunners.First().Side.Source = newItem;
                        }
                        else if (side.IsEnabled && side.Action == action && side.Action == SynchronizationAction.Update)
                        {
                            var newItem = map(runItem, runItemSide);
                            await runSide.UpdateAsync(newItem);
                            foreach (var subItem in runItemSide.SubItems)
                                subItem.SideRunners.First().Side.Source = newItem;
                        }
                        else if (side.IsEnabled && side.Action == action && side.Action == SynchronizationAction.Delete)
                        {
                            await runSide.DeleteAsync(runItemSide.SideItem);
                        }
                    }));
                    // ----
                    levels.Add((side.Relations, runItemSide));
                }
            }
            return (levels, processActions.Transform(acts =>
                new Func<Task>(async () =>
                {
                    foreach (var act in acts) await act();
                })));
        }
        private static (
            ICollection<(
                ICollection<ItemRelationPreview> Items, 
                IItemSideRunner Runner)> NextLevels,
            Func<Task> ProcessAction) 
            ProcessRelations(ICollection<ItemRelationPreview> relations, IItemSideRunner runItemSide, SynchronizationAction action)
        {
            if (!relations.Any()) return (Enumerable.Empty<(ICollection<ItemRelationPreview>, IItemSideRunner)>().ToList(), ()=>Task.FromResult(0));
            var levels = new List<(ICollection<ItemRelationPreview>, IItemSideRunner)>();
            var processActions = new List<Func<Task>>();
            foreach (var rel in relations)
            {
                var runSubItem = runItemSide.SubItems.Single(si => si.Id == rel.Id);
                var runSubItemSide = runSubItem.SideRunners.First();
                // -----
                processActions.Add(new Func<Task>(async () => {
                    var runSubSide = runSubItemSide.Side;
                    var subMap = new Func<IItemRunner, IItemSideRunner, object>((i, s) =>
                    {
                        if (runSubSide.Comparator.GetItemTypes().typeA.GetTypeInfo().IsAssignableFrom(runSubItem.MasterItem.GetType().GetTypeInfo()))
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
                    if (rel.IsEnabled && rel.Action == action && rel.Action == SynchronizationAction.Insert)
                    {
                        var newItem = subMap(runSubItem, runSubItemSide);
                        await runSubSide.InsertAsync(newItem);
                        foreach (var subItem in runSubItemSide.SubItems)
                            subItem.SideRunners.First().Side.Source = newItem;
                    }
                    else if (rel.IsEnabled && rel.Action == action && rel.Action == SynchronizationAction.Update)
                    {
                        var newItem = subMap(runSubItem, runSubItemSide);
                        await runSubSide.UpdateAsync(newItem);
                        foreach (var subItem in runSubItemSide.SubItems)
                            subItem.SideRunners.First().Side.Source = newItem;
                    }
                    else if (rel.IsEnabled && rel.Action == action && rel.Action == SynchronizationAction.Delete)
                    {
                        await runSubSide.DeleteAsync(runSubItemSide.SideItem);
                    }
                }));
                // -----
                if (rel.Relations.Any())
                    levels.Add((rel.Relations, runSubItemSide));
            }
            return (levels, processActions.Transform(acts =>
                new Func<Task>(async () =>
                {
                    foreach (var act in acts) await act();
                })));
        }
    }
}