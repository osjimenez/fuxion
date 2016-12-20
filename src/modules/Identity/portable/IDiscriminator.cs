﻿using System;
using System.Collections.Generic;
using Fuxion.Identity.Helpers;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using Fuxion.Logging;

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
    [AttributeUsage(AttributeTargets.Class,AllowMultiple = false, Inherited = false)]
    public class TypeDiscriminatedAttribute :Attribute
    {
        public TypeDiscriminatedAttribute(string id) { Id = id; Name = id; }
        public string Id { get; set; }
        public string Name { get; set; }
    }
    [Discriminator("TYPE")]
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    public class TypeDiscriminator : IDiscriminator<string, string>
    {
        internal TypeDiscriminator() { }

        public string Id { get; internal set; }
        object IDiscriminator.Id { get { return Id; } }

        public string Name { get; internal set; }

        public string TypeId { get; internal set; }
        object IDiscriminator.TypeId { get { return TypeId; } }

        public string TypeName { get; internal set; }

        public IEnumerable<TypeDiscriminator> Inclusions { get; internal set; }
        public IEnumerable<TypeDiscriminator> Exclusions { get; internal set; }

        IEnumerable<IDiscriminator> IInclusive<IDiscriminator>.Inclusions { get { return Inclusions; } }

        IEnumerable<IDiscriminator> IExclusive<IDiscriminator>.Exclusions { get { return Exclusions; } }

        IEnumerable<IDiscriminator<string, string>> IInclusive<IDiscriminator<string, string>>.Inclusions { get { return Inclusions; } }

        IEnumerable<IDiscriminator<string, string>> IExclusive<IDiscriminator<string, string>>.Exclusions { get { return Exclusions; } }
        public override string ToString() { return this.ToOneLineString(); }


        //public override int GetHashCode() { return Id.GetHashCode(); }
        //public override bool Equals(object obj) { return obj is TypeDiscriminator && Compare(this, obj as TypeDiscriminator); }
        //private static bool Compare(TypeDiscriminator item1, TypeDiscriminator item2)
        //{
        //    if (ReferenceEquals(item1, null) && ReferenceEquals(item2, null)) return true;
        //    else if (ReferenceEquals(item1, null) && !ReferenceEquals(item2, null)) return false;
        //    else if (!ReferenceEquals(item1, null) && ReferenceEquals(item2, null)) return false;
        //    else return item1.Id == item2.Id;
        //}
        //public static bool operator ==(TypeDiscriminator item1, TypeDiscriminator item2) { return Compare(item1, item2); }
        //public static bool operator !=(TypeDiscriminator item1, TypeDiscriminator item2) { return !Compare(item1, item2); }
    }

    public class TypeDiscriminatorFactory
    {
        ILog log = LogManager.Create<TypeDiscriminatorFactory>();
        public string DiscriminatorTypeId { get; set; } = "TYPE";
        public string DiscriminatorTypeName { get; set; } = "TYPE";
        [DebuggerDisplay("{" + nameof(Discriminator) + "}")]
        class Entry
        {
            public Type Type { get; set; }
            public TypeDiscriminator Discriminator { get; set; }
        }
        List<Entry> entries = new List<Entry>();
        public IEnumerable<TypeDiscriminator> GetAll() { return entries.Select(e => e.Discriminator); }
        public void RegisterTree<T>(params Type[] types) => RegisterTree(typeof(T), types);
        public void RegisterTree(Type baseType, params Type[] types) {
            foreach (var type in types)
            {
                var nn = type.Name;
                if (baseType == type ||
                    (baseType.GetTypeInfo().IsGenericTypeDefinition
                    ? type.IsSubclassOfRawGeneric(baseType)
                    : type.GetTypeInfo().IsSubclassOf(baseType)))
                {
                    Register(type);
                }
            }
            //Register(baseType);
        }
        public void Register<T>() => Register(typeof(T));
        public void Register(params Type[] types)
        {
            foreach (var type in types)
            {
                // Calculo el id para este tipo
                var id = GetIdFunction(type);
                var aux = entries.FirstOrDefault(e => e.Discriminator.Id == id);
                if (aux != null)
                {
                    var ex = new Exception($"El tipo '{type.FullName}' no se puede registrar porque el id '{id}' ya se ha registrado para el tipo '{aux.Type.FullName}'");
                    log.Error(ex.Message, ex);
                    throw ex;
                }

                // Creo la entrada con el tipo
                var ent = new Entry { Type = type };

                var inclusions = entries
                    .Where(e =>
                        type != e.Type &&
                        (type.GetTypeInfo().IsGenericTypeDefinition
                        ? e.Type.IsSubclassOfRawGeneric(type)
                        : e.Type.GetTypeInfo().IsSubclassOf(type)))
                    .Select(e => e.Discriminator);
                var exclusions = entries
                    .Where(e =>
                        type != e.Type &&
                        (e.Type.GetTypeInfo().IsGenericTypeDefinition
                        ? type.IsSubclassOfRawGeneric(e.Type)
                        : type.GetTypeInfo().IsSubclassOf(e.Type)))
                    .Select(e => e.Discriminator);

                ent.Discriminator = new TypeDiscriminator
                {
                    Id = GetIdFunction(type),
                    Name = GetNameFunction(type),
                    TypeId = DiscriminatorTypeId,
                    TypeName = DiscriminatorTypeName,
                    Inclusions = inclusions,
                    Exclusions = exclusions
                };
                entries.Add(ent);
                log.Info($"El tipo '{type.FullName}' se ha registrado para ser discriminado con el id '{ent.Discriminator.Id}'");
            }
        }
        public void ClearAllRegisters() => entries.Clear();
        public TypeDiscriminator FromType<T>() => FromType(typeof(T));
        public TypeDiscriminator FromType(Type type)
        {
            var ent = entries.FirstOrDefault(e => e.Type == type);
            if (ent == null) throw new KeyNotFoundException($"The type '{type.FullName}' was not registered. Use '{nameof(TypeDiscriminatorFactory)}.{nameof(Register)}' to register it.");
            return ent.Discriminator;
        }
        public TypeDiscriminator FromId(string id)
        {
            return entries.FirstOrDefault(e => e.Discriminator.Id == id)?.Discriminator;
        }
        public Func<Type, string> GetIdFunction { get; set; } = type =>
        {
            var att = type.GetTypeInfo().GetCustomAttribute<TypeDiscriminatedAttribute>(false, false, true);
            if (att != null) return att.Id;
            return type.GetSignature(true);
        };
        public Func<Type, string> GetNameFunction { get; set; } = type =>
        {
            var att = type.GetTypeInfo().GetCustomAttribute<TypeDiscriminatedAttribute>(false, false, true);
            if (att != null) return att.Name;
            return type.Name;
        };
    }
}
