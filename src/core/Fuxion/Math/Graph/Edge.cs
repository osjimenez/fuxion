using System.Diagnostics;

namespace Fuxion.Math.Graph;

[DebuggerDisplay($"{{{nameof(Source)}}} -> {{{nameof(Target)}}}")]
public class Edge<T>
{
	public Edge(T source, T target)
	{
		Source = source;
		Target = target;
	}
	public T Source { get; private set; }
	public T Target { get; private set; }
}