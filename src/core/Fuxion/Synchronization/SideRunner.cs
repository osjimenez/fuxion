using Fuxion.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
	internal class SideRunner<TSource, TItem> : ISideRunner
	{
		//public SideRunner(Side<TSource, TItem> definition, IPrinter printer, IComparatorRunner comparator,ICollection<LoadedItem> entries, IEnumerable<IComparatorResultInternal> results,ICollection<ISideRunner> subSides)
		//{
		//	this.printer = printer;
		//	Definition = definition;
		//	Comparator = comparator;
		//	Entries = entries;
		//	Results = results;
		//	SubSides = subSides;
		//}
		public SideRunner(Side<TSource, TItem> definition, IPrinter printer)
		{
			this.printer = printer;
			Definition = definition;
		}
		IPrinter printer;
		public Guid Id { get; } = Guid.NewGuid();
		public ISide Definition { get; set; }
		public object? Source { get { return ((Side<TSource, TItem>)Definition).Source; } set { ((Side<TSource, TItem>)Definition).Source = (TSource)value!; } }
		public ICollection<LoadedItem> Entries { get; set; } = new List<LoadedItem>();
		public IComparatorRunner? Comparator { get; set; }
		public IEnumerable<IComparatorResultInternal> Results { get; set; } = new List<IComparatorResultInternal>();
		public ICollection<ISideRunner> SubSides { get; set; } = new List<ISideRunner>();

		public string? GetItemName(object? item) => item != null ? ((Side<TSource, TItem>)Definition).OnNaming((TItem)item) : null;
		public string? GetItemTag(object? item) => (item != null && ((Side<TSource, TItem>)Definition).OnTagging != null) ? ((Side<TSource, TItem>)Definition).OnTagging((TItem)item) : null;
		public Task InsertAsync(object item)
		{
			printer.WriteLine($"Inserting '{GetItemName(item)}' in side '{Definition.Name}'");
			if (item is TItem)
				return TaskManager.StartNew(() => ((Side<TSource, TItem>)Definition).OnInsert(((Side<TSource, TItem>)Definition).Source, (TItem)item));
			else throw new ArgumentException($"'{nameof(item)}' must be of type '{typeof(TItem).Name}'", nameof(item));
		}
		public Task DeleteAsync(object item)
		{
			printer.WriteLine($"Deleting '{GetItemName(item)}' in side '{Definition.Name}'");
			if (item is TItem)
				return TaskManager.StartNew(() => ((Side<TSource, TItem>)Definition).OnDelete(((Side<TSource, TItem>)Definition).Source, (TItem)item));
			else throw new ArgumentException($"'{nameof(item)}' must be of type '{typeof(TItem).Name}'", nameof(item));
		}
		public Task UpdateAsync(object item)
		{
			printer.WriteLine($"Updating '{GetItemName(item)}' in side '{Definition.Name}'");
			if (item is TItem)
				return TaskManager.StartNew(() => ((Side<TSource, TItem>)Definition).OnUpdate(((Side<TSource, TItem>)Definition).Source, (TItem)item));
			else throw new ArgumentException($"'{nameof(item)}' must be of type '{typeof(TItem).Name}'", nameof(item));
		}
		public Task Load() => TaskManager.StartNew(() =>
		{
			using (printer.Indent($"Loading side '{Definition.Name}':"))
			{
				var rr = new List<LoadedItem>();
				var res = ((Side<TSource, TItem>)Definition).OnLoad(((Side<TSource, TItem>)Definition).Source)?.Cast<object>() ?? Enumerable.Empty<object>();
				foreach (var item in res)
				{
					var loadedItem = new LoadedItem(item, Enumerable.Empty<ISideRunner>().ToList());
					if (SubSides != null)
						foreach (var subSide in SubSides)
						{

							var clon = subSide.Clone();
							clon.Source = item;
							clon.Definition.Name = $"{clon.Definition.Name.Replace("%sourceName%", GetItemName(item))}";
							//clon.Definition.Name = $"{clon.Definition.Name} ({GetItemName(item)})";
							printer.WriteLine($"Creating side-clon '{clon.Definition.Name}-{clon.Definition.IsMaster}' with source '{GetItemName(item)}'");
							clon.Load().Wait();
							loadedItem.Sides.Add(clon);
						}
					rr.Add(loadedItem);
				}
				Entries = rr;
			}
			printer.WriteLine($"Side '{Definition.Name}' loaded");
		});
		public ISideRunner Clone() =>
			new SideRunner<TSource, TItem>(((Side<TSource, TItem>)Definition).Clone(), printer)
			{
				Comparator = Comparator,
				Entries = Entries,
				Results = Results,
				SubSides = SubSides
			};
	}
}
