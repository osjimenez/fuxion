using System;
using System.Collections.Generic;
using Fuxion.Identity.Helpers;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using Fuxion.Logging;
using Fuxion.Reflection;
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
    public class Discriminator : IDiscriminator
    {
        private Discriminator() { }
        public object TypeId { get; private set; }

        public string TypeName { get; private set; }

        public object Id => "<--- EMPTY --->";

        public string Name => "EMPTY";

        public IEnumerable<IDiscriminator> Inclusions => throw new NotImplementedException();

        public IEnumerable<IDiscriminator> Exclusions => throw new NotImplementedException();

        public static IDiscriminator Empty<TDiscriminator>()
        {
            var att = typeof(TDiscriminator).GetTypeInfo().GetCustomAttribute<DiscriminatorAttribute>();
            if (att != null)
                return new Discriminator
                {
                    TypeId = att.TypeId,
                    TypeName = typeof(TDiscriminator).Name,
                };
            throw new ArgumentException($"The type '{typeof(TDiscriminator).Name}' isn't adorned with Discriminator attribute");
        }
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
                    Printer.WriteLine("┌" + ("".PadRight(typeId, '─')) + "┬" + ("".PadRight(typeName, '─')) + "┬" + ("".PadRight(id, '─')) + "┬" + ("".PadRight(name, '─')) + "┐");
                    Printer.WriteLine("│" + "TYPE_ID".PadRight(typeId, ' ') + "│" + "TYPE_NAME".PadRight(typeName, ' ') + "│" + "ID".PadRight(id, ' ') + "│" + "NAME".PadRight(name, ' ') + "│");
                    Printer.WriteLine("├" + ("".PadRight(typeId, '─')) + "┼" + ("".PadRight(typeName, '─')) + "┼" + ("".PadRight(id, '─')) + "┼" + ("".PadRight(name, '─')) + "┤");
                    foreach (var sco in me) Printer.WriteLine("│" + sco.TypeId.ToString().PadRight(typeId, ' ') + "│" + sco.TypeName.PadRight(typeName, ' ') + "│" + sco.Id.ToString().PadRight(id, ' ') + "│" + sco.Name.PadRight(name, ' ') + "│");
                    Printer.WriteLine("└" + ("".PadRight(typeId, '─')) + "┴" + ("".PadRight(typeName, '─')) + "┴" + ("".PadRight(id, '─')) + "┴" + ("".PadRight(name, '─')) + "┘");
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
                throw new Exception($"The Type '{type.Name}' must implements '{nameof(IDiscriminator)}' interface");
            Type = type;
        }
        public Type Type { get; set; }
    }
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class TypeDiscriminatedAttribute :Attribute
    {
        public TypeDiscriminatedAttribute(string id) { Id = id; Name = id; }
        public TypeDiscriminatedAttribute(bool enabled) { Enabled = enabled; }
        public string Id { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; } = true;
        public string[] Inclusions { get; set; }
        public string[] Exclusions { get; set; }
    }
    [Discriminator("TYPE")]
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    public class TypeDiscriminator : IDiscriminator<string, string>
    {
        internal TypeDiscriminator() { }
        //internal TypeDiscriminator(Func<IEnumerable<TypeDiscriminator>> getInclusions, Func<IEnumerable<TypeDiscriminator>> getExclusions)
        //{
        //    this.getInclusions = getInclusions;
        //    this.getExclusions = getExclusions;
        //}

        //Func<IEnumerable<TypeDiscriminator>> getInclusions;
        //Func<IEnumerable<TypeDiscriminator>> getExclusions;

        public string Id { get; internal set; }
        object IDiscriminator.Id { get { return Id; } }

        public string Name { get; internal set; }

        public string TypeId { get; internal set; }
        object IDiscriminator.TypeId { get { return TypeId; } }

        public string TypeName { get; internal set; }

        //public IEnumerable<TypeDiscriminator> Inclusions { get { return getInclusions?.Invoke(); } }//; internal set; }
        //public IEnumerable<TypeDiscriminator> Exclusions { get { return getExclusions?.Invoke(); } }//; internal set; }

        public IList<TypeDiscriminator> Inclusions { get; internal set; }
        public IList<TypeDiscriminator> Exclusions { get; internal set; }

        IEnumerable<IDiscriminator> IInclusive<IDiscriminator>.Inclusions { get { return Inclusions; } }

        IEnumerable<IDiscriminator> IExclusive<IDiscriminator>.Exclusions { get { return Exclusions; } }

        IEnumerable<IDiscriminator<string, string>> IInclusive<IDiscriminator<string, string>>.Inclusions { get { return Inclusions; } }

        IEnumerable<IDiscriminator<string, string>> IExclusive<IDiscriminator<string, string>>.Exclusions { get { return Exclusions; } }

        public override string ToString() { return this.ToOneLineString(); }

        public override bool Equals(object obj)
        {
            if (!(obj is TypeDiscriminator)) return false;
            var c = obj as TypeDiscriminator;
            return c.Id == Id && c.TypeId == TypeId;
            //var inc = c.Inclusions.Select(i => (i.TypeId, i.Id)).ToList();
            //var exc = c.Exclusions.Select(i => (i.TypeId, i.Id)).ToList();
            //return c.Id == Id && c.TypeId == TypeId && c.Inclusions.SequenceEqual(Inclusions) && c.Exclusions.SequenceEqual(Exclusions);
        }
        public override int GetHashCode() { return TypeId.GetHashCode() ^ Id.GetHashCode(); }
        public static bool operator ==(TypeDiscriminator a, TypeDiscriminator b) { return EqualityComparer<TypeDiscriminator>.Default.Equals(a, b); }
        public static bool operator !=(TypeDiscriminator a, TypeDiscriminator b) { return !EqualityComparer<TypeDiscriminator>.Default.Equals(a, b); }
    }
    public class TypeDiscriminatorFactory
    {
        ILog log = LogManager.Create<TypeDiscriminatorFactory>();
        public string DiscriminatorTypeId { get; set; } = "TYPE";
        public string DiscriminatorTypeName { get; set; } = "TYPE";
        public bool AllowMoreThanOneTypeByDiscriminator { get; set; }
        public bool AllowVirtualTypeDiscriminators { get; set; }
        [DebuggerDisplay("{" + nameof(Discriminator) + "}")]
        class Entry
        {
            public List<TypeEntry> Typess { get; set; }
            public TypeDiscriminator Discriminator { get; set; }

            public List<Entry> AllInclusions { get; set; } = new List<Entry>();
            public List<Entry> AllExclusions { get; set; } = new List<Entry>();

            public bool IsVirtual => Typess.IsNullOrEmpty();
            public bool HasAttribute => Typess?.Any(t => t.Attribute != null) ?? false;
            public bool DefineInclusionsExplicitly => Typess?.Any(t => t.DefineInclusionsExplicitly) ?? false;
            public bool DefineExclusionsExplicitly => Typess?.Any(t => t.DefineExclusionsExplicitly) ?? false;
        }
        class TypeEntry
        {
            public Type Type { get; set; }
            public TypeDiscriminatedAttribute Attribute { get; set; }
            public bool DefineInclusionsExplicitly => !Attribute?.Inclusions.IsNullOrEmpty() ?? false;
            public bool DefineExclusionsExplicitly => !Attribute?.Exclusions.IsNullOrEmpty() ?? false;
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
        }
        public void Register<T>() => Register(typeof(T));
        public void Register(params Type[] types)
        {
            if (initialized) throw new InvalidOperationException("Cannot do registers after obtain discriminators, please, do all registers before use");
            types = types.RemoveNulls();
            foreach (var type in types)
            {
                var att = type.GetTypeInfo().GetCustomAttribute<TypeDiscriminatedAttribute>(false, false, true);
                if (!att?.Enabled ?? false) continue;
                var ent = new Entry
                {
                    Typess = new[] 
                    {
                        new TypeEntry
                        {
                            Type = type,
                            Attribute = att
                        }
                    }.ToList(),
                    Discriminator = new TypeDiscriminator
                    {
                        Id = GetIdFunction(type),
                        Name = GetNameFunction(type),
                        TypeId = DiscriminatorTypeId,
                        TypeName = DiscriminatorTypeName
                    }
                };
                var existent = entries.FirstOrDefault(e => e.Discriminator.Id == ent.Discriminator.Id);
                if (existent != null)
                {
                    // La entrada ya existe
                    if (AllowMoreThanOneTypeByDiscriminator)
                    {
                        existent.Typess = existent.Typess.Concat(ent.Typess).ToList();
                    }
                    else throw new Exception($"Type '{type.FullName}' cannot be registered because the id '{ent.Discriminator.Id}' already registered for '{existent.Discriminator.Id}'");
                }
                else entries.Add(ent);
            }
            return;


            //foreach (var type in types.Where(t => t.GetTypeInfo().GetCustomAttribute<TypeDiscriminatedAttribute>(false, false)?.Enabled ?? true))
            //{
            //    if (type == null) throw new ArgumentException("Cannot register type 'null'");
            //    // Compruebo si lo tengo que ignorar
            //    if (type.GetTypeInfo().GetCustomAttribute<TypeDiscriminatedAttribute>() == null)
            //    {
            //        var parent = type.GetTypeInfo().BaseType;
            //        while (parent != null)
            //        {
            //            var at = parent.GetTypeInfo().GetCustomAttribute<TypeDiscriminatedAttribute>();
            //            if (at != null && !at.Enabled) return;
            //            parent = parent.GetTypeInfo().BaseType;
            //        }
            //    }

            //    // Calculo el id para este tipo
            //    var id = GetIdFunction(type);
            //    var aux = entries.FirstOrDefault(e => e.Discriminator.Id == id);
            //    if (!AllowMoreThanOneTypeByDiscriminator && aux != null)
            //    {
            //        var ex = new Exception($"El tipo '{type.FullName}' no se puede registrar porque el id '{id}' ya se ha registrado para el tipo '{aux.Type.FullName}'");
            //        log.Error(ex.Message, ex);
            //        throw ex;
            //    }

            //    // Creo la entrada con el tipo
            //    var ent = new Entry { Type = type };
            //    //var getInclusions = new Func<IEnumerable<TypeDiscriminator>>(() =>
            //    //{
            //    //    var at = type.GetTypeInfo().GetCustomAttribute<TypeDiscriminatedAttribute>(false, false, true);
            //    //    if(at != null && !at.Inclusions.IsNullOrEmpty())
            //    //    {
            //    //        List<TypeDiscriminator> r = new List<TypeDiscriminator>();
            //    //        foreach (var incc in at.Inclusions)
            //    //        {
            //    //            var oo = entries.FirstOrDefault(e => e.Discriminator.Id == incc)?.Discriminator;
            //    //            if (oo == null)
            //    //            {
            //    //                if (AllowVirtualTypeDiscriminators)
            //    //                {
            //    //                    oo = GetVirtualDiscriminator(incc);
            //    //                }
            //    //                else throw new ArgumentException($"Discriminator with id '{incc}' was not found to populate inclusions of type '{type.Name}'. You can set AllowVirtualTypeDiscriminators property to avoid this exception.");

            //    //            }
            //    //            r.Add(oo);
            //    //        }
            //    //        return r;
            //    //    }
            //    //    var inc = entries
            //    //        .Where(e =>
            //    //            e.Type != null &&
            //    //            type != e.Type &&
            //    //            (type.GetTypeInfo().IsGenericTypeDefinition
            //    //                ? e.Type.IsSubclassOfRawGeneric(type)
            //    //                : e.Type.GetTypeInfo().IsSubclassOf(type)));
            //    //    // Me quedo solo con los que no estan incluidos como inclusion en otros discriminadores
            //    //    // O lo que es lo mismo, no quiero todo el arbol, solo las inclusiones de primer nivel, 
            //    //    // para acceder a todas las inclusiones deberé recorrer el arbol
            //    //    var res = inc
            //    //        .Where(i => !inc.Any(ii => ii.Discriminator.Inclusions.Contains(i.Discriminator)))
            //    //        .Select(i => i.Discriminator);
            //    //    return res;
            //    //});
            //    //var getExclusions = new Func<IEnumerable<TypeDiscriminator>>(() =>
            //    //{
            //    //    var at = type.GetTypeInfo().GetCustomAttribute<TypeDiscriminatedAttribute>(false, false, true);
            //    //    if (at != null && !at.Exclusions.IsNullOrEmpty())
            //    //    {
            //    //        List<TypeDiscriminator> r = new List<TypeDiscriminator>();
            //    //        foreach (var incc in at.Exclusions)
            //    //        {
            //    //            var oo = entries.FirstOrDefault(e => e.Discriminator.Id == incc)?.Discriminator;
            //    //            if (oo == null)
            //    //            {
            //    //                if (AllowVirtualTypeDiscriminators)
            //    //                {
            //    //                    oo = GetVirtualDiscriminator(incc);
            //    //                }
            //    //                else throw new ArgumentException($"Discriminator with id '{incc}' was not found to populate exclusions of type '{type.Name}'. You can set AllowVirtualTypeDiscriminators property to avoid this exception.");

            //    //            }
            //    //            r.Add(oo);
            //    //        }
            //    //        return r;
            //    //    }
            //    //    var exc = entries
            //    //        .Where(e =>
            //    //            e.Type != null &&
            //    //            type != e.Type &&
            //    //            (e.Type.GetTypeInfo().IsGenericTypeDefinition
            //    //            ? type.IsSubclassOfRawGeneric(e.Type)
            //    //            : type.GetTypeInfo().IsSubclassOf(e.Type)));
            //    //    var res = exc
            //    //        .Where(e => !exc.Any(ee => ee.Discriminator.Exclusions.Contains(e.Discriminator)))
            //    //        .Select(e => e.Discriminator);
            //    //    return res;
            //    //});

            //    ent.Discriminator = new TypeDiscriminator(
            //        () => GetInclusionsOrExclusions(type, true),
            //        () => GetInclusionsOrExclusions(type, false))
            //    {
            //        Id = GetIdFunction(type),
            //        Name = GetNameFunction(type),
            //        TypeId = DiscriminatorTypeId,
            //        TypeName = DiscriminatorTypeName,
            //    };
            //    if (aux != null)
            //    {
            //        if (aux.Discriminator != ent.Discriminator)
            //        {
            //            var ex = new Exception($"El tipo '{type.FullName}' no se puede registrar porque el id '{id}' ya se ha registrado para el tipo '{aux.Type.FullName}' y el discriminador resultante no es equivalente.");
            //            log.Error(ex.Message, ex);
            //            throw ex;
            //        }
            //    }
            //    else entries.Add(ent);
            //    log.Info($"El tipo '{type.FullName}' se ha registrado para ser discriminado con el id '{ent.Discriminator.Id}'");
            //}
        }
        //private IEnumerable<TypeDiscriminator> GetInclusionsOrExclusions(Type type, bool inclusions)
        //{
        //    var at = type.GetTypeInfo().GetCustomAttribute<TypeDiscriminatedAttribute>(false, false, true);
        //    if (at != null && !(inclusions ? at.Inclusions : at.Exclusions).IsNullOrEmpty())
        //    {
        //        List<TypeDiscriminator> r = new List<TypeDiscriminator>();
        //        foreach (var incc in (inclusions ? at.Inclusions : at.Exclusions))
        //        {
        //            var oo = entries.FirstOrDefault(e => e.Discriminator.Id == incc)?.Discriminator;
        //            if (oo == null)
        //            {
        //                if (AllowVirtualTypeDiscriminators)
        //                {
        //                    oo = GetOrCreateVirtualDiscriminator(incc);
        //                }
        //                else throw new ArgumentException($"Discriminator with id '{incc}' was not found to populate inclusions of type '{type.Name}'. You can set AllowVirtualTypeDiscriminators property to avoid this exception.");

        //            }
        //            r.Add(oo);
        //        }
        //        return r;
        //    }
        //    var ll = entries
        //        .Where(ent =>
        //        {

        //            return false;
        //        });

        //    //var list = entries
        //    //    .Where(e => e.Discriminator.Exclusions.Contains(ent.Discriminator))
        //    //    .ToList();

        //    var inc = entries
        //        .Where(e =>
        //            e.Type != null &&
        //            type != e.Type &&
        //            (type.GetTypeInfo().IsGenericTypeDefinition
        //                ? e.Type.IsSubclassOfRawGeneric(type)
        //                : e.Type.GetTypeInfo().IsSubclassOf(type)));
        //    // Me quedo solo con los que no estan incluidos como inclusion en otros discriminadores
        //    // O lo que es lo mismo, no quiero todo el arbol, solo las inclusiones de primer nivel, 
        //    // para acceder a todas las inclusiones deberé recorrer el arbol
        //    var res = inc
        //        .Where(i => !inc.Any(ii => ii.Discriminator.Inclusions.Contains(i.Discriminator)))
        //        .Select(i => i.Discriminator);
        //    return res;
        //}
        bool initialized = false;
        private void Initialize()
        {
            // Recorro todas las entradas marcadas con el atributo TypeDiscriminated que han especificado el parámetro Inclusions o Exclusions
            // En estas entradas estan todos los posibles tipos virtuales
            foreach (var ent in entries
                .Where(e => e.HasAttribute) // Algun tipo contiene el atributo
                .Where(e=>e.DefineInclusionsExplicitly || e.DefineExclusionsExplicitly)
                //.Where(e => !e.Attribute.Inclusions.IsNullOrEmpty() || !e.Attribute.Exclusions.IsNullOrEmpty())
                .ToList())
            {
                // Recorro las inclusiones y exclusiones cuyo id no esta en la lista de entradas
                foreach (var id in (
                        ent.Typess
                            .Where(t => t.Attribute?.Inclusions != null)
                            .SelectMany(t => t.Attribute.Inclusions)
                            .Concat(ent.Typess
                                .Where(t => t.Attribute?.Exclusions != null)
                                .SelectMany(t => t.Attribute.Exclusions)))
                    .Where(id => !entries.Any(e => e.Discriminator.Id == id)))
                {
                    // Compruebo si existe ya la entrada
                    if (!entries.Any(e => e.Discriminator.Id == id))
                    {
                        // Creo el tipo virtual para cada una de ellas
                        entries.Add(new Entry
                        {
                            Discriminator = new TypeDiscriminator
                            {
                                Id = id,
                                Name = id,
                                TypeId = DiscriminatorTypeId,
                                TypeName = DiscriminatorTypeName,
                            }
                        });
                    }
                }
            }

            // Ahora tengo todos las entradas definidas, incluidas las de los tipos virtuales
            // Recorro todas las entradas para calcular las inclusiones y exclusiones
            foreach (var ent in entries)
            {
                // INCLUSIONES
                if (ent.DefineInclusionsExplicitly)
                {
                    // Recorro todas las inclusiones
                    foreach (var inc in ent.Typess.SelectMany(t => t.Attribute.Inclusions.Select(i => new
                    {
                        Inclusion = i,
                        Type = t.Type
                    })))
                    {
                        var incent = entries.FirstOrDefault(e => e.Discriminator.Id == inc.Inclusion);
                        if (incent == null) throw new ArgumentException($"The inclusion discriminator id '{inc.Inclusion}', defined explicitly in type '{inc.Type.Name}' was not found");
                        ent.AllInclusions.Add(incent);
                    }
                }
                else if (!ent.IsVirtual)
                {
                    // Se calcularan las inclusiones por herencia
                    ent.AllInclusions = entries
                        .Where(e =>
                            !e.IsVirtual && // Quito las entradas de tipos virtuales
                            ent.Discriminator.Id != e.Discriminator.Id && // Me excluyo a mi mismo
                            ent.Typess.Any(t => t.Type.GetTypeInfo().IsGenericTypeDefinition // Compruebo si el tipo es genérico
                                ? e.Typess.Any(ty => ty.Type.IsSubclassOfRawGeneric(t.Type)) // Compruebo si es derivado del genérico
                                : e.Typess.Any(ty => ty.Type.GetTypeInfo().IsSubclassOf(t.Type)))
                        )
                        //(ent.Type.GetTypeInfo().IsGenericTypeDefinition // Compruebo si el tipo es genérico
                        //    ? e.Type.IsSubclassOfRawGeneric(ent.Type) // Compruebo si es derivado del genérico
                        //    : e.Type.GetTypeInfo().IsSubclassOf(ent.Type))) // Compruebo si es derivado del tipo
                        .ToList();
                }
                // EXCLUSIONES
                if (ent.DefineExclusionsExplicitly)
                {
                    // Se han definido explicitamente las exclusiones
                    foreach (var exc in ent.Typess.SelectMany(t => t.Attribute.Exclusions.Select(i => new
                    {
                        Exclusion = i,
                        Type = t.Type
                    })))
                    {
                        var excent = entries.FirstOrDefault(e => e.Discriminator.Id == exc.Exclusion);
                        if (excent == null) throw new ArgumentException($"The exclusion discriminator id '{exc}', defined explicitly in type '{exc.Type.Name}' was not found");
                        ent.AllExclusions.Add(excent);
                    }
                }
                else if(!ent.IsVirtual)
                {
                    // Se calcularan las exclusiones por herencia
                    //ent.AllExclusions = entries
                    //    .Where(e =>
                    //        e.Type != null &&
                    //        ent.Type != e.Type &&
                    //        (e.Type.GetTypeInfo().IsGenericTypeDefinition
                    //            ? ent.Type.IsSubclassOfRawGeneric(e.Type)
                    //            : ent.Type.GetTypeInfo().IsSubclassOf(e.Type)))
                    //    .ToList();
                    ent.AllExclusions = entries
                        .Where(e =>
                            !e.IsVirtual && // Quito las entradas de tipos virtuales
                            ent.Discriminator.Id != e.Discriminator.Id && // Me excluyo a mi mismo
                            e.Typess.Any(t => t.Type.GetTypeInfo().IsGenericTypeDefinition // Compruebo si el tipo es genérico
                                ? ent.Typess.Any(ty => ty.Type.IsSubclassOfRawGeneric(t.Type)) // Compruebo si es derivado del genérico
                                : ent.Typess.Any(ty => ty.Type.GetTypeInfo().IsSubclassOf(t.Type)))
                        )
                        //(ent.Type.GetTypeInfo().IsGenericTypeDefinition // Compruebo si el tipo es genérico
                        //    ? e.Type.IsSubclassOfRawGeneric(ent.Type) // Compruebo si es derivado del genérico
                        //    : e.Type.GetTypeInfo().IsSubclassOf(ent.Type))) // Compruebo si es derivado del tipo
                        .ToList();
                }
            }

            // Calculo las inclusiones y exclusiones de los tipos virtuales
            foreach (var ent in entries.Where(e => e.IsVirtual))
            {
                // En funcion de las inclusiones y exclusiones del resto de entradas
                ent.AllInclusions = entries
                    .Where(e => e.AllExclusions.Select(ee => ee.Discriminator.Id).Contains(ent.Discriminator.Id))
                    .ToList();
                ent.AllExclusions = entries
                    .Where(e => e.AllInclusions.Select(ee => ee.Discriminator.Id).Contains(ent.Discriminator.Id))
                    .ToList();
                

                //// Agrego este tipo virtual como inclusion o exclusion a mis inclusiones y exclusiones
                //foreach (var inc in ent.AllInclusions.Where(i => !i.AllExclusions.Any(e => e.Discriminator.Id == ent.Discriminator.Id)))
                //{
                //    inc.AllExclusions.Add(ent);
                //}
                //foreach (var inc in ent.AllExclusions.Where(e => !e.AllInclusions.Any(i => i.Discriminator.Id == ent.Discriminator.Id)))
                //{
                //    inc.AllInclusions.Add(ent);
                //}
            }
            foreach(var ent in entries)
            {
                // Agrego todo el arbol de inlcusiones y exclusiones agrgando las de los hijos
                ent.AllInclusions = ent.AllInclusions
                    .Concat(ent.AllInclusions.SelectMany(i => i.AllInclusions))
                    .Distinct()
                    .ToList();

                ent.AllExclusions = ent.AllExclusions
                    .Concat(ent.AllExclusions.SelectMany(i => i.AllExclusions))
                    .Distinct()
                    .ToList();
            }

            // Ahora tengo que eliminar las inclusiones y exclusiones que sobran para quedarme solo con las de primer nivel
            // Esto es, para conocer todas las inclusiones de un tipo deberé recorrer el arbol de inclusiones entero
            foreach(var ent in entries)
            {
                //var res = exc
                //    .Where(e => !exc.Any(ee => ee.Discriminator.Exclusions.Contains(e.Discriminator)))
                //    .Select(e => e.Discriminator);

                ent.Discriminator.Inclusions = ent.AllInclusions
                    .Except(ent.AllInclusions.SelectMany(e => e.AllInclusions))
                    .Select(e => e.Discriminator)
                    .ToList();
                ent.Discriminator.Exclusions = ent.AllExclusions
                    .Except(ent.AllExclusions.SelectMany(e => e.AllExclusions))
                    .Select(e => e.Discriminator)
                    .ToList();
            }


            //foreach (var ent in entries.ToList())
            //{
            //    var att = ent.Type.GetTypeInfo().GetCustomAttribute<TypeDiscriminatedAttribute>(false, false, true);
            //    if (att != null && !att.Inclusions.IsNullOrEmpty())
            //    {
            //        foreach (var incc in att.Inclusions)
            //        {
            //            var oo = entries.FirstOrDefault(e => e.Discriminator.Id == incc)?.Discriminator;
            //            if (oo == null)
            //            {
            //                if (AllowVirtualTypeDiscriminators)
            //                {
            //                    oo = GetVirtualDiscriminator(incc);
            //                }
            //            }
            //        }
            //    }
            //}
            initialized = true;
        }
        //private TypeDiscriminator GetOrCreateVirtualDiscriminator(string discriminatorId)
        //{
        //    var ent = entries.FirstOrDefault(e => e.Id == discriminatorId);
        //    if (ent != null) return ent.Discriminator;
        //    var inclusions = entries
        //        .Where(e => e.Discriminator.Exclusions.Any(ex => ex.Id == discriminatorId))
        //        .Select(e => e.Discriminator);
        //    var res = new Entry
        //    {
        //        Id = discriminatorId,
        //        //IsVirtual = true,
        //        Discriminator = new TypeDiscriminator(
        //            // Inclusions
        //            () => entries
        //                .Where(e => e.Discriminator.Exclusions.Any(ex => ex.Id == discriminatorId))
        //                .Select(e => e.Discriminator),
        //            // Exclusions
        //            () => entries
        //                .Where(e => e.Discriminator.Inclusions.Any(ex => ex.Id == discriminatorId))
        //                .Select(e => e.Discriminator))
        //        {
        //            Id = discriminatorId,
        //            Name = discriminatorId,
        //            TypeId = DiscriminatorTypeId,
        //            TypeName = DiscriminatorTypeName
        //        }
        //    };
        //    entries.Add(res);
        //    return res.Discriminator;
        //}
        public void ClearAllRegisters()
        {
            entries.Clear();
            initialized = false;
        }
        public TypeDiscriminator FromType<T>() => FromType(typeof(T));
        public TypeDiscriminator FromType(Type type) => FromId(GetIdFunction(type));
        public TypeDiscriminator FromId(string id)
        {
            var res = AllFromId(id);
            if (res.Count() > 1) throw new InvalidStateException($"More than one discriminator for the id '{id}'. Use '{nameof(AllFromId)}' method instead");
            return res.FirstOrDefault();
        }
        public IEnumerable<TypeDiscriminator> AllFromId(string id)
        {
            if (!initialized) Initialize();
            return entries.Where(e => e.Discriminator.Id == id).Select(e => e.Discriminator);
        }
        
        
        public Func<Type, string> GetIdFunction { get; set; } = type =>
        {
            var att = type.GetTypeInfo().GetCustomAttribute<TypeDiscriminatedAttribute>(false, false, true);
            if (att != null && !string.IsNullOrWhiteSpace(att.Id)) return att.Id;
            return type.GetSignature(true);
        };
        public Func<Type, string> GetNameFunction { get; set; } = type =>
        {
            var att = type.GetTypeInfo().GetCustomAttribute<TypeDiscriminatedAttribute>(false, false, true);
            if (att != null && !string.IsNullOrWhiteSpace(att.Name)) return att.Name;
            return type.Name;
        };
    }
}
