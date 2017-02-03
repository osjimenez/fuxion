using Fuxion.Threading.Tasks;
using System;
using System.Collections.Generic;
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
        IEnumerable<object> Entries { get; set; }
        ISynchronizationComparator Comparator { get; set; }
        IEnumerable<ISynchronizationComparatorResultInternal> Results { get; set; }
        string GetItemName(object item);
        Task Load();
        Task InsertAsync(object item);
        Task DeleteAsync(object item);
        Task UpdateAsync(object item);
    }
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
        Task ISynchronizationSideInternal.Load() => TaskManager.StartNew(() => ((ISynchronizationSideInternal)this).Entries = OnLoad(Source).Cast<object>());
        IEnumerable<object> ISynchronizationSideInternal.Entries { get; set; }
        ISynchronizationComparator ISynchronizationSideInternal.Comparator { get; set; }
        IEnumerable<ISynchronizationComparatorResultInternal> ISynchronizationSideInternal.Results { get; set; }
    }
}
