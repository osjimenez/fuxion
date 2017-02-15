using Fuxion.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    internal class SideRunner<TSource, TItem, TKey> : ISideRunner
    {
        public SideRunner(SideDefinition<TSource, TItem, TKey> definition)
        {
            Definition = definition;
        }

        public Guid Id { get; } = Guid.NewGuid();
        public ISideDefinition Definition { get; set; }
        public object Source { get { return ((SideDefinition<TSource, TItem, TKey>)Definition).Source; } set { ((SideDefinition<TSource, TItem, TKey>)Definition).Source = (TSource)value; } }
        public ICollection<LoadedItem> Entries { get; set; }
        public IComparatorRunner Comparator { get; set; }
        public IEnumerable<IComparatorResultInternal> Results { get; set; }
        public ICollection<ISideRunner> SubSides { get; set; }

        public string GetItemName(object item) => item != null ? ((SideDefinition<TSource, TItem, TKey>)Definition).OnNaming((TItem)item) : null;
        public Task InsertAsync(object item)
        {
            Printer.WriteLine($"Inserting '{GetItemName(item)}' in side '{Definition.Name}'");
            if (item is TItem)
                return TaskManager.StartNew(() => ((SideDefinition<TSource, TItem, TKey>)Definition).OnInsert(((SideDefinition<TSource, TItem, TKey>)Definition).Source, (TItem)item));
            else throw new ArgumentException($"'{nameof(item)}' must be of type '{typeof(TItem).Name}'", nameof(item));
        }
        public Task DeleteAsync(object item)
        {
            Printer.WriteLine($"Deleting '{GetItemName(item)}' in side '{Definition.Name}'");
            if (item is TItem)
                return TaskManager.StartNew(() => ((SideDefinition<TSource, TItem, TKey>)Definition).OnDelete(((SideDefinition<TSource, TItem, TKey>)Definition).Source, (TItem)item));
            else throw new ArgumentException($"'{nameof(item)}' must be of type '{typeof(TItem).Name}'", nameof(item));
        }
        public Task UpdateAsync(object item)
        {
            Printer.WriteLine($"Updating '{GetItemName(item)}' in side '{Definition.Name}'");
            if (item is TItem)
                return TaskManager.StartNew(() => ((SideDefinition<TSource, TItem, TKey>)Definition).OnUpdate(((SideDefinition<TSource, TItem, TKey>)Definition).Source, (TItem)item));
            else throw new ArgumentException($"'{nameof(item)}' must be of type '{typeof(TItem).Name}'", nameof(item));
        }
        public Task Load() => TaskManager.StartNew(() =>
        {
            Printer.Indent($"Loading side '{Definition.Name}':", () =>
            {
                var rr = new List<LoadedItem>();
                var res = ((SideDefinition<TSource, TItem, TKey>)Definition).OnLoad(((SideDefinition<TSource, TItem, TKey>)Definition).Source)?.Cast<object>() ?? Enumerable.Empty<object>();
                foreach (var item in res)
                {
                    var loadedItem = new LoadedItem
                    {
                        Item = item,
                        Sides = Enumerable.Empty<ISideRunner>().ToList()
                    };
                    if (SubSides != null)
                        foreach (var subSide in SubSides)
                        {
                            
                            var clon = subSide.Clone();
                            clon.Source = item;
                            clon.Definition.Name = $"{clon.Definition.Name} ({GetItemName(item)})";
                            Printer.WriteLine($"Creating side-clon '{clon.Definition.Name}-{clon.Definition.IsMaster}' with source '{GetItemName(item)}'");
                            clon.Load().Wait();
                            loadedItem.Sides.Add(clon);
                        }
                    rr.Add(loadedItem);
                }
                Entries = rr;
            });
            Printer.WriteLine($"Side '{Definition.Name}' loaded");
        });
        public ISideRunner Clone() =>
            new SideRunner<TSource, TItem, TKey>(((SideDefinition<TSource, TItem, TKey>)Definition).Clone())
            {
                Comparator = Comparator,
                Entries = Entries,
                Results = Results,
                SubSides = SubSides
            };
    }
}
