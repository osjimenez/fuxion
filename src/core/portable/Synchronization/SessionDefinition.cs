using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    public class SessionDefinition
    {
        internal Guid Id { get; } = Guid.NewGuid();
        public string Name { get; set; }
        public ICollection<WorkDefinition> Works { get; set; } = new List<WorkDefinition>();
        //public Task<SessionPreview> PreviewAsync()
        //{
        //    return Printer.IndentAsync($"Previewing synchronization session '{Name}'",
        //        async () =>
        //    {
        //        var res = new SessionPreview(Id);
        //        var tasks = Works.Select(w => w.PreviewAsync());
        //        var resTask = await Task.WhenAll(tasks);
        //        res.Works = resTask;
        //        return res;
        //    });
        //}
        //public async Task RunAsync(SessionPreview preview)
        //{
        //    // 1 - Insert 1º level
        //    // 2 - Update 1º level
        //    // 3 - Insert 2º level
        //    // 4 - Update 2º level
        //    // . - ...............
        //    // 5 - Insert nº level
        //    // 6 - Update nº level
        //    // 7 - Delete nº level
        //    // . - ...............
        //    // 8 - Delete 2º level
        //    // 9 - Delete 1º level

        //    List<Task> tasks = new List<Task>();

        //    foreach(var work in preview.Works)
        //    {
        //        var runWork = Works.Single(w => w.Id == work.Id);
        //        foreach(var item in work.Items)
        //        {
        //            var runItem = runWork.Items.Single(i => i.Id == item.Id);
        //            foreach(var side in item.Sides)
        //            {
        //                var runSide = runWork.InternalSides.Single(s => s.Id == side.Id);
        //                var runItemSide = runItem.Sides.Single(s => s.Side.Id == side.Id);
        //                var map = new Func<IItem, IItemSide, object>((i, s) =>
        //                {
        //                    if (runSide.Comparator.GetItemTypes().Item1 == runWork.MasterSide.GetItemType())
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
        //                if (side.Action == SynchronizationAction.Insert)
        //                {
        //                    var newItem = map(runItem, runItemSide);
        //                    await runSide.InsertAsync(newItem);
        //                    foreach (var subItem in runItemSide.SubItems)
        //                        subItem.Sides.First().Side.Source = newItem;
        //                }
        //                await ProcessRelations(side.Relations, runItemSide);
        //            }
        //        }
        //    }

        //    //var actions = new List<Action>();
        //    //await Printer.ForeachAsync($"Running synchronization session '{Name}'", Works, async work =>
        //    //{
        //    //    var workPre = preview.Works.FirstOrDefault(w => w.Id == work.Id);
        //    //    var act = new Action(() => { });
        //    //    await work.RunAsync(workPre, act);
        //    //});
        //    //foreach (var act in actions)
        //    //    act();
        //}
        //private static async Task ProcessRelations(ICollection<ItemRelationPreview> relations, IItemSide runItemSide)
        //{
        //    foreach (var rel in relations)
        //    {
        //        var runSubItem = runItemSide.SubItems.Single(si => si.Id == rel.Id);
        //        var runSubItemSide = runSubItem.Sides.First();
        //        var runSubSide = runSubItemSide.Side;
        //        var subMap = new Func<IItem, IItemSide, object>((i, s) =>
        //        {
        //            if (runSubSide.Comparator.GetItemTypes().Item1 == runSubItem.MasterItem.GetType())
        //            {
        //                // Master is A in this comparator
        //                if (runSubItem.MasterItem != null)
        //                    return runSubSide.Comparator.MapAToB(i.MasterItem, s.SideItem);
        //            }
        //            else
        //            {
        //                // Master is B in this comparator
        //                if (runSubItem.MasterItem != null)
        //                    return runSubSide.Comparator.MapBToA(i.MasterItem, s.SideItem);
        //            }
        //            return null;
        //        });
        //        if (rel.Action == SynchronizationAction.Insert)
        //        {
        //            var newItem = subMap(runSubItem, runSubItemSide);
        //            await runSubSide.InsertAsync(newItem);
        //            foreach (var subItem in runSubItemSide.SubItems)
        //                subItem.Sides.First().Side.Source = newItem;
        //        }
        //        await ProcessRelations(rel.Relations, runSubItemSide);
        //    }
        //}
    }
}
