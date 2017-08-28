using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
namespace Fuxion.Collections.ObjectModel
{
	public class DeactivatableObservableCollection<T> : ObservableCollection<T>
	{
		bool suppressNotification = false;
		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			if (!suppressNotification)
				base.OnCollectionChanged(e);
		}
		public void Add(IEnumerable<T> items) { Add(items, true); }
		public void Add(IEnumerable<T> items, bool mustBeNotified)
		{
			if (items == null) throw new ArgumentNullException("list");
			var list = items.ToList();
			if (list.Count == 0) throw new ArgumentException("La lista de elementos no puede estar vacia.");
			int preCount = Count;
			foreach (T item in list)
				Add(item, false);
			if (mustBeNotified) OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, list, preCount));
		}
		public void Add(T item, bool mustBeNotified)
		{
			suppressNotification = !mustBeNotified;
			base.Add(item);
			suppressNotification = false;
		}
		public void Clear(bool mustBeNotified)
		{
			suppressNotification = !mustBeNotified;
			base.Clear();
			suppressNotification = false;
		}
		public void Insert(int index, T item, bool mustBeNotified)
		{
			suppressNotification = !mustBeNotified;
			base.Insert(index, item);
			suppressNotification = false;
		}
		public void Move(int oldIndex, int newIndex, bool mustBeNotified)
		{
			suppressNotification = !mustBeNotified;
			base.Move(oldIndex, newIndex);
			suppressNotification = false;
		}
		
		public void RemoveAt(int index, bool mustBeNotified)
		{
			suppressNotification = !mustBeNotified;
			base.RemoveAt(index);
			suppressNotification = false;
		}
		public void Remove(IEnumerable<T> items) { Remove(items, true); }
		public void Remove(IEnumerable<T> items, bool mustBeNotified)
		{
			if (items == null) throw new ArgumentNullException("list");
			var list = items.ToList();
			if (list.Count == 0) throw new ArgumentException("La lista de elementos no puede estar vacia.");
			foreach (T item in list)
				Remove(item, false);
			if (mustBeNotified) OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, list));
		}
		public void Remove(T item, bool mustBeNotified)
		{
			suppressNotification = !mustBeNotified;
			base.Remove(item);
			suppressNotification = false;
		}
		public void Replace(IEnumerable<T> originalList, IEnumerable<T> subtituteList) { Replace(originalList, subtituteList, true); }
		public void Replace(IEnumerable<T> originalList, IEnumerable<T> subtituteList, bool mustBeNotified)
		{
			if (originalList == null || subtituteList == null)
				throw new ArgumentNullException("list");
			if (!originalList.All(o => this.Contains(o))) throw new ArgumentException("Some elements of the original list aren't in collection.");
			var oriList = originalList.ToList();
			var subList = subtituteList.ToList();
			if (oriList.Count != subList.Count) throw new ArgumentException("Original and substitute lists size cannot be differ.");
			if (oriList.Count == 0) throw new ArgumentException("Original and substitute lists cannot be empty.");
			suppressNotification = true;
			for (int i = 0; i < oriList.Count; i++)
				this[this.IndexOf(oriList[i])] = subList[i];
			suppressNotification = false;
			if (mustBeNotified) OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, subList, oriList));
		}
	}
}
