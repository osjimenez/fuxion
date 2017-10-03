using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Fuxion.Collections.ObjectModel
{
    public static class ObservableCollectionExtensions
    {
        public static void Sort<T>(this ObservableCollection<T> collection, Comparison<T> comparison)
        {
            // Create a list from collection
            var sortableList = new List<T>(collection);
            // Order the list using comparison
            sortableList.Sort(comparison);
            // Move the elements of the collection using the sorted list indexes
            for (int i = 0; i < sortableList.Count; i++)
                collection.Move(collection.IndexOf(sortableList[i]), i);
        }
    }
}
