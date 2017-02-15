using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    internal interface IComparatorRunner : IComparatorDefinition
    {
        ICollection<IComparatorResultInternal> CompareItems(ISideRunner sideA, ISideRunner sideB, bool runInverted = false);
        //ICollection<ISynchronizationComparatorResultInternal> CompareItems(IEnumerable<SynchronizationLoadedItem> itemsA, IEnumerable<SynchronizationLoadedItem> itemsB, bool runInverted = false);
        IComparatorResultInternal CompareItem(LoadedItem itemA, LoadedItem itemB, bool runInverted = false);
        object MapAToB(object itemA, object itemB);
        object MapBToA(object itemB, object itemA);
    }
}
