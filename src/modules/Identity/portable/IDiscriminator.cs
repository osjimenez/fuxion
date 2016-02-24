using System;
using System.Collections.Generic;
using Fuxion.Identity.Helpers;
using System.Linq;
using System.Reflection;
using System.Diagnostics;

namespace Fuxion.Identity
{
    public interface IDiscriminator : IInclusive<IDiscriminator>, IExclusive<IDiscriminator>
    {
        //IEnumerable<object> Inclusions { get; }
        //IEnumerable<object> Exclusions { get; }
        object TypeId { get; }
        string TypeName { get; }
        object Id { get; }
        string Name { get; }
    }
    public interface IDiscriminator<TId, TTypeId> : IDiscriminator, IInclusive<IDiscriminator<TId, TTypeId>>, IExclusive<IDiscriminator<TId, TTypeId>>
    {
        //new IEnumerable<TId> Inclusions { get; }
        //new IEnumerable<TId> Exclusions { get; }
        new TTypeId TypeId { get; }
        new TId Id { get; }
    }
    public static class DiscriminatorExtensions
    {
        public static string ToOneLineString(this IDiscriminator me)
        {
            return $"{me.TypeId} - {me.Id}";
        }
        public static bool IsValid(this IDiscriminator me)
        {
            return
                !Comparer.AreEquals(me.Id, me.Id?.GetType().GetDefaultValue())
                && !string.IsNullOrWhiteSpace(me.Name)
                && !Comparer.AreEquals(me.TypeId, me.TypeId?.GetType().GetDefaultValue())
                && !string.IsNullOrWhiteSpace(me.TypeName);
        }
        public static void Print(this IEnumerable<IDiscriminator> me, PrintMode mode)
        {
            switch (mode)
            {
                case PrintMode.OneLine:
                    break;
                case PrintMode.PropertyList:
                    break;
                case PrintMode.Table:
                    var typeId = me.Select(s => s.TypeId.ToString().Length).Union(new[] { "TYPE_ID".Length }).Max();
                    var typeName = me.Select(s => s.TypeName.Length).Union(new[] { "TYPE_NAME".Length }).Max();
                    var id = me.Select(s => s.Id.ToString().Length).Union(new[] { "ID".Length }).Max();
                    var name = me.Select(s => s.Name.Length).Union(new[] { "ID".Length }).Max();
                    Printer.Print("┌" + ("".PadRight(typeId, '─')) + "┬" + ("".PadRight(typeName, '─')) + "┬" + ("".PadRight(id, '─')) + "┬" + ("".PadRight(name, '─')) + "┐");
                    Printer.Print("│" + "TYPE_ID".PadRight(typeId, ' ') + "│" + "TYPE_NAME".PadRight(typeName, ' ') + "│" + "ID".PadRight(id, ' ') + "│" + "NAME".PadRight(name, ' ') + "│");
                    Printer.Print("├" + ("".PadRight(typeId, '─')) + "┼" + ("".PadRight(typeName, '─')) + "┼" + ("".PadRight(id, '─')) + "┼" + ("".PadRight(name, '─')) + "┤");
                    foreach (var sco in me) Printer.Print("│" + sco.TypeId.ToString().PadRight(typeId, ' ') + "│" + sco.TypeName.PadRight(typeName, ' ') + "│" + sco.Id.ToString().PadRight(id, ' ') + "│" + sco.Name.PadRight(name, ' ') + "│");
                    Printer.Print("└" + ("".PadRight(typeId, '─')) + "┴" + ("".PadRight(typeName, '─')) + "┴" + ("".PadRight(id, '─')) + "┴" + ("".PadRight(name, '─')) + "┘");
                    break;
            }
        }
    }
    public class DiscriminatorEqualityComparer : IEqualityComparer<IDiscriminator>
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
    //[AttributeUsage(AttributeTargets.Class)]
    //public class DiscriminatorAttribute : Attribute
    //{
    //    public DiscriminatorAttribute(string key)
    //    {
    //        Key = key;
    //    }
    //    public string Key { get; set; }
    //}
    [AttributeUsage(AttributeTargets.Class)]
    public class DiscriminatorAttribute : Attribute
    {
        public DiscriminatorAttribute(object typeId)
        {
            TypeId = typeId;
        }
        public object TypeId { get; set; }
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
        public static string DiscriminatorTypeId { get; set; } = "TYPE";
        public static string DiscriminatorTypeName { get; set; } = "TYPE";
        public static Type[] KnownTypes { get; set; }
        public static TypeDiscriminator Create<T>() { return Create(typeof(T)); }
        public static TypeDiscriminator Create(Type type)
        {
            if (KnownTypes == null) throw new ArgumentException($"You must set '{nameof(TypeDiscriminator)}.{nameof(KnownTypes)}' before create any discriminator");
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
                TypeId = DiscriminatorTypeId,
                TypeName = DiscriminatorTypeName,
                Inclusions = KnownTypes.Where(t => t.GetTypeInfo().IsSubclassOf(type)).Select(t => Create(t)),
                Exclusions = bases.Select(t => Create(t)),
            };
            return res;
        }
        //public static TypeDiscriminator Create(string typeFullName, params Assembly[] assemblies)
        //{
        //    //var asss = AppDomain.CurrentDomain.GetAssemblies();
        //    var assName = string.Join(".", typeFullName.Split('.').Reverse().Skip(1).Reverse());
        //    var ass = assemblies.FirstOrDefault(a => a.GetName().Name == assName);
        //    return Create(ass.GetType(typeFullName));
        //}
        public static Func<Type, string> GetIdFunction { get; set; } = type => type.GetSignature(true);
        public static Func<Type, string> GetNameFunction { get; set; } = type => type.Name;

        public string Id { get; private set; }
        object IDiscriminator.Id { get { return Id; } }

        public string Name { get; private set; }

        public string TypeId { get; private set; }
        object IDiscriminator.TypeId { get { return TypeId; } }

        public string TypeName { get; private set; }

        public IEnumerable<TypeDiscriminator> Inclusions { get; private set; }
        public IEnumerable<TypeDiscriminator> Exclusions { get; private set; }

        IEnumerable<IDiscriminator> IInclusive<IDiscriminator>.Inclusions { get { return Inclusions; } }

        IEnumerable<IDiscriminator> IExclusive<IDiscriminator>.Exclusions { get { return Exclusions; } }

        IEnumerable<IDiscriminator<string, string>> IInclusive<IDiscriminator<string, string>>.Inclusions { get { return Inclusions; } }

        IEnumerable<IDiscriminator<string, string>> IExclusive<IDiscriminator<string, string>>.Exclusions { get { return Exclusions; } }
        public override string ToString() { return this.ToOneLineString(); }
    }
}
