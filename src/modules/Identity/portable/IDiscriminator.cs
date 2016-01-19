using System;
using System.Collections.Generic;
using Fuxion.Identity.Helpers;
using System.Linq;
namespace Fuxion.Identity
{
    public interface IDiscriminator
    {
        //IEnumerable<object> Path { get; }
        IEnumerable<object> Inclusions { get; }
        IEnumerable<object> Exclusions { get; }
        object TypeId { get; }
        string TypeName { get; }
        object Id { get; }
        string Name { get; }
    }
    public interface IDiscriminator<TId, TTypeId> : IDiscriminator
    {
        //new IEnumerable<TId> Path { get; }
        new IEnumerable<TId> Inclusions { get; }
        new IEnumerable<TId> Exclusions { get; }
        new TTypeId TypeId { get; }
        new TId Id { get; }
    }
    public static class DiscriminatorExtensions
    {
        public static bool IsValid(this IDiscriminator me)
        {
            return
                !Comparer.AreEquals(me.Id, me.Id?.GetType().GetDefaultValue())
                && !string.IsNullOrWhiteSpace(me.Name)
                && !Comparer.AreEquals(me.TypeId, me.TypeId?.GetType().GetDefaultValue())
                && !string.IsNullOrWhiteSpace(me.TypeName);
        }
    }
    class DiscriminatorEqualityComparer : IEqualityComparer<IDiscriminator>
    {
        public bool Equals(IDiscriminator x, IDiscriminator y)
        {
            return AreEquals(x, y);
        }

        public int GetHashCode(IDiscriminator obj)
        {
            if (obj == null) return 0;
            return obj.Id.GetHashCode();
        }
        static bool AreEquals(object obj1, object obj2)
        {
            // If both are NULL, return TRUE
            if (Equals(obj1, null) && Equals(obj2, null)) return true;
            // If some of them is null, return FALSE
            if (Equals(obj1, null) || Equals(obj2, null)) return false;
            // If any of them are of other type, return FALSE
            if (!(obj1 is IDiscriminator) || !(obj2 is IDiscriminator)) return false;
            var dis1 = (IDiscriminator)obj1;
            var dis2 = (IDiscriminator)obj2;
            // Use 'Equals' to compare the ids
            return Comparer.AreEquals(dis1.Id, dis2.Id) && Comparer.AreEquals(dis1.TypeId, dis2.TypeId);
        }
    }
    //public interface IDiscriminatorBase
    //{
    //    object Id { get; }
    //    string Name { get; }
    //}

    //public interface IGraphDiscriminator : IDiscriminatorBase
    //{
    //    IEnumerable<IGraphDiscriminator> Inclusions { get; }
    //    IEnumerable<IGraphDiscriminator> Exclusions { get; }
    //}
    //public interface ITreeDiscriminator : IGraphDiscriminator
    //{
    //    IGraphDiscriminator Parent { get; }
    //    IEnumerable<IGraphDiscriminator> Childs { get; }
    //}
    //public interface ILinearDiscriminator : ITreeDiscriminator
    //{
    //    IGraphDiscriminator Parent { get; }
    //    IGraphDiscriminator Child { get; }
    //}

    //class oo : ILinearDiscriminator
    //{
    //    public oo Source { get; set; }
    //    public oo Target { get; set; }

    //    IGraphDiscriminator ILinearDiscriminator.Child { get { return Target; } }
    //    IEnumerable<IGraphDiscriminator> ITreeDiscriminator.Childs { get { return ((ILinearDiscriminator)Target).Exclusions.Concat(new[] { Target }); } }
    //    IEnumerable<IGraphDiscriminator> IGraphDiscriminator.Exclusions { get { return ((ILinearDiscriminator)Source).Exclusions.Concat(new[] { Source }); } }
    //    object IDiscriminatorBase.Id { get { throw new NotImplementedException(); } }
    //    IEnumerable<IGraphDiscriminator> IGraphDiscriminator.Inclusions { get { return ((ILinearDiscriminator)Target).Exclusions.Concat(new[] { Target }); } }
    //    string IDiscriminatorBase.Name { get { throw new NotImplementedException(); } }

    //    IGraphDiscriminator ITreeDiscriminator.Parent { get { return Source; } }
    //    IGraphDiscriminator ILinearDiscriminator.Parent { get { return Source; } }
    //}
}
