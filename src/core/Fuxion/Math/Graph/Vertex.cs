using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Fuxion.Math.Graph
{
	[DebuggerDisplay("{" + nameof(Value) + "}")]
	public class Vertex<T>
	{
		public Vertex()
		{
			Value = default!;
			Index = -1;
			Dependencies = new List<Vertex<T>>();
		}

		public Vertex(T value)
			: this()
		{
			Value = value;
		}

		public Vertex(IEnumerable<Vertex<T>> dependencies) : this()
		{
			Dependencies = dependencies.ToList();
		}

		public Vertex(T value, IEnumerable<Vertex<T>> dependencies)
			: this(dependencies)
		{
			this.Value = value;
		}

		internal int Index { get; set; }

		internal int LowLink { get; set; }

		public T Value { get; set; }

		public ICollection<Vertex<T>> Dependencies { get; set; }
	}
}
