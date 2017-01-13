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
        public static ISyncPropertyPreview Invert(this ISyncPropertyPreview me)
        {
            var meType = me.GetType();
            if (meType.IsSubclassOfRawGeneric(typeof(SyncPropertyPreview<,>)))
            {
                var resType = typeof(SyncPropertyPreview<,>).MakeGenericType(meType.GetTypeInfo().GenericTypeArguments[1], meType.GetTypeInfo().GenericTypeArguments[0]);
                var res = Activator.CreateInstance(resType, me.PropertyName, me.SideValue, me.MasterValue);
                return (ISyncPropertyPreview)res;
            }
            else throw new InvalidCastException($"The property '{me.PropertyName}' cannot be inverted because is of type '{meType.Name}' and is not subclass of generic type '{typeof(SyncPropertyPreview<,>).Name}'");
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
                var addMethod = resType.GetRuntimeMethod("Add", new[] { typeof(ISyncPropertyPreview) });
                foreach(var pro in me)
                {
                    addMethod.Invoke(res, new[] { pro.Invert() });
                }
                return (ISyncComparatorResult)res;
            }
            else throw new InvalidCastException($"The item '{me}' cannot be inverted because is of type '{meType.Name}' and is not subclass of generic type '{typeof(SyncComparatorResult<,,>).Name}'");
        }
    }
    public interface ISyncSide {
        Guid SyncId { get; }
        bool IsMaster { get; }
        string Name { get; }
        string SingularItemTypeName { get; }
        string PluralItemTypeName { get; }
        IEnumerable<object> Load();
        string GetItemName(object item);
        void Add(object item);
        void Delete(object item);
        void Update(object item);
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

        IEnumerable<object> ISyncSide.Load() => Loader(Source).Cast<object>();
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
    }
    
    public interface ISyncWork
    {
        Guid SyncId { get; }
        IEnumerable<ISyncWork> SubWorks { get; }
        Task<SyncWorkPreview> PreviewAsync(SyncSessionPreview session);
        Task RunAsync(SyncWorkPreview preview);
    }
    public class SyncWork : ISyncWork
    {
        public Guid SyncId { get; } = Guid.NewGuid();
        public IEnumerable<ISyncSide> Sides { get; set; }
        public IEnumerable<ISyncComparator> Comparators { get; set; }
        public IEnumerable<ISyncWork> SubWorks { get; set; }

        class PreviewEntry
        {
            public IEnumerable<object> Items { get; set; }
            public ISyncSide Side { get; set; }
            public ISyncComparator Comparator { get; set; }

            public Task Load()
            {
                return TaskManager.StartNew(() => Items = Side.Load());
            }
        }
        ISyncSide masterSide;
        List<PreviewEntry> entries;
        public async Task<SyncWorkPreview> PreviewAsync(SyncSessionPreview session)
        {
            // Get master side
            var masters = Sides.Where(s => s.IsMaster);
            if (masters.Count() != 1) throw new ArgumentException("One, and only one SyncSide must be the master side");
            masterSide = masters.Single();
            // Create entries from sides
            entries = Sides.Select(s => new PreviewEntry { Side = s }).ToList();
            var masterEntry = entries.Single(e => e.Side.IsMaster);
            // Search for comparators for any side with master side
            foreach (var side in Sides.Where(s => !s.IsMaster))
            {
                var cc = Comparators.Where(c =>
                {
                    var mt = masterSide.GetItemType();
                    var st = side.GetItemType();
                    var ct = c.GetItemTypes();
                    return (ct.Item1 == mt && ct.Item2 == st) || (ct.Item2 == mt && ct.Item1 == st);
                });
                if (cc.Count() != 1) throw new ArgumentException("One, and only one ISyncComparator must be added for master side and each side");
                entries.Single(e => e.Side == side).Comparator = cc.Single();
            }
            // Load all sides in parallel
            await Task.WhenAll(entries.Select(e => e.Load()));
            // Compare each side with master side
            var items = new List<Tuple<PreviewEntry, ISyncComparatorResult>>();
            foreach (var ent in entries.Where(e => !e.Side.IsMaster))
            {
                
                if(ent.Comparator.GetItemTypes().Item1 == masterSide.GetItemType())
                {
                    // Master is A in this comparer
                    items.AddRange(ent.Comparator.CompareItems(masterEntry.Items, ent.Items).Select(i => new Tuple<PreviewEntry, ISyncComparatorResult>(ent, i)));
                }else
                {
                    // Master is B in this comparer
                    items.AddRange(ent.Comparator.CompareItems(ent.Items, masterEntry.Items).Select(p => p.Invert()).Select(i => new Tuple<PreviewEntry, ISyncComparatorResult>(ent, i)));
                }
            }
            // Group side results by item key
            var gros = items.Select(i => new
            {
                Key = i.Item2.Key,
                MasterItemType = masterSide.GetItemType(),
                MasterItem = i.Item2.MasterItem,
                MasterName = masterSide.GetItemName(i.Item2.MasterItem),
                SideSyncId = i.Item1.Side.SyncId,
                SideItemType = i.Item1.Side.GetItemType(),
                SideItem = i.Item2.SideItem,
                SideItemName = i.Item1.Side.GetItemName(i.Item2.SideItem),
                SideName = i.Item1.Side.Name,
                Properties = i.Item2.ToArray()
            }).GroupBy(r => r.Key);
            var res = new SyncWorkPreview(SyncId);
            foreach(var gro in gros)
            {
                // Create item preview
                var fir = gro.First(); // Use first element to get master info, all items in this group has the same master item
                var itemPreviewType = typeof(SyncItemPreview<,>).MakeGenericType(fir.MasterItemType, fir.Key.GetType());
                var itemPreview = (ISyncItemPreview)Activator.CreateInstance(itemPreviewType, fir.Key, fir.MasterItem, fir.MasterName);
                var sides = new List<ISyncSidePreview>();
                foreach (var i in gro)
                {
                    // Create side preview
                    var sidePreviewType = typeof(SyncSidePreview<>).MakeGenericType(i.SideItemType);
                    var sidePreview = (ISyncSidePreview)Activator.CreateInstance(sidePreviewType, i.SideSyncId, i.SideName, i.SideItem, i.SideItemName);
                    foreach (var pro in i.Properties)
                        ((IList<ISyncPropertyPreview>)sidePreview).Add(pro);
                    // Add side to item
                    ((IList<ISyncSidePreview>)itemPreview).Add(sidePreview);
                }
                // Add item to work
                res.Add(itemPreview);
            }
            // Check result and suggest an action
            foreach(var item in res)
            {
                foreach(var side in item)
                {
                    if (item.MasterItem == null && side.SideItem != null)
                        side.Action = SyncAction.Delete;
                    else if (side.SideItem == null)
                        side.Action = SyncAction.Add;
                    else if (side.Count() > 0)
                        side.Action = SyncAction.Update;
                }
            }

            return res;
        }
        public async Task RunAsync(SyncWorkPreview preview)
        {
            foreach (var item in preview)
            {
                foreach (var side in item)
                {
                    var ent = entries.FirstOrDefault(e => e.Side.SyncId == side.SyncId);
                    object sideItem = null;
                    if (ent.Comparator.GetItemTypes().Item1 == masterSide.GetItemType())
                    {
                        // Master is A in this comparator
                        if (item.MasterItem != null)
                            sideItem = ent.Comparator.MapAToB(item.MasterItem, side.SideItem);
                    }
                    else
                    {
                        // Master is B in this comparator
                        if (item.MasterItem != null)
                            sideItem = ent.Comparator.MapBToA(item.MasterItem, side.SideItem);
                    }
                    switch (side.Action)
                    {
                        case SyncAction.Add:
                            ent.Side.Add(sideItem);
                            break;
                        case SyncAction.Delete:
                            ent.Side.Delete(side.SideItem);
                            break;
                        case SyncAction.Update:
                            ent.Side.Update(sideItem);
                            break;
                    }
                }
            }
        }
    }
    public class SyncSession
    {
        public Guid SyncId { get; } = Guid.NewGuid();
        public ICollection<ISyncWork> Works { get; set; } = new List<ISyncWork>();
        public async Task<SyncSessionPreview> PreviewAsync()
        {
            var res = new SyncSessionPreview(SyncId);
            foreach (var work in Works)
                res.Add(await work.PreviewAsync(res));
            return res;
        }
        public async Task RunAsync(SyncSessionPreview preview)
        {
            foreach(var work in Works)
            {
                var workPre = preview.FirstOrDefault(w => w.SyncId == work.SyncId);
                await work.RunAsync(workPre);
            }
        }
    }
}
