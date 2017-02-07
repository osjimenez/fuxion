using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    public class SynchronizationPropertyPreview
    {
        internal SynchronizationPropertyPreview() { }
        public string PropertyName { get; set; }
        public string MasterValue { get; set; }
        public string SideValue { get; set; }
    }
    public class SynchronizationItemSidePreview
    {
        internal SynchronizationItemSidePreview() { }
        internal SynchronizationItemSidePreview(Guid id) { Id = id; }
        public Guid Id { get; set; }

        public bool SideItemExist { get; set; }
        public string SideName { get; set; }
        public string Key { get; set; }
        public string SideItemName { get; set; }
        public SynchronizationAction Action { get; set; }

        public IList<SynchronizationPropertyPreview> Properties { get; set; }
    }
    public class SynchronizationItemPreview
    {
        internal SynchronizationItemPreview() { }
        internal SynchronizationItemPreview(Guid id) { Id = id; }
        public Guid Id { get; set; }
        public bool MasterItemExist { get; set; }
        public string MasterItemName { get; set; }
        public IList<SynchronizationItemSidePreview> Sides { get; set; }
    }
    public class SynchronizationWorkPreview
    {
        internal SynchronizationWorkPreview() { }
        internal SynchronizationWorkPreview(Guid id) { Id = id; }
        public Guid Id { get; set; }
        public IList<SynchronizationItemPreview> Items { get; set; }
    }
    public class SynchronizationSessionPreview
    {
        internal SynchronizationSessionPreview() { }
        internal SynchronizationSessionPreview(Guid id) { Id = id; }
        public Guid Id { get; set; }
        public IList<SynchronizationWorkPreview> Works { get; set; }
        public void Print()
        {
            // Print preview results
            Printer.Foreach("Preview: ", Works, work =>
            {
                Printer.WriteLine("Work:");
                Printer.Indent(() =>
                {
                    foreach (var item in work.Items)
                    {
                        Printer.WriteLine($"Item '{(item.MasterItemExist ? item.MasterItemName : "null")}' has '{item.Sides.Count()}' sides");
                        Printer.Indent(() =>
                        {
                            foreach (var side in item.Sides)
                            {
                                if (side.SideItemExist)
                                {
                                    if (side.SideItemName.StartsWith("Tom"))
                                    {
                                        side.Action = SynchronizationAction.None;
                                    }
                                    Printer.WriteLine($"{side.Action.ToString().ToUpper()} - In '{side.SideName}' side is named '{side.SideItemName}' with key '{side.Key}' and has '{side.Properties.Count()}' change(s)");
                                    Printer.Indent(() =>
                                    {
                                        foreach (var pro in side.Properties)
                                        {
                                            Printer.WriteLine($"Property '{pro.PropertyName}' will be changed from '{pro.SideValue}' to '{pro.MasterValue}'");
                                        }
                                    });
                                }
                                else Printer.WriteLine($"{side.Action.ToString().ToUpper()} - In '{side.SideName}' side does not exist");
                            }
                        });
                    }
                });
            });
        }
    }
}
