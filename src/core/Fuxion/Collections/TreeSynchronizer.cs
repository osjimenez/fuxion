namespace Fuxion.Collections;

using System.Collections.ObjectModel;

public class TreeSynchronizer<TItem, TKey> where TKey : struct
{
	public TreeSynchronizer(ObservableCollection<TItem> items, Func<TItem, TKey> getKeyFunc, Func<TItem, TKey?> getParentKeyFunc, Func<TItem, TKey[]> getChildrenKeys)
	{
		this.getKeyFunc = getKeyFunc;
		this.getParentKeyFunc = getParentKeyFunc;
		this.getChildrenKeys = getChildrenKeys;
		Items = items;
		Tree = new ReadOnlyObservableCollection<TItem>(_Tree);
		items.CollectionChanged += (_, __) => Refresh();
	}

	private readonly Func<TItem, TKey> getKeyFunc;
	private readonly Func<TItem, TKey?> getParentKeyFunc;
	private readonly Func<TItem, TKey[]> getChildrenKeys;

	public ObservableCollection<TItem> Items { get; set; }

	private readonly ObservableCollection<TItem> _Tree = new ObservableCollection<TItem>();
	public ReadOnlyObservableCollection<TItem> Tree { get; set; }

	private void Refresh()
	{
		_Tree.Clear();

		var entries = Items.Select(i => new TreeSynchronizerEntry<TItem, TKey>(i, getKeyFunc(i), getParentKeyFunc(i), getChildrenKeys(i))).ToList();

		// Get root items
		foreach (var root in Items.Where(i => !Tree.Any(ii => getKeyFunc(i).Equals(getParentKeyFunc(ii)))))
		{
			_Tree.Add(root);
		}
		// Iterate root items
		foreach (var root in _Tree)
		{

		}
	}
}
public class TreeSynchronizerEntry<TItem, TKey> where TKey : struct
{
	public TreeSynchronizerEntry(TItem item, TKey key, TKey? parentKey, TKey[] childrenKeys)
	{
		Item = item;
		Key = key;
		ParentKey = parentKey;
		ChildrenKeys = childrenKeys;
	}
	public TItem Item { get; }
	public TKey Key { get; }
	public TKey? ParentKey { get; }
	public TKey[] ChildrenKeys { get; }
}