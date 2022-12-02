using System.Collections.Specialized;
using Fuxion.Collections.ObjectModel;

namespace Fuxion.Test.Collections.ObjectModel;

public class DeactivatableObservableCollectionTest
{
	[Fact(DisplayName = "DeactivatableObservableCollection - Add")]
	public void DeactivatableObservableCollection_Add()
	{
		var step = 0;
		var col = new DeactivatableObservableCollection<string>();
		col.CollectionChanged += (s, e) =>
		{
			switch (step)
			{
				case 0:
					Assert.True(e.Action == NotifyCollectionChangedAction.Add);
					Assert.NotNull(e.NewItems);
					Assert.Null(e.OldItems);
					Assert.True(e.NewItems?.Count == 1);
					break;
				case 1:
					Assert.True(e.Action == NotifyCollectionChangedAction.Add);
					Assert.NotNull(e.NewItems);
					Assert.Null(e.OldItems);
					Assert.True(e.NewItems?.Count == 2);
					break;
			}
			step++;
		};
		col.Add("Oscar");
		col.Add(new[]
		{
			"Oscar", "Asier"
		});
	}
	[Fact(DisplayName = "DeactivatableObservableCollection - Clear")]
	public void DeactivatableObservableCollection_Clear()
	{
		var col = new DeactivatableObservableCollection<string>
		{
			new[]
			{
				"Oscar", "Asier"
			}
		};
		col.CollectionChanged += (s, e) =>
		{
			Assert.True(e.Action == NotifyCollectionChangedAction.Reset);
			Assert.Null(e.NewItems);
			Assert.Null(e.OldItems);
		};
		col.Clear();
	}
	[Fact(DisplayName = "DeactivatableObservableCollection - Remove")]
	public void DeactivatableObservableCollection_Remove()
	{
		var step = 0;
		var col = new DeactivatableObservableCollection<string>
		{
			"Oscar"
		};
		col.CollectionChanged += (s, e) =>
		{
			switch (step)
			{
				case 0:
					Assert.True(e.Action == NotifyCollectionChangedAction.Remove);
					Assert.Null(e.NewItems);
					Assert.NotNull(e.OldItems);
					Assert.True(e.OldItems?.Count == 1);
					break;
				case 1:
					Assert.True(e.Action == NotifyCollectionChangedAction.Remove);
					Assert.Null(e.NewItems);
					Assert.NotNull(e.OldItems);
					Assert.True(e.OldItems?.Count == 2);
					break;
			}
			step++;
		};
		col.Remove("Oscar");
		col.Add(new[]
		{
			"Oscar", "Asier"
		}, false);
		col.Remove(new[]
		{
			"Oscar", "Asier"
		});
	}
	[Fact(DisplayName = "DeactivatableObservableCollection - Replace")]
	public void DeactivatableObservableCollection_Replace()
	{
		var step = 0;
		var col = new DeactivatableObservableCollection<string>
		{
			"Oscar", "Asier"
		};
		col.CollectionChanged += (s, e) =>
		{
			switch (step)
			{
				case 0:
					Assert.True(e.Action == NotifyCollectionChangedAction.Replace);
					Assert.NotNull(e.NewItems);
					Assert.NotNull(e.OldItems);
					Assert.True(e.NewItems?.Count == 1);
					Assert.True(e.OldItems?.Count == 1);
					Assert.Same("Oscar", e.OldItems?[0]);
					Assert.Same("Oscar2", e.NewItems?[0]);
					break;
				case 1:
					Assert.True(e.Action == NotifyCollectionChangedAction.Replace);
					Assert.NotNull(e.NewItems);
					Assert.NotNull(e.OldItems);
					Assert.True(e.NewItems?.Count == 2);
					Assert.True(e.OldItems?.Count == 2);
					Assert.Same("Oscar2", e.OldItems?[0]);
					Assert.Same("Oscar3", e.NewItems?[0]);
					Assert.Same("Asier", e.OldItems?[1]);
					Assert.Same("Asier3", e.NewItems?[1]);
					break;
			}
			step++;
		};
		col[0] = "Oscar2";
		col.Replace(new[]
		{
			"Oscar2", "Asier"
		}, new[]
		{
			"Oscar3", "Asier3"
		}, true);
	}
}