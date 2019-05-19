using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Fuxion.Identity.Test.Mocks
{
	[DebuggerDisplay("{" + nameof(Name) + "}")]
	internal class GuidDiscriminator : IDiscriminator<Guid, Guid>
	{
		public GuidDiscriminator(Guid id, string name, Guid typeId, string typeName)
		{
			Id = id;
			Name = name;
			TypeKey = typeId;
			TypeName = typeName;
		}
		public Guid Id { get; private set; }
		object? IDiscriminator.Id => Id;

		public string? Name { get; private set; }

		public Guid TypeKey { get; private set; }
		object IDiscriminator.TypeKey => TypeKey;

		public string TypeName { get; private set; }

		public IEnumerable<GuidDiscriminator> Inclusions { get; set; } = new List<GuidDiscriminator>();
		IEnumerable<IDiscriminator> IInclusive<IDiscriminator>.Inclusions => Inclusions;
		IEnumerable<IDiscriminator<Guid, Guid>> IInclusive<IDiscriminator<Guid, Guid>>.Inclusions => Inclusions;

		public IEnumerable<GuidDiscriminator> Exclusions { get; set; } = new List<GuidDiscriminator>();
		IEnumerable<IDiscriminator> IExclusive<IDiscriminator>.Exclusions => Exclusions;
		IEnumerable<IDiscriminator<Guid, Guid>> IExclusive<IDiscriminator<Guid, Guid>>.Exclusions => Exclusions;
	}
}
