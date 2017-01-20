using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    public class SyncPropertyPreview
    {
        public string PropertyName { get; set; }
        public string MasterValue { get; set; }
        public string SideValue { get; set; }
    }
    public class SyncItemSidePreview
    {
        internal SyncItemSidePreview() { }
        internal SyncItemSidePreview(Guid syncId) { SideId = syncId; }
        public Guid SideId { get; set; }

        public bool SideItemExist { get; set; }
        public string SideName { get; set; }
        public string Key { get; set; }
        public string SideItemName { get; set; }
        public SyncAction Action { get; set; }

        public IList<SyncPropertyPreview> Properties { get; set; }
    }
    public class SyncItemPreview
    {
        internal SyncItemPreview() { }
        internal SyncItemPreview(Guid syncId) { ItemId = syncId; }
        public Guid ItemId { get; set; }
        public bool MasterItemExist { get; set; }
        public string MasterItemName { get; set; }
        public IList<SyncItemSidePreview> Sides { get; set; }
    }
    public class SyncWorkPreview
    {
        internal SyncWorkPreview() { }
        internal SyncWorkPreview(Guid syncId) { WorkId = syncId; }
        public Guid WorkId { get; set; }
        public IList<SyncItemPreview> Items { get; set; }
    }
    public class SyncSessionPreview
    {
        internal SyncSessionPreview() { }
        internal SyncSessionPreview(Guid syncId) { SessionId = syncId; }
        public Guid SessionId { get; set; }
        public IList<SyncWorkPreview> Works { get; set; }
    }
}
