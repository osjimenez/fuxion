using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using Fuxion.Logging;
namespace Fuxion.Identity
{
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
            public List<EntryType> Types { get; set; }
            public TypeDiscriminator Discriminator { get; set; }

            public List<Entry> AllInclusions { get; set; } = new List<Entry>();
            public List<Entry> AllExclusions { get; set; } = new List<Entry>();

            public bool IsVirtual => Types.IsNullOrEmpty();
            public bool HasAttribute => Types?.Any(t => t.Attribute != null) ?? false;
            public bool DefineInclusionsExplicitly => Types?.Any(t => t.DefineInclusionsExplicitly) ?? false;
            public bool DefineExclusionsExplicitly => Types?.Any(t => t.DefineExclusionsExplicitly) ?? false;
        }
        [DebuggerDisplay("{" + nameof(Type) + "}")]
        class EntryType
        {
            public Type Type { get; set; }
            public TypeDiscriminatedAttribute Attribute { get; set; }
            public bool DefineInclusionsExplicitly => !Attribute?.Inclusions.IsNullOrEmpty() ?? false;
            public bool DefineExclusionsExplicitly => !Attribute?.Exclusions.IsNullOrEmpty() ?? false;
        }
        List<Entry> entries = new List<Entry>();
        bool initialized = false;

        //public Func<Type, string> GetIdFunction { get; set; } = type =>
        //{
        //    var att = type.GetTypeInfo().GetCustomAttribute<TypeDiscriminatedAttribute>(false, false, true);
        //    if (att != null && !string.IsNullOrWhiteSpace(att.Id)) return att.Id;
        //    return type.GetSignature(true);
        //};
        public Func<Type, TypeDiscriminatedAttribute, string> GetIdFunction { get; set; } = (type, att) =>
        {
            //var att = type.GetTypeInfo().GetCustomAttribute<TypeDiscriminatedAttribute>(false, false, true);
            if (att != null && !string.IsNullOrWhiteSpace(att.Id)) return att.Id;
            return type.GetSignature(true);
        };
        public Func<Type, TypeDiscriminatedAttribute, string> GetNameFunction { get; set; } = (type, att) =>
        {
            //var att = type.GetTypeInfo().GetCustomAttribute<TypeDiscriminatedAttribute>(false, false, true);
            if (att != null && !string.IsNullOrWhiteSpace(att.Name)) return att.Name;
            return type.Name;
        };

        private void Initialize()
        {
            // Recorro todas las entradas marcadas con el atributo TypeDiscriminated que han especificado el parámetro Inclusions o Exclusions
            // En estas entradas estan todos los posibles tipos virtuales
            foreach (var ent in entries
                .Where(e => e.HasAttribute) // Algun tipo contiene el atributo
                .Where(e => e.DefineInclusionsExplicitly || e.DefineExclusionsExplicitly)
                //.Where(e => !e.Attribute.Inclusions.IsNullOrEmpty() || !e.Attribute.Exclusions.IsNullOrEmpty())
                .ToList())
            {
                // Recorro las inclusiones y exclusiones cuyo id no esta en la lista de entradas
                foreach (var id in (
                        ent.Types
                            .Where(t => t.Attribute?.Inclusions != null)
                            .SelectMany(t => t.Attribute.Inclusions)
                            .Concat(ent.Types
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
                    foreach (var inc in ent.Types.SelectMany(t => t.Attribute.Inclusions.Select(i => new
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
                            ent.Types.Any(t => t.Type.GetTypeInfo().IsGenericTypeDefinition // Compruebo si el tipo es genérico
                                ? e.Types.Any(ty => ty.Type.IsSubclassOfRawGeneric(t.Type)) // Compruebo si es derivado del genérico
                                : e.Types.Any(ty => ty.Type.GetTypeInfo().IsSubclassOf(t.Type)))
                        )
                        .ToList();
                }
                // EXCLUSIONES
                if (ent.DefineExclusionsExplicitly)
                {
                    // Se han definido explicitamente las exclusiones
                    foreach (var exc in ent.Types.SelectMany(t => t.Attribute.Exclusions.Select(i => new
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
                else if (!ent.IsVirtual)
                {
                    // Se calcularan las exclusiones por herencia
                    ent.AllExclusions = entries
                        .Where(e =>
                            !e.IsVirtual && // Quito las entradas de tipos virtuales
                            ent.Discriminator.Id != e.Discriminator.Id && // Me excluyo a mi mismo
                            e.Types.Any(t => t.Type.GetTypeInfo().IsGenericTypeDefinition // Compruebo si el tipo es genérico
                                ? ent.Types.Any(ty => ty.Type.IsSubclassOfRawGeneric(t.Type)) // Compruebo si es derivado del genérico
                                : ent.Types.Any(ty => ty.Type.GetTypeInfo().IsSubclassOf(t.Type)))
                        )
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
            }
            foreach (var ent in entries)
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
            foreach (var ent in entries)
            {
                ent.Discriminator.Inclusions = ent.AllInclusions
                    .Except(ent.AllInclusions.SelectMany(e => e.AllInclusions))
                    .Select(e => e.Discriminator)
                    .ToList();
                ent.Discriminator.Exclusions = ent.AllExclusions
                    .Except(ent.AllExclusions.SelectMany(e => e.AllExclusions))
                    .Select(e => e.Discriminator)
                    .ToList();
            }
            ValidateRegistrations();
            initialized = true;
        }
        private void ValidateRegistrations()
        {
#if DEBUG
            foreach(var ent in entries)
            {
                Debug.WriteLine($"ENTRY: {ent.Discriminator.Id}");
                Debug.WriteLine($"   Inclusions: {ent.Discriminator.Inclusions.Aggregate("", (a, e) => a + ", " + e.Id, a => a.Trim(',',' '))}");
                Debug.WriteLine($"   Exclusions: {ent.Discriminator.Exclusions.Aggregate("", (a, e) => a + ", " + e.Id, a => a.Trim(',', ' '))}");
            }
#endif
            var errors = new List<InvalidTypeDiscriminatorException>();
            foreach(var ent in entries)
            {
                foreach(var dis in ent.Discriminator.Inclusions.Where(i => !i.Exclusions.Contains(ent.Discriminator)))
                    errors.Add(new InvalidTypeDiscriminatorException($"The discriminator '{ent.Discriminator.Name}' include the discriminator '{dis.Name}', but '{dis.Name}' not exclude '{ent.Discriminator.Name}'"));
                foreach(var dis in ent.Discriminator.Exclusions.Where(i => !i.Inclusions.Contains(ent.Discriminator)))
                    errors.Add(new InvalidTypeDiscriminatorException($"The discriminator '{ent.Discriminator.Name}' exclude the discriminator '{dis.Name}', but '{dis.Name}' not include '{ent.Discriminator.Name}'"));
            }
            if (errors.Count > 1)
                throw new AggregateException("Type discriminator registrations are not valid, see inner exceptions for details", errors);
            else if (errors.Count == 1)
                throw errors[0];
        }

        public void RegisterTree<T>(params Type[] types) => RegisterTree(typeof(T), types);
        public void RegisterTree(Type baseType, params Type[] types)
            => Register(types
                .Where(type => baseType == type ||
                    (baseType.GetTypeInfo().IsGenericTypeDefinition
                        ? type.IsSubclassOfRawGeneric(baseType)
                        : type.GetTypeInfo().IsSubclassOf(baseType))).ToArray());
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
                    Types = new[] 
                    {
                        new EntryType
                        {
                            Type = type,
                            Attribute = att
                        }
                    }.ToList(),
                    Discriminator = new TypeDiscriminator
                    {
                        Id = GetIdFunction(type, att),
                        Name = GetNameFunction(type, att),
                        TypeId = DiscriminatorTypeId,
                        TypeName = DiscriminatorTypeName
                    }
                };
                var existent = entries.FirstOrDefault(e => e.Discriminator.Id == ent.Discriminator.Id);
                if (existent != null)
                {
                    // La entrada ya existe, agrego el tipo a la lista de tipos de la entrada
                    if (AllowMoreThanOneTypeByDiscriminator)
                    {
                        existent.Types = existent.Types.Concat(ent.Types).ToList();
                    }
                    else throw new Exception($"Type '{type.FullName}' cannot be registered because the id '{ent.Discriminator.Id}' already registered for '{existent.Discriminator.Id}'");
                }
                else entries.Add(ent);
            }
        }
        public void ClearAllRegisters()
        {
            entries.Clear();
            initialized = false;
        }
        public IEnumerable<TypeDiscriminator> GetAll() => entries.Select(e => e.Discriminator);
        public TypeDiscriminator FromType<T>() => FromType(typeof(T));
        public TypeDiscriminator FromType(Type type) => FromId(GetIdFunction(type, type.GetTypeInfo().GetCustomAttribute<TypeDiscriminatedAttribute>(false, false, true)));
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
    }
    public class InvalidTypeDiscriminatorException : FuxionException
    {
        public InvalidTypeDiscriminatorException(string message) : base(message) { }
    }
}
