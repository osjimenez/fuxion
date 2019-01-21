using System.Collections.Generic;
using System.Diagnostics;
namespace Fuxion.Identity
{
	[Discriminator(TypeDiscriminatorId)]
	[DebuggerDisplay("{" + nameof(Name) + "}")]
	public sealed class TypeDiscriminator : IDiscriminator<string, string>
	{
		internal TypeDiscriminator() { }
		internal const string TypeDiscriminatorId = "TYPE";

		public string Id { get; internal set; }
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		object IDiscriminator.Id => Id;

		public string Name { get; internal set; }

		public string TypeId { get; internal set; }
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		object IDiscriminator.TypeId => TypeId;

		public string TypeName { get; internal set; }

		public List<TypeDiscriminator> Inclusions { get; internal set; }
		public List<TypeDiscriminator> Exclusions { get; internal set; }

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		IEnumerable<IDiscriminator> IInclusive<IDiscriminator>.Inclusions => Inclusions;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		IEnumerable<IDiscriminator> IExclusive<IDiscriminator>.Exclusions => Exclusions;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		IEnumerable<IDiscriminator<string, string>> IInclusive<IDiscriminator<string, string>>.Inclusions => Inclusions;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		IEnumerable<IDiscriminator<string, string>> IExclusive<IDiscriminator<string, string>>.Exclusions => Exclusions;

		public static TypeDiscriminator Empty => new TypeDiscriminator
		{
			TypeId = TypeDiscriminatorId,
			TypeName = TypeDiscriminatorId
		};

		public override string ToString() => this.ToOneLineString();

		public override bool Equals(object obj)
		{
			if (!(obj is TypeDiscriminator))
				return false;
			var c = obj as TypeDiscriminator;
			return c.Id == Id && c.TypeId == TypeId;
		}
		public override int GetHashCode() => TypeId.GetHashCode() ^ Id.GetHashCode();
		public static bool operator ==(TypeDiscriminator a, TypeDiscriminator b) => EqualityComparer<TypeDiscriminator>.Default.Equals(a, b);
		public static bool operator !=(TypeDiscriminator a, TypeDiscriminator b) => !EqualityComparer<TypeDiscriminator>.Default.Equals(a, b);
	}
}
