using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    public interface ISyncPropertyPreview
    {
        string PropertyName { get; }
        object MasterValue { get; }
        object SideValue { get; }
    }
    [CollectionDataContract]
    public class SyncPropertyPreview<TMasterProperty, TSideProperty> : ISyncPropertyPreview
    {
        public SyncPropertyPreview(string propertyName, TMasterProperty masterValue, TSideProperty sideValue)
        {
            PropertyName = propertyName;
            MasterValue = masterValue;
            SideValue = sideValue;
        }

        public string PropertyName { get; set; }
        public TMasterProperty MasterValue { get; }
        object ISyncPropertyPreview.MasterValue { get { return MasterValue; } }
        public TSideProperty SideValue { get; }
        object ISyncPropertyPreview.SideValue { get { return SideValue; } }
    }
    public enum SyncAction
    {
        None = 0,
        Add,
        Delete,
        Update
    }
    public interface ISyncSidePreview : IEnumerable<ISyncPropertyPreview>
    {
        Guid SyncId { get; }
        string Name { get; }
        object SideItem { get; }
        string SideItemName { get; }
        SyncAction Action { get; set; }
    }
    [CollectionDataContract]
    public class SyncSidePreview<TSideItem> : List<ISyncPropertyPreview>, ISyncSidePreview
    {
        public SyncSidePreview(Guid syncId, string name, TSideItem sideItem, string sideItemName)
        {
            SyncId = syncId;
            Name = name;
            SideItem = sideItem;
            SideItemName = sideItemName;
        }
        public Guid SyncId { get; }
        public string Name { get; }
        public TSideItem SideItem { get; }
        object ISyncSidePreview.SideItem { get { return SideItem; } }
        public string SideItemName { get; }
        public SyncAction Action { get; set; }
    }
    public interface ISyncItemPreview : IEnumerable<ISyncSidePreview>
    {
        string Key { get; }
        object MasterItem { get; }
        string MasterName { get; }
    }
    [CollectionDataContract]
    public class SyncItemPreview<TMasterItem, TKey> : List<ISyncSidePreview>, ISyncItemPreview
    {
        public SyncItemPreview(TKey key, TMasterItem masterItem, string masterName)
        {
            Key = key;
            MasterItem = masterItem;
            MasterName = masterName;
        }
        [IgnoreDataMember]
        public TKey Key { get; }
        string ISyncItemPreview.Key { get { return Key.ToString(); } }

        [DataMember]
        public string MasterName { get; }
        [IgnoreDataMember]
        public TMasterItem MasterItem { get; }
        object ISyncItemPreview.MasterItem { get { return MasterItem; } }
    }
    //public interface ISyncWorkPreview : IEnumerable<ISyncItemPreview> {
    //    Guid SyncId { get; }
    //}
    [CollectionDataContract]
    public class SyncWorkPreview : List<ISyncItemPreview>
    {
        internal SyncWorkPreview() { }
        public SyncWorkPreview(Guid syncId) { SyncId = syncId; }
        public Guid SyncId { get; }
    }
    //public interface ISyncSessionPreview : IEnumerable<ISyncWorkPreview> { }
    //[KnownType(typeof(SyncSessionPreview))]
    //[KnownType(typeof(SyncWorkPreview))]
    //[KnownType(typeof(SyncItemPreview<,>))]
    [CollectionDataContract]
    public class SyncSessionPreview : List<SyncWorkPreview>
    {
        internal SyncSessionPreview() { }
        internal SyncSessionPreview(Guid syncId) { SyncId = syncId; }
        public Guid SyncId { get; }
    }
}
