using System.Diagnostics;

namespace Fuxion.Math.Graph;

[DebuggerDisplay("{" + nameof(Value) + "}")]
public class Vertex<T>
{
	public Vertex()
	{
		Value        = default!;
		Index        = -1;
		Dependencies = new List<Vertex<T>>();
	}
	public Vertex(T                      value) : this() => Value = value;
	public Vertex(IEnumerable<Vertex<T>> dependencies) : this() => Dependencies = dependencies.ToList();
	public Vertex(T                      value, IEnumerable<Vertex<T>> dependencies) : this(dependencies) => Value = value;
	internal int                    Index        { get; set; }
	internal int                    LowLink      { get; set; }
	public   T                      Value        { get; set; }
	public   ICollection<Vertex<T>> Dependencies { get; set; }
}