using System.Collections.Generic;
namespace Fuxion.Synchronization
{
    internal interface IComparatorRunner
    {
        ICollection<IComparatorResultInternal> CompareSides(ISideRunner sideA, ISideRunner sideB, bool runInverted, IPrinter printer);
        //IComparatorResultInternal CompareItems(LoadedItem itemA, LoadedItem itemB, bool runInverted = false);
        object MapAToB(object itemA, object itemB);
        object MapBToA(object itemB, object itemA);
    }
}
