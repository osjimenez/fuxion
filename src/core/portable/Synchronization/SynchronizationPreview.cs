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
    }
}
