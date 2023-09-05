using System.Collections.ObjectModel;

namespace Fuxion.Collections;

public class TreeSynchronizer<TItem, TKey> where TKey : struct
{
	public TreeSynchronizer(ObservableCollection<TItem> items, Func<TItem, TKey> getKeyFunc, Func<TItem, TKey?> getParentKeyFunc, Func<TItem, TKey[]> getChildrenKeys)
	{
		this.getKeyFunc = getKeyFunc;
		this.getParentKeyFunc = getParentKeyFunc;
		this.getChildrenKeys = getChildrenKeys;
		Items = items;
		Tree = new(_Tree);
		items.CollectionChanged += (_, __) => Refresh();
	}
	readonly ObservableCollection<TItem> _Tree = new();
	readonly Func<TItem, TKey[]> getChildrenKeys;
	readonly Func<TItem, TKey> getKeyFunc;
	readonly Func<TItem, TKey?> getParentKeyFunc;
	public ObservableCollection<TItem> Items { get; set; }
	public ReadOnlyObservableCollection<TItem> Tree { get; set; }
	void Refresh()
	{
		_Tree.Clear();
		var entries = Items.Select(i => new TreeSynchronizerEntry<TItem, TKey>(i, getKeyFunc(i), getParentKeyFunc(i), getChildrenKeys(i))).ToList();

		// Get root items
		foreach (var root in Items.Where(i => !Tree.Any(ii => getKeyFunc(i).Equals(getParentKeyFunc(ii))))) _Tree.Add(root);
		// Iterate root items
		foreach (var root in _Tree) { }
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