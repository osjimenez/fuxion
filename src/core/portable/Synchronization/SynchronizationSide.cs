using Fuxion.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    public interface ISynchronizationSide
    {
        bool IsMaster { get; }
        string Name { get; }
        string SingularItemTypeName { get; }
        string PluralItemTypeName { get; }
    }
    internal interface ISynchronizationSideInternal : ISynchronizationSide
    {
        Guid Id { get; }
        new string Name { get; set; }
        object Source { get; set; }
        IEnumerable<SynchronizationLoadedItem> Entries { get; set; }
        ISynchronizationComparatorInternal Comparator { get; set; }
        IEnumerable<ISynchronizationComparatorResultInternal> Results { get; set; }
        string GetItemName(object item);
        Task Load();
        Task InsertAsync(object item);
        Task DeleteAsync(object item);
        Task UpdateAsync(object item);
        ICollection<ISynchronizationSideInternal> SubSides { get; set; }
        ISynchronizationSideInternal Clone();
    }
    [DebuggerDisplay("{" + nameof(Item) + "}")]
    internal class SynchronizationLoadedItem
    {
        public object Item { get; set; }
        public ICollection<ISynchronizationSideInternal> Sides { get; set; }
    }
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    public class SynchronizationSide<TSource, TItem, TKey> : ISynchronizationSideInternal
    {

        public bool IsMaster { get; set; }
        public string Name { get; set; }
        public string SingularItemTypeName { get; set; }
        public string PluralItemTypeName { get; set; }

        public TSource Source { get; set; }
        public Func<TItem, string> OnNaming { get; set; }
        public Func<TSource, ICollection<TItem>> OnLoad { get; set; }
        public Action<TSource, TItem> OnInsert { get; set; }
        public Action<TSource, TItem> OnDelete { get; set; }
        public Action<TSource, TItem> OnUpdate { get; set; }

        Guid ISynchronizationSideInternal.Id { get; } = Guid.NewGuid();
        object ISynchronizationSideInternal.Source { get { return Source; } set { Source = (TSource)value; } }
        string ISynchronizationSideInternal.GetItemName(object item) => item != null ? OnNaming((TItem)item) : null;
        Task ISynchronizationSideInternal.InsertAsync(object item)
        {
            if (item is TItem)
                return TaskManager.StartNew(() => OnInsert(Source, (TItem)item));
            else throw new ArgumentException($"'{nameof(item)}' must be of type '{typeof(TItem).Name}'", nameof(item));
        }
        Task ISynchronizationSideInternal.DeleteAsync(object item)
        {
            if (item is TItem)
                return TaskManager.StartNew(() => OnDelete(Source, (TItem)item));
            else throw new ArgumentException($"'{nameof(item)}' must be of type '{typeof(TItem).Name}'", nameof(item));
        }
        Task ISynchronizationSideInternal.UpdateAsync(object item)
        {
            if (item is TItem)
                return TaskManager.StartNew(() => OnUpdate(Source, (TItem)item));
            else throw new ArgumentException($"'{nameof(item)}' must be of type '{typeof(TItem).Name}'", nameof(item));
        }
        Task ISynchronizationSideInternal.Load() => TaskManager.StartNew(() =>
        {
            Printer.WriteLine($"Loading side '{Name}' ...");
            var me = (ISynchronizationSideInternal)this;
            var rr = new List<SynchronizationLoadedItem>();
            var res = OnLoad(Source)?.Cast<object>() ?? Enumerable.Empty<object>();
            foreach (var item in res)
            {
                rr.Add(new SynchronizationLoadedItem
                {
                    Item = item,
                    Sides = me.SubSides?.Select(side =>
                    {
                        var clon = side.Clone();
                        clon.Source = item;
                        clon.Name = $"{clon.Name} ({((ISynchronizationSideInternal)this).GetItemName(item)})";
                        clon.Load().Wait();
                        return clon;
                    }).ToList() ?? Enumerable.Empty<ISynchronizationSideInternal>().ToList()
                });
            }
            me.Entries = rr;
            Printer.WriteLine($"Side '{Name}' loaded");
            //return me.Entries = res;
        });
        IEnumerable<SynchronizationLoadedItem> ISynchronizationSideInternal.Entries { get; set; } = new List<SynchronizationLoadedItem>();
        ISynchronizationComparatorInternal ISynchronizationSideInternal.Comparator { get; set; }
        IEnumerable<ISynchronizationComparatorResultInternal> ISynchronizationSideInternal.Results { get; set; }
        ICollection<ISynchronizationSideInternal> ISynchronizationSideInternal.SubSides { get; set; }
        ISynchronizationSideInternal ISynchronizationSideInternal.Clone() { return Clone(); }
        private SynchronizationSide<TSource, TItem, TKey> Clone()
        {
            var res = new SynchronizationSide<TSource, TItem, TKey>
            {
                IsMaster = IsMaster,
                Name = Name,
                OnDelete = OnDelete,
                OnInsert = OnInsert,
                OnLoad = OnLoad,
                OnNaming = OnNaming,
                OnUpdate = OnUpdate,
                PluralItemTypeName = PluralItemTypeName,
                SingularItemTypeName = SingularItemTypeName,
                Source = Source,
            };
            var s = (ISynchronizationSideInternal)res;
            s.Comparator = ((ISynchronizationSideInternal)this).Comparator;
            s.Entries = ((ISynchronizationSideInternal)this).Entries;
            s.Results = ((ISynchronizationSideInternal)this).Results;
            s.SubSides = ((ISynchronizationSideInternal)this).SubSides;
            return res;
        }
    }
}
