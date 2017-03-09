using Fuxion.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    internal class SideRunner<TSource, TItem> : ISideRunner
    {
        public SideRunner(Side<TSource, TItem> definition)
        {
            Definition = definition;
        }

        public Guid Id { get; } = Guid.NewGuid();
        public ISide Definition { get; set; }
        public object Source { get { return ((Side<TSource, TItem>)Definition).Source; } set { ((Side<TSource, TItem>)Definition).Source = (TSource)value; } }
        public ICollection<LoadedItem> Entries { get; set; }
        public IComparatorRunner Comparator { get; set; }
        public IEnumerable<IComparatorResultInternal> Results { get; set; }
        public ICollection<ISideRunner> SubSides { get; set; }

        public string GetItemName(object item) => item != null ? ((Side<TSource, TItem>)Definition).OnNaming((TItem)item) : null;
        public Task InsertAsync(object item)
        {
            Printer.WriteLine($"Inserting '{GetItemName(item)}' in side '{Definition.Name}'");
            if (item is TItem)
                return TaskManager.StartNew(() => ((Side<TSource, TItem>)Definition).OnInsert(((Side<TSource, TItem>)Definition).Source, (TItem)item));
            else throw new ArgumentException($"'{nameof(item)}' must be of type '{typeof(TItem).Name}'", nameof(item));
        }
        public Task DeleteAsync(object item)
        {
            Printer.WriteLine($"Deleting '{GetItemName(item)}' in side '{Definition.Name}'");
            if (item is TItem)
                return TaskManager.StartNew(() => ((Side<TSource, TItem>)Definition).OnDelete(((Side<TSource, TItem>)Definition).Source, (TItem)item));
            else throw new ArgumentException($"'{nameof(item)}' must be of type '{typeof(TItem).Name}'", nameof(item));
        }
        public Task UpdateAsync(object item)
        {
            Printer.WriteLine($"Updating '{GetItemName(item)}' in side '{Definition.Name}'");
            if (item is TItem)
                return TaskManager.StartNew(() => ((Side<TSource, TItem>)Definition).OnUpdate(((Side<TSource, TItem>)Definition).Source, (TItem)item));
            else throw new ArgumentException($"'{nameof(item)}' must be of type '{typeof(TItem).Name}'", nameof(item));
        }
        public Task Load() => TaskManager.StartNew(() =>
        {
            Printer.Indent($"Loading side '{Definition.Name}':", () =>
            {
                var rr = new List<LoadedItem>();
                var res = ((Side<TSource, TItem>)Definition).OnLoad(((Side<TSource, TItem>)Definition).Source)?.Cast<object>() ?? Enumerable.Empty<object>();
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
                            clon.Definition.Name = $"{clon.Definition.Name.Replace("%sourceName%", GetItemName(item))}";
                            //clon.Definition.Name = $"{clon.Definition.Name} ({GetItemName(item)})";
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
            new SideRunner<TSource, TItem>(((Side<TSource, TItem>)Definition).Clone())
            {
                Comparator = Comparator,
                Entries = Entries,
                Results = Results,
                SubSides = SubSides
            };
    }
}
