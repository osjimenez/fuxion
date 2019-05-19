using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Fuxion.Identity.Test.Mocks
{
	[DebuggerDisplay("{" + nameof(Name) + "}")]
	internal class GuidFunction : IFunction<Guid>
	{
		public GuidFunction(Guid id, string name) { Id = id; Name = name; }

		public Guid Id { get; private set; }
		object IFunction.Id => Id;

		public string Name { get; set; }

		public IEnumerable<IFunction<Guid>> Inclusions { get; set; } = new List<IFunction<Guid>>();
		IEnumerable<IFunction> IInclusive<IFunction>.Inclusions => Inclusions;

		public IEnumerable<IFunction<Guid>> Exclusions { get; set; } = new List<IFunction<Guid>>();
		IEnumerable<IFunction> IExclusive<IFunction>.Exclusions => Exclusions;
	}
}
