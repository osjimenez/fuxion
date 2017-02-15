using Fuxion.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    public class SideDefinition<TSource, TItem, TKey> : ISideRunner
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

        Guid ISideRunner.Id { get; } = Guid.NewGuid();
        object ISideRunner.Source { get { return Source; } set { Source = (TSource)value; } }
        string ISideRunner.GetItemName(object item) => item != null ? OnNaming((TItem)item) : null;
        Task ISideRunner.InsertAsync(object item)
        {
            if (item is TItem)
                return TaskManager.StartNew(() => OnInsert(Source, (TItem)item));
            else throw new ArgumentException($"'{nameof(item)}' must be of type '{typeof(TItem).Name}'", nameof(item));
        }
        Task ISideRunner.DeleteAsync(object item)
        {
            if (item is TItem)
                return TaskManager.StartNew(() => OnDelete(Source, (TItem)item));
            else throw new ArgumentException($"'{nameof(item)}' must be of type '{typeof(TItem).Name}'", nameof(item));
        }
        Task ISideRunner.UpdateAsync(object item)
        {
            if (item is TItem)
                return TaskManager.StartNew(() => OnUpdate(Source, (TItem)item));
            else throw new ArgumentException($"'{nameof(item)}' must be of type '{typeof(TItem).Name}'", nameof(item));
        }
        Task ISideRunner.Load() => TaskManager.StartNew(() =>
        {
            Printer.WriteLine($"Loading side '{Name}' ...");
            var me = (ISideRunner)this;
            var rr = new List<LoadedItem>();
            var res = OnLoad(Source)?.Cast<object>() ?? Enumerable.Empty<object>();
            foreach (var item in res)
            {
                rr.Add(new LoadedItem
                {
                    Item = item,
                    Sides = me.SubSides?.Select(side =>
                    {
                        var clon = side.Clone();
                        clon.Source = item;
                        clon.Name = $"{clon.Name} ({((ISideRunner)this).GetItemName(item)})";
                        clon.Load().Wait();
                        return clon;
                    }).ToList() ?? Enumerable.Empty<ISideRunner>().ToList()
                });
            }
            me.Entries = rr;
            Printer.WriteLine($"Side '{Name}' loaded");
            //return me.Entries = res;
        });
        ICollection<LoadedItem> ISideRunner.Entries { get; set; } = new List<LoadedItem>();
        IComparatorRunner ISideRunner.Comparator { get; set; }
        IEnumerable<IComparatorResultInternal> ISideRunner.Results { get; set; }
        ICollection<ISideRunner> ISideRunner.SubSides { get; set; }
        ISideRunner ISideRunner.Clone() { return Clone(); }
        private SideDefinition<TSource, TItem, TKey> Clone()
        {
            var res = new SideDefinition<TSource, TItem, TKey>
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
            var s = (ISideRunner)res;
            s.Comparator = ((ISideRunner)this).Comparator;
            s.Entries = ((ISideRunner)this).Entries;
            s.Results = ((ISideRunner)this).Results;
            s.SubSides = ((ISideRunner)this).SubSides;
            return res;
        }
    }
}
