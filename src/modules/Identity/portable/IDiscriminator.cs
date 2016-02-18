using System;
using System.Collections.Generic;
using Fuxion.Identity.Helpers;
using System.Linq;
using System.Reflection;
using System.Diagnostics;

namespace Fuxion.Identity
{
    public interface IDiscriminator
    {
        IEnumerable<object> Inclusions { get; }
        IEnumerable<object> Exclusions { get; }
        object TypeId { get; }
        string TypeName { get; }
        object Id { get; }
        string Name { get; }
    }
    public interface IDiscriminator<TId, TTypeId> : IDiscriminator
    {
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
    [AttributeUsage(AttributeTargets.Class)]
    public class DiscriminatorAttribute : Attribute
    {
        public DiscriminatorAttribute(string key)
        {
            Key = key;
        }
        public string Key { get; set; }
    }
    [AttributeUsage(AttributeTargets.Property)]
    public class DiscriminatedByAttribute : Attribute
    {

        public DiscriminatedByAttribute(Type type)
        {
            if (!typeof(IDiscriminator).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
                throw new Exception("The Type '" + type.Name + "' must implements '" +
                                               typeof(IDiscriminator).Name + "' interface");
            Type = type;
        }
        public Type Type { get; set; }
    }
    [Discriminator("TYPE")]
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    public class TypeDiscriminator : IDiscriminator<string, string>
    {
        private TypeDiscriminator()
        {
        }
        public static TypeDiscriminator Create<T>(params Type[] knownTypes) { return Create(typeof(T), knownTypes); }
        public static TypeDiscriminator Create(Type type, params Type[] knownTypes)
        {
            var id = type.GetSignature(true);
            var @base = type.GetTypeInfo().BaseType;
            var bases = new List<Type>();
            while (@base != typeof(object))
            {
                bases.Add(@base);
                @base = @base.GetTypeInfo().BaseType;
            }
            bases.Reverse();
            var res = new TypeDiscriminator
            {
                Id = GetIdFunction(type),
                Name = GetNameFunction(type),
                TypeId = "TYPE",
                TypeName = "TYPE",
                Inclusions = knownTypes.Where(t => t.GetTypeInfo().IsSubclassOf(type)).Select(t => GetIdFunction(t)),
                Exclusions = bases.Select(t => GetIdFunction(t)),
            };
            return res;
        }
        public static Func<Type, string> GetIdFunction { get; set; } = type => type.GetSignature(true);
        public static Func<Type, string> GetNameFunction { get; set; } = type => type.Name;
        //public static TypeDiscriminator Create(string typeFullName, params Assembly[] assemblies)
        //{
        //    //var asss = AppDomain.CurrentDomain.GetAssemblies();
        //    var assName = string.Join(".", typeFullName.Split('.').Reverse().Skip(1).Reverse());
        //    var ass = assemblies.FirstOrDefault(a => a.GetName().Name == assName);
        //    return Create(ass.GetType(typeFullName));
        //}
        public string Id { get; private set; }
        object IDiscriminator.Id { get { return Id; } }

        public string Name { get; private set; }

        public string TypeId { get; private set; }
        object IDiscriminator.TypeId { get { return TypeId; } }

        public string TypeName { get; private set; }

        public IEnumerable<string> Inclusions { get; private set; }
        IEnumerable<object> IDiscriminator.Inclusions { get { return Inclusions; } }

        public IEnumerable<string> Exclusions { get; private set; }
        IEnumerable<object> IDiscriminator.Exclusions { get { return Exclusions; } }
        
    }
}
