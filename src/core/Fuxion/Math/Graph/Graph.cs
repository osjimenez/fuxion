using System.Collections;
using System.Diagnostics;

namespace Fuxion.Math.Graph;

public class Graph<T> where T : class
{
	readonly List<Edge<T>> Edges = new();
	bool _allowCycles;
	public bool AllowCycles
	{
		get => _allowCycles;
		set
		{
			if (_allowCycles != value)
			{
				_allowCycles = value;
				if (!AllowCycles && HasCycles()) throw new GraphCyclicException();
			}
		}
	}
	public void Remove(T value)
	{
		foreach (var edge in Edges.ToList())
			if (edge.Source == value || edge.Target == value)
				Edges.Remove(edge);
	}
	public void AddEdge(T source, T target)
	{
		Edges.Add(new(source, target));
		if (!AllowCycles && HasCycles()) throw new GraphCyclicException($"Source={source}   Target={target}");
	}
	public void AddEdges(params (T source, T target)[] edges)
	{
		foreach (var edge in edges) Edges.Add(new(edge.source, edge.target));
		//if (!AllowCycles && HasCycles()) throw new GraphCyclicException();
	}
	public bool HasCycles()
	{
		var graph = Edges.Select(e => e.Source).Concat(Edges.Select(e => e.Target)).Distinct().Select(v => new Vertex<T>(v)).ToList();
		foreach (var vertex in graph)
		{
			var dep = Edges.Where(e => e.Source == vertex.Value).Select(e => e.Target);
			vertex.Dependencies = graph.Where(v => dep.Contains(v.Value)).ToList();
		}
		var detector = new StronglyConnectedComponentFinder<T>();
		var components = detector.DetectCycle(graph);
		return components.Cycles().Any();
	}
	public IEnumerable<T> GetDescendants(T vertex, IEqualityComparer<T>? comparer = null)
	{
		if (comparer == null) comparer = EqualityComparer<T>.Default;
		Debug.WriteLine("DESCENDANTS of " + vertex);
		var res = new List<T>();
		var currentLevel = Edges.Where(e => comparer!.Equals(e.Source, vertex)).Select(e => e.Target).ToList();
		Debug.WriteLine("First level: " + currentLevel.Aggregate("", (c, n) => c.ToString() + " -> " + n));
		while (currentLevel.Any())
		{
			res.AddRange(currentLevel);
			var nextLevel = Edges.Where(e => currentLevel.Contains(e.Source, comparer)).Select(e => e.Target).ToList();
			Debug.WriteLine("Next level: " + nextLevel.Aggregate("", (c, n) => c.ToString() + " -> " + n));
			currentLevel.Clear();
			currentLevel.AddRange(nextLevel);
		}
		res = res.Distinct(comparer).ToList();
		Debug.WriteLine("RESULT: " + res.Aggregate("", (c, n) => c.ToString() + " -> " + n));
		return res;
	}
	public IEnumerable<T> GetAscendants(T vertex, IEqualityComparer<T>? comparer = null)
	{
		if (comparer == null) comparer = EqualityComparer<T>.Default;
		Debug.WriteLine("ASCENDANTS of " + vertex);
		var res = new List<T>();
		var currentLevel = Edges.Where(e => comparer!.Equals(e.Target, vertex)).Select(e => e.Source).ToList();
		Debug.WriteLine("First level: " + currentLevel.Aggregate("", (c, n) => c.ToString() + " -> " + n));
		while (currentLevel.Any())
		{
			res.AddRange(currentLevel);
			var nextLevel = Edges.Where(e => currentLevel.Contains(e.Target, comparer)).Select(e => e.Source).ToList();
			Debug.WriteLine("Next level: " + nextLevel.Aggregate("", (c, n) => c.ToString() + " -> " + n));
			currentLevel.Clear();
			currentLevel.AddRange(nextLevel);
		}
		res = res.Distinct(comparer).ToList();
		Debug.WriteLine("RESULT: " + res.Aggregate("", (c, n) => c.ToString() + " -> " + n));
		return res;
	}
}

public class StronglyConnectedComponent<T> : IEnumerable<Vertex<T>>
{
	public StronglyConnectedComponent() => list = new();
	public StronglyConnectedComponent(IEnumerable<Vertex<T>> collection) => list = new(collection);
	readonly LinkedList<Vertex<T>> list;
	public int Count => list.Count;
	public bool IsCycle => list.Count > 1;
	public IEnumerator<Vertex<T>> GetEnumerator() => list.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator();
	public void Add(Vertex<T> vertex) => list.AddLast(vertex);
}

/// <summary>
///    Implementation of the Tarjan stronly connected components algorithm.
/// </summary>
/// <seealso cref="http://en.wikipedia.org/wiki/Tarjan's_strongly_connected_components_algorithm" />
/// <seealso cref="http://stackoverflow.com/questions/261573/best-algorithm-for-detecting-cycles-in-a-directed-graph" />
public class StronglyConnectedComponentFinder<T>
{
	int index;
	Stack<Vertex<T>>? stack;
	StronglyConnectedComponentList<T>? stronglyConnectedComponents;
	/// <summary>
	///    Calculates the sets of strongly connected vertices.
	/// </summary>
	/// <param name="graph">Graph to detect cycles within.</param>
	/// <returns>Set of strongly connected components (sets of vertices)</returns>
	public StronglyConnectedComponentList<T> DetectCycle(IEnumerable<Vertex<T>> graph)
	{
		stronglyConnectedComponents = new();
		index = 0;
		stack = new();
		foreach (var v in graph)
			if (v.Index < 0)
				StrongConnect(v);
		return stronglyConnectedComponents;
	}
	void StrongConnect(Vertex<T> v)
	{
		v.Index = index;
		v.LowLink = index;
		index++;
		stack?.Push(v);
		foreach (var w in v.Dependencies)
			if (w.Index < 0)
			{
				StrongConnect(w);
				v.LowLink = System.Math.Min(v.LowLink, w.LowLink);
			} else if (stack?.Contains(w) ?? false) v.LowLink = System.Math.Min(v.LowLink, w.Index);
		if (v.LowLink == v.Index)
		{
			var scc = new StronglyConnectedComponent<T>();
			Vertex<T>? w;
			do
			{
				w = stack?.Pop()!;
				scc.Add(w);
			} while (v != w);
			stronglyConnectedComponents?.Add(scc);
		}
	}
}

public class StronglyConnectedComponentList<T> : IEnumerable<StronglyConnectedComponent<T>>
{
	public StronglyConnectedComponentList() => collection = new();
	public StronglyConnectedComponentList(IEnumerable<StronglyConnectedComponent<T>> collection) => this.collection = new(collection);
	readonly LinkedList<StronglyConnectedComponent<T>> collection;
	public int Count => collection.Count;
	public IEnumerator<StronglyConnectedComponent<T>> GetEnumerator() => collection.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => collection.GetEnumerator();
	public void Add(StronglyConnectedComponent<T> scc) => collection.AddLast(scc);
	public IEnumerable<StronglyConnectedComponent<T>> IndependentComponents() => this.Where(c => !c.IsCycle);
	public IEnumerable<StronglyConnectedComponent<T>> Cycles() => this.Where(c => c.IsCycle);
}