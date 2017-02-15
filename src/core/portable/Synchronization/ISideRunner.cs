using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    internal interface ISideRunner : ISideDefinition
    {
        Guid Id { get; }
        new string Name { get; set; }
        object Source { get; set; }
        ICollection<LoadedItem> Entries { get; set; }
        IComparatorRunner Comparator { get; set; }
        IEnumerable<IComparatorResultInternal> Results { get; set; }
        string GetItemName(object item);
        Task Load();
        Task InsertAsync(object item);
        Task DeleteAsync(object item);
        Task UpdateAsync(object item);
        ICollection<ISideRunner> SubSides { get; set; }
        ISideRunner Clone();
    }
}
