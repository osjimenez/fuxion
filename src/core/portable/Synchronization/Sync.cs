using Fuxion.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Fuxion.Reflection;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Fuxion.Synchronization
{
    public static class SyncExtensions
    {
        sdaflklñsjadklñj
        public static Type GetItemType(this ISyncSide me)
        {
            return me.GetType().GetTypeInfo().GenericTypeArguments[1];
        }
        public static Tuple<Type,Type> GetItemTypes(this ISyncComparator me)
        {
            return new Tuple<Type, Type>(me.GetType().GetTypeInfo().GenericTypeArguments[0], me.GetType().GetTypeInfo().GenericTypeArguments[1]);
        }
        public static Type GetKeyType(this ISyncComparator me)
        {
            return me.GetType().GetTypeInfo().GenericTypeArguments[2];
        }
        public static ISyncProperty Invert(this ISyncProperty me)
        {
            var meType = me.GetType();
            if (meType.IsSubclassOfRawGeneric(typeof(SyncProperty<,>)))
            {
                var resType = typeof(SyncProperty<,>).MakeGenericType(meType.GetTypeInfo().GenericTypeArguments[1], meType.GetTypeInfo().GenericTypeArguments[0]);
                var res = Activator.CreateInstance(resType, me.PropertyName, me.SideValue, me.MasterValue);
                return (ISyncProperty)res;
            }
            else throw new InvalidCastException($"The property '{me.PropertyName}' cannot be inverted because is of type '{meType.Name}' and is not subclass of generic type '{typeof(SyncProperty<,>).Name}'");
        }
        public static ISyncComparatorResult Invert(this ISyncComparatorResult me)
        {
            var meType = me.GetType();
            if (meType.IsSubclassOfRawGeneric(typeof(SyncComparatorResult<,,>)))
            {
                var resType = typeof(SyncComparatorResult<,,>).MakeGenericType(meType.GetTypeInfo().GenericTypeArguments[1], meType.GetTypeInfo().GenericTypeArguments[0], meType.GetTypeInfo().GenericTypeArguments[2]);
                var res = Activator.CreateInstance(resType);
                resType.GetRuntimeProperty(nameof(me.MasterItem)).SetValue(res, me.SideItem);
                resType.GetRuntimeProperty(nameof(me.SideItem)).SetValue(res, me.MasterItem);
                resType.GetRuntimeProperty(nameof(me.Key)).SetValue(res, me.Key);
                var addMethod = resType.GetRuntimeMethod("Add", new[] { typeof(ISyncProperty) });
                foreach(var pro in me)
                {
                    addMethod.Invoke(res, new[] { pro.Invert() });
                }
                return (ISyncComparatorResult)res;
            }
            else throw new InvalidCastException($"The item '{me}' cannot be inverted because is of type '{meType.Name}' and is not subclass of generic type '{typeof(SyncComparatorResult<,,>).Name}'");
        }
    }
    public interface ISyncProperty
    {
        string PropertyName { get; }
        object MasterValue { get; }
        object SideValue { get; }
    }
    public class SyncProperty<TMasterProperty, TSideProperty> : ISyncProperty
    {
        public SyncProperty(string propertyName, TMasterProperty masterValue, TSideProperty sideValue)
        {
            PropertyName = propertyName;
            MasterValue = masterValue;
            SideValue = sideValue;
        }

        public string PropertyName { get; set; }
        public TMasterProperty MasterValue { get; }
        object ISyncProperty.MasterValue { get { return MasterValue; } }
        public TSideProperty SideValue { get; }
        object ISyncProperty.SideValue { get { return SideValue; } }
    }

    public interface ISyncItemSide
    {
        Guid SyncId { get; }
        string Name { get; }
        object Key { get; }
        object SideItem { get; }
        string SideItemName { get; }
        IEnumerable<ISyncProperty> Properties { get; }
    }
    public class SyncItemSide<TSideItem, TKey> : ISyncItemSide
    {
        public SyncItemSide(Guid syncId, string name, TKey key, TSideItem sideItem, string sideItemName)
        {
            SyncId = syncId;
            Name = name;
            SideItem = sideItem;
            SideItemName = sideItemName;
        }
        public Guid SyncId { get; }
        public string Name { get; }
        public TKey Key { get; }
        object ISyncItemSide.Key { get { return Key; } }
        public TSideItem SideItem { get; }
        object ISyncItemSide.SideItem { get { return SideItem; } }
        public string SideItemName { get; }
        public IEnumerable<ISyncProperty> Properties { get; set; } = new List<ISyncProperty>();
    }
    public interface ISyncItem
    {
        Guid SyncId { get; }
        object MasterItem { get; }
        string MasterName { get; }
        IEnumerable<ISyncItemSide> Sides { get; }
    }
    public class SyncItem<TMasterItem> : ISyncItem
    {
        public SyncItem(TMasterItem masterItem, string masterName)
        {
            MasterItem = masterItem;
            MasterName = masterName;
        }
        public Guid SyncId { get; } = Guid.NewGuid();
        public string MasterName { get; }
        public TMasterItem MasterItem { get; }
        object ISyncItem.MasterItem { get { return MasterItem; } }
        public IEnumerable<ISyncItemSide> Sides { get; set; } = new List<ISyncItemSide>();
    }
    public interface ISyncSide {
        Guid SyncId { get; }
        bool IsMaster { get; }
        string Name { get; }
        string SingularItemTypeName { get; }
        string PluralItemTypeName { get; }
        Task Load();
        string GetItemName(object item);
        void Add(object item);
        void Delete(object item);
        void Update(object item);

        IEnumerable<object> Entries { get; set; }
        ISyncComparator Comparator { get; set; }
        IEnumerable<ISyncComparatorResult> Results { get; set; }
    }
    public class SyncSide<TSource, TItem, TKey> : ISyncSide
    {
        public Guid SyncId { get; } = Guid.NewGuid();
        public bool IsMaster { get; set; }
        public string Name { get; set; }
        public string SingularItemTypeName { get; set; }
        public string PluralItemTypeName { get; set; }

        public TSource Source { get; set; }
        public Func<TSource, ICollection<TItem>> Loader { get; set; }
        public Func<TItem, string> Nominator { get; set; }
        public Action<TSource, TItem> Adder { get; set; }
        public Action<TSource, TItem> Remover { get; set; }
        public Action<TSource, TItem> Updater { get; set; }

        //IEnumerable<object> ISyncSide.Load() => Loader(Source).Cast<object>();
        public string GetItemName(object item) => item != null ? Nominator((TItem)item) : null;

        public void Add(object item)
        {
            if(item is TItem)
                Adder(Source, (TItem)item);
            else throw new ArgumentException($"'{nameof(item)}' must be of type '{typeof(TItem).Name}'", nameof(item));
        }
        public void Delete(object item)
        {
            if (item is TItem)
                Remover(Source, (TItem)item);
            else throw new ArgumentException($"'{nameof(item)}' must be of type '{typeof(TItem).Name}'", nameof(item));
        }
        public void Update(object item)
        {
            if (item is TItem)
                Updater(Source, (TItem)item);
            else throw new ArgumentException($"'{nameof(item)}' must be of type '{typeof(TItem).Name}'", nameof(item));
        }

        public IEnumerable<object> Entries { get; set; }
        public ISyncComparator Comparator { get; set; }
        public IEnumerable<ISyncComparatorResult> Results { get; set; }
        public Task Load() => TaskManager.StartNew(() => Entries = Loader(Source).Cast<object>());
    }
    public class SyncWork
    {
        public Guid SyncId { get; } = Guid.NewGuid();
        public IEnumerable<ISyncSide> Sides { get; set; }
        public IEnumerable<ISyncComparator> Comparators { get; set; }
        //public IEnumerable<SyncWork> SubWorks { get; set; }
        public IList<ISyncItem> Items { get; set; } = new List<ISyncItem>();

        ISyncSide MasterSide { get; set; }
        //List<WorkEntry> Entries { get; set; }

        //class WorkEntry
        //{
        //    public IEnumerable<object> Items { get; set; }
        //    public ISyncSide Side { get; set; }
        //    public ISyncComparator Comparator { get; set; }
        //    public IEnumerable<ISyncComparatorResult> Results { get; set; }
        //    public Task Load() => TaskManager.StartNew(() => Items = Side.Load());
        //}

        public async Task<SyncWorkPreview> PreviewAsync()
        {
            // Get master side
            var masters = Sides.Where(s => s.IsMaster);
            if (masters.Count() != 1) throw new ArgumentException("One, and only one SyncSide must be the master side");
            MasterSide = masters.Single();
            // Create entries from sides
            
            //Entries = Sides.Select(s => new WorkEntry { Side = s }).ToList();
            //var masterEntry = Sides.Single(s => s.IsMaster);

            // Search for comparators for any side with master side
            foreach (var side in Sides.Where(s => !s.IsMaster))
            {
                var cc = Comparators.Where(c =>
                {
                    var mt = MasterSide.GetItemType();
                    var st = side.GetItemType();
                    var ct = c.GetItemTypes();
                    return (ct.Item1 == mt && ct.Item2 == st) || (ct.Item2 == mt && ct.Item1 == st);
                });
                if (cc.Count() != 1) throw new ArgumentException("One, and only one ISyncComparator must be added for master side and each side");
                side.Comparator = cc.Single();
                //Entries.Single(e => e.Side == side).Comparator = cc.Single();
            }
            // Load all sides in parallel
            await Task.WhenAll(Sides.Select(s => s.Load()));
            // Compare each side with master side
            foreach (var sid in Sides.Where(s => !s.IsMaster))
            {
                if(sid.Comparator.GetItemTypes().Item1 == MasterSide.GetItemType())
                {
                    // Master is A in this comparer
                    sid.Results = sid.Comparator.CompareItems(MasterSide.Entries, sid.Entries);
                }else
                {
                    // Master is B in this comparer
                    sid.Results = sid.Comparator.CompareItems(sid.Entries, MasterSide.Entries).Select(p => p.Invert());
                }
            }
            // Group side results by item key and populate with sides results
            //var items = new List<ISyncItem>();

            var ppp = Sides
                .Where(e => !e.IsMaster)
                .SelectMany(e => e.Results.Select(r => new
                {
                    Key = r.Key,
                    MasterItemType = MasterSide.GetItemType(),
                    MasterItem = r.MasterItem,
                    MasterName = MasterSide.GetItemName(r.MasterItem),
                    SideSyncId = e.SyncId,
                    SideItemType = e.GetItemType(),
                    SideItem = r.SideItem,
                    SideItemName = e.GetItemName(r.SideItem),
                    SideName = e.Name,
                    Properties = r.ToArray()
                }))
                .GroupBy(r => r.MasterItem).ToList();

            foreach (var gro in Sides
                .Where(e => !e.IsMaster)
                .SelectMany(e => e.Results.Select(r => new
                {
                    Key = r.Key,
                    MasterItemType = MasterSide.GetItemType(),
                    MasterItem = r.MasterItem,
                    MasterName = MasterSide.GetItemName(r.MasterItem),
                    SideSyncId = e.SyncId,
                    SideItemType = e.GetItemType(),
                    SideItem = r.SideItem,
                    SideItemName = e.GetItemName(r.SideItem),
                    SideName = e.Name,
                    Properties = r.ToArray()
                }))
                .GroupBy(r => r.MasterItem))
            {
                // Create item preview
                var fir = gro.First(); // Use first element to get master info, all items in this group has the same master item
                var itemPreviewType = typeof(SyncItem<>).MakeGenericType(fir.MasterItemType);
                var itemPreview = (ISyncItem)Activator.CreateInstance(itemPreviewType, fir.MasterItem, fir.MasterName);
                var sides = new List<ISyncItemSide>();
                foreach (var i in gro)
                {
                    // Create side preview
                    var sidePreviewType = typeof(SyncItemSide<,>).MakeGenericType(i.SideItemType, i.Key.GetType());
                    var sidePreview = (ISyncItemSide)Activator.CreateInstance(sidePreviewType, i.SideSyncId, i.SideName, i.Key, i.SideItem, i.SideItemName);
                    foreach (var pro in i.Properties)
                        ((IList<ISyncProperty>)sidePreview.Properties).Add(pro);
                    // Add side to item
                    ((IList<ISyncItemSide>)itemPreview.Sides).Add(sidePreview);
                }
                // Add item to work
                Items.Add(itemPreview);
            }
            // Create preview response
            var preWork = new SyncWorkPreview(SyncId);
            var preItems = new List<SyncItemPreview>();
            foreach (var item in Items)
            {
                var preItem = new SyncItemPreview(item.SyncId);
                preItem.MasterItemExist = item.MasterItem != null;
                preItem.MasterItemName = item.MasterName;
                var preSides = new List<SyncItemSidePreview>();
                foreach(var side in item.Sides)
                {
                    var preSide = new SyncItemSidePreview(side.SyncId);
                    preSide.Key = side.Key.ToString();
                    preSide.SideItemExist = side.SideItem != null;
                    preSide.SideItemName = side.SideItemName;
                    preSide.SideName = side.Name;
                    var prePros = new List<SyncPropertyPreview>();
                    foreach(var pro in side.Properties)
                    {
                        var prePro = new SyncPropertyPreview();
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
            // Check result and suggest an action
            foreach (var item in preWork.Items)
            {
                foreach (var side in item.Sides)
                {
                    if (!item.MasterItemExist)
                        side.Action = SyncAction.Delete;
                    else if (!side.SideItemExist)
                        side.Action = SyncAction.Add;
                    else if (side.Properties.Count() > 0)
                        side.Action = SyncAction.Update;
                }
            }

            return preWork;
        }
        public async Task RunAsync(SyncWorkPreview preview)
        {
            foreach (var item in preview.Items)
            {
                var runItem = Items.Single(i => i.SyncId == item.ItemId);
                foreach (var side in item.Sides)
                {
                    var runSide = Sides.Single(e => e.SyncId == side.SideId);
                    var runItemSide = runItem.Sides.Single(s => s.SyncId == side.SideId);
                    object sideItem = null;
                    if (runSide.Comparator.GetItemTypes().Item1 == MasterSide.GetItemType())
                    {
                        // Master is A in this comparator
                        if (item.MasterItemExist)
                            sideItem = runSide.Comparator.MapAToB(runItem.MasterItem, runItemSide.SideItem);
                    }
                    else
                    {
                        // Master is B in this comparator
                        if (item.MasterItemExist)
                            sideItem = runSide.Comparator.MapBToA(runItem.MasterItem, runItemSide.SideItem);
                    }
                    switch (side.Action)
                    {
                        case SyncAction.Add:
                            runSide.Add(sideItem);
                            break;
                        case SyncAction.Delete:
                            runSide.Delete(runItemSide.SideItem);
                            break;
                        case SyncAction.Update:
                            runSide.Update(sideItem);
                            break;
                    }
                }
            }
        }
    }
    public class SyncSession
    {
        public Guid SyncId { get; } = Guid.NewGuid();
        public ICollection<SyncWork> Works { get; set; } = new List<SyncWork>();
        public async Task<SyncSessionPreview> PreviewAsync()
        {
            var res = new SyncSessionPreview(SyncId);
            //var works = new List<SyncWorkPreview>();
            //foreach (var work in Works)
            //{
            //    var r = work.PreviewAsync().Result;
            //    works.Add(r);
            //}
            //var ooo = Works.Select(w => w.PreviewAsync().Result).ToList();
            res.Works = Works.Select(w => w.PreviewAsync().Result).ToList();
            return res;
        }
        public async Task RunAsync(SyncSessionPreview preview)
        {
            foreach(var work in Works)
            {
                var workPre = preview.Works.FirstOrDefault(w => w.WorkId == work.SyncId);
                await work.RunAsync(workPre);
            }
        }
    }
}
