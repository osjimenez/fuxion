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
        object IDiscriminator.Id { get { return Id; } }

        public string Name { get; internal set; }

        public string TypeId { get; internal set; }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        object IDiscriminator.TypeId { get { return TypeId; } }

        public string TypeName { get; internal set; }

        public List<TypeDiscriminator> Inclusions { get; internal set; }
        public List<TypeDiscriminator> Exclusions { get; internal set; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IEnumerable<IDiscriminator> IInclusive<IDiscriminator>.Inclusions { get { return Inclusions; } }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IEnumerable<IDiscriminator> IExclusive<IDiscriminator>.Exclusions { get { return Exclusions; } }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IEnumerable<IDiscriminator<string, string>> IInclusive<IDiscriminator<string, string>>.Inclusions { get { return Inclusions; } }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IEnumerable<IDiscriminator<string, string>> IExclusive<IDiscriminator<string, string>>.Exclusions { get { return Exclusions; } }

        public static TypeDiscriminator Empty => new TypeDiscriminator
        {
            TypeId = TypeDiscriminatorId,
            TypeName = TypeDiscriminatorId
        };

        public override string ToString() { return this.ToOneLineString(); }

        public override bool Equals(object obj)
        {
            if (!(obj is TypeDiscriminator)) return false;
            var c = obj as TypeDiscriminator;
            return c.Id == Id && c.TypeId == TypeId;
        }
        public override int GetHashCode() { return TypeId.GetHashCode() ^ Id.GetHashCode(); }
        public static bool operator ==(TypeDiscriminator a, TypeDiscriminator b) { return EqualityComparer<TypeDiscriminator>.Default.Equals(a, b); }
        public static bool operator !=(TypeDiscriminator a, TypeDiscriminator b) { return !EqualityComparer<TypeDiscriminator>.Default.Equals(a, b); }
    }
}
