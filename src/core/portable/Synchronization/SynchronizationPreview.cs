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
    public class SynchronizationItemRelationPreview
    {
        internal SynchronizationItemRelationPreview() { }

        public bool MasterItemExist { get; set; }
        public string MasterItemName { get; set; }

        public bool SideItemExist { get; set; }
        public string SideName { get; set; }
        public string Key { get; set; }
        public string SideItemName { get; set; }
        public SynchronizationAction Action { get; set; }
        public ICollection<SynchronizationPropertyPreview> Properties { get; set; } = new List<SynchronizationPropertyPreview>();
        public ICollection<SynchronizationItemRelationPreview> Relations { get; set; } = new List<SynchronizationItemRelationPreview>();
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

        public ICollection<SynchronizationPropertyPreview> Properties { get; set; } = new List<SynchronizationPropertyPreview>();
        public ICollection<SynchronizationItemRelationPreview> Relations { get; set; } = new List<SynchronizationItemRelationPreview>();
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
            Printer.Foreach("Preview: ", Works, work =>
            {
                Printer.Foreach("Work:", work.Items, item =>
                {
                    Printer.Foreach($"Item '{(item.MasterItemExist ? item.MasterItemName : "null")}' has '{item.Sides.Count()}' sides", item.Sides, side =>
                    {
                        if (side.SideItemExist)
                        {
                            Printer.Foreach($"{side.Action.ToString().ToUpper()} - In '{side.SideName}' side is named '{side.SideItemName}' with key '{side.Key}' and has '{side.Properties.Count()}' change(s)", side.Properties, pro =>
                            {
                                Printer.WriteLine($"Property '{pro.PropertyName}' will be changed from '{pro.SideValue}' to '{pro.MasterValue}'");
                            });
                        }
                        else Printer.WriteLine($"{side.Action.ToString().ToUpper()} - In '{side.SideName}' side does not exist");
                    });
                });
            });
        }
    }
}
