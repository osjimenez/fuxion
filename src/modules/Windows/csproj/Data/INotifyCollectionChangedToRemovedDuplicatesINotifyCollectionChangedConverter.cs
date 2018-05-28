using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Windows.Data
{
    public class INotifyCollectionChangedToRemovedDuplicatesINotifyCollectionChangedConverter : GenericConverter<INotifyCollectionChanged, INotifyCollectionChanged>
    {
        public override INotifyCollectionChanged Convert(INotifyCollectionChanged source, CultureInfo culture)
        {
            if (source == null) return null;
            if (!(source is IEnumerable<object>)) throw new NotSupportedException($"The source must be '{nameof(IEnumerable<object>)}'.");
            ObservableCollection<object> res = new ObservableCollection<object>(((IEnumerable<object>)source).Distinct());
            source.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                    foreach (var item in e.NewItems)
                        if(!res.Contains(item))
                            res.Add(item);
                if (e.OldItems != null)
                    foreach (var item in e.OldItems)
                        if (!((IEnumerable<object>)source).Contains(item))
                            res.Remove(item);
				if (e.Action == NotifyCollectionChangedAction.Reset)
					res.Clear();
            };
            return res;
        }
    }
}
