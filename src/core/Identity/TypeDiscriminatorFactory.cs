using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Fuxion.Identity
{
	public class TypeDiscriminatorFactory
	{
		public string DiscriminatorTypeName { get; set; } = TypeDiscriminator.TypeDiscriminatorId;
		public bool AllowMoreThanOneTypeByDiscriminator { get; set; }
		#region Classes
		[DebuggerDisplay("{" + nameof(Discriminator) + "}")]
		private class Entry
		{
			public Entry(TypeDiscriminator discriminaator)
			{
				Discriminator = discriminaator;
			}

			public string Id => Discriminator.Id;

			public List<EntryType> Types { get; set; } = new List<EntryType>();
			public TypeDiscriminator Discriminator { get; set; }
			public bool IsVirtual => Types.IsNullOrEmpty();
		}
		[DebuggerDisplay("{" + nameof(Type) + "}")]
		private class EntryType
		{
			public EntryType(Entry entry, Type type, TypeDiscriminatedAttribute? attribute)
			{
				Entry = entry;
				Type = type;
				Attribute = attribute;
			}
			public Entry Entry { get; set; }
			public Type Type { get; set; }
			public TypeDiscriminatedAttribute? Attribute { get; set; }

			public EntryType? BaseType { get; set; }
			public EntryType? DeepBaseType { get; set; }
			public List<EntryType> DerivedTypes { get; set; } = new List<EntryType>();
		}
		[DebuggerDisplay("{" + nameof(Count) + "} entries")]
		private class EntryList : KeyedCollection<string, Entry>
		{
			protected override string GetKeyForItem(Entry item) => item.Id;
		}
		#endregion
		private readonly EntryList entries = new EntryList();
		private bool initialized = false;
		public Func<Type, TypeDiscriminatedAttribute?, string> GetIdFunction { get; set; } = (type, att) =>
		{
			if (att != null && !string.IsNullOrWhiteSpace(att.Id))
				return att.Id;
			return type.GetSignature(true);
		};
		public Func<Type, TypeDiscriminatedAttribute?, string> GetNameFunction { get; set; } = (type, att) =>
		{
			if (att != null && !string.IsNullOrWhiteSpace(att.Name))
				return att.Name;
			return type.Name;
		};
		public Func<string, string> GetVirtualNameFunction { get; set; } = (virtualId) => virtualId;

		private void Initialize()
		{
			ValidateEntries();
#if DEBUG
			Debug.WriteLine("List of types to initialize");
			var maxTypeFullNameLength = entries.SelectMany(e => e.Types).Max(t => t.Type.FullName.Length);
			foreach (var type in entries.SelectMany(e => e.Types).OrderBy(t => t.Type.Name))
			{
				Debug.WriteLine($"   {type.Type.FullName.PadLeft(maxTypeFullNameLength)} - {(type.Attribute == null ? "NULL" : $"{type.Attribute.DisableMode}")}");
			}
#endif
			// Creo todas las entradas virtuales que se han encontrado en inclusiones y exclusiones
			// No creo las entradas virtuales que han sido indicadas en un tipo pero no hay contra-parte
			// De esta forma, puedo NO cargar todos los tipos de un arbol y no tener errores con los tipos virtuales
			foreach (var id in entries.SelectMany(e => e.Types)
						.SelectMany(t => new string[] { }
							.Concat(t.Attribute?.ExplicitInclusions ?? Enumerable.Empty<string>())
							.Concat(t.Attribute?.AdditionalInclusions ?? Enumerable.Empty<string>())
							.Concat(t.Attribute?.AvoidedInclusions ?? Enumerable.Empty<string>()))
						.Intersect(entries.SelectMany(e => e.Types)
							.SelectMany(t => new string[] { }
								.Concat(t.Attribute?.ExplicitExclusions ?? Enumerable.Empty<string>())
								.Concat(t.Attribute?.AdditionalExclusions ?? Enumerable.Empty<string>())
								.Concat(t.Attribute?.AvoidedExclusions ?? Enumerable.Empty<string>())))
						.Where(id => !entries.Contains(id))
						.ToList())
			{
				entries.Add(new Entry(new TypeDiscriminator(TypeDiscriminator.TypeDiscriminatorId, DiscriminatorTypeName)
				{
					Id = id,
					Name = GetVirtualNameFunction(id)
				}));
			}
			// Coloco los tipos base y derivados de cada tipo
			{
				var allTypes = entries.SelectMany(e => e.Types ?? Enumerable.Empty<EntryType>()).ToList();
				void SetHierarchy(EntryType type)
				{
					var parent = type.Type.GetTypeInfo().BaseType;
					if (parent.GenericTypeArguments.Length > 0)
						parent = parent.GetTypeInfo().GetGenericTypeDefinition();
					type.BaseType = allTypes.FirstOrDefault(t => t.Type == parent);
					type.DeepBaseType = type.BaseType;
					type.DerivedTypes = allTypes
						.Where(t =>
						{
							if (t.Type.GetTypeInfo().BaseType.GenericTypeArguments.Length > 0)
								return type.Type == t.Type.GetTypeInfo().BaseType.GetGenericTypeDefinition();
							return t.Type.GetTypeInfo().BaseType == type.Type;
						})
						.ToList();

					while (type.DeepBaseType == null && parent != null)
					{
						parent = parent.GetTypeInfo().BaseType;
						type.DeepBaseType = allTypes.FirstOrDefault(t => t.Type == parent);
					}
				}
				foreach (var type in allTypes)
				{
					SetHierarchy(type);
				}
				// Aplico el DisableMode (sé que todos los disable modes de una entrada son iguales)
				{
					void ClearType(EntryType type)
					{
						// Remove from ALL TYPES ??????????
						entries.Remove(type.Entry);
						allTypes.Remove(type);
						foreach (var t in allTypes.Where(t => t.DerivedTypes.Contains(type)))
							SetHierarchy(t);
						foreach (var t in allTypes.Where(t => t.BaseType != null && t.BaseType == type))
							SetHierarchy(t);
						foreach (var t in allTypes.Where(t => t.DeepBaseType != null && t.BaseType == type))
							SetHierarchy(t);

					}
					void ClearDerived(EntryType ty)
					{
						ClearType(ty);
						foreach (var der in ty.DerivedTypes.ToList())
							ClearDerived(der);
					}
					foreach (var ent in entries.Where(e => !e.IsVirtual).ToList())
					{
						var type = ent.Types.FirstOrDefault(t => t.Attribute != null);
						if (type?.Attribute?.DisableMode != null)
						{
							switch (type.Attribute.DisableMode)
							{
								case TypeDiscriminationDisableMode.DisableType:
									ClearType(type);
									break;
								case TypeDiscriminationDisableMode.DisableHierarchy:
									ClearDerived(type);
									break;
								default:
									break;
							}
						}
					}
				}
				// Agrego a tipos cuya base es null y tienen deepbase a los derivados de este deep padre
				foreach (var type in allTypes.Where(t => t.BaseType == null && t.DeepBaseType != null && !t.DeepBaseType.DerivedTypes.Contains(t)))
				{
					type.DeepBaseType?.DerivedTypes.Add(type);
				}
			}
			// Asigno las inclusiones y exclusiones
			{
				foreach (var ent in entries)
				{
					List<TypeDiscriminator> Search(IEnumerable<string> source)
						=> entries.Where(e => source?.RemoveNulls().Contains(e.Id) ?? false)
							.Select(e => e.Discriminator)
							.ToList();
					// EXPLICITS
					{
						// Inclusions
						ent.Discriminator.Inclusions = Search(ent.Types.SelectMany(t => t.Attribute?.ExplicitInclusions ?? new string[] { }))
							.Transform(res => res.Count > 0
								? res
								: ent.Types?.SelectMany(t => t.DerivedTypes.Select(t2 => t2.Entry.Discriminator)).Distinct().ToList() ?? Enumerable.Empty<TypeDiscriminator>().ToList());
						// Exclusions
						ent.Discriminator.Exclusions = Search(ent.Types.SelectMany(t => t.Attribute?.ExplicitExclusions ?? new string[] { }))
							.Transform(res => res.Count > 0
								? res
								: ent.Discriminator.Exclusions = ent.Types?.Select(t => t.DeepBaseType?.Entry.Discriminator).RemoveNulls().ToList() ?? Enumerable.Empty<TypeDiscriminator>().ToList());
					}
					// ADDED
					{
						// Inclusions
						ent.Discriminator.Inclusions.AddRange(Search(ent.Types.SelectMany(t => t.Attribute?.AdditionalInclusions ?? new string[] { })));
						// Exclusions
						ent.Discriminator.Exclusions.AddRange(Search(ent.Types.SelectMany(t => t.Attribute?.AdditionalExclusions ?? new string[] { })));
					}
					// AVOIDED
					{
						// Inclusions
						foreach (var avo in Search(ent.Types.SelectMany(t => t.Attribute?.AvoidedInclusions ?? new string[] { })))
						{
							Debug.WriteLine($"Inclusion {avo.Id} avoided from {ent.Id}");
							ent.Discriminator.Inclusions.RemoveAll(d => d.Id == avo.Id);
						}
						// Exclusions
						foreach (var avo in Search(ent.Types.SelectMany(t => t.Attribute?.AvoidedExclusions ?? new string[] { })))
						{
							Debug.WriteLine($"Exclusion {avo.Id} avoided from {ent.Id}");
							ent.Discriminator.Exclusions.RemoveAll(d => d.Id == avo.Id);
						}
					}
				}
				// VIRTUAL TYPES
				{
					foreach (var ent in entries.Where(e => e.IsVirtual))
					{
						ent.Discriminator.Inclusions = entries.Where(e => e.Discriminator.Exclusions?.Contains(ent.Discriminator) ?? false).Select(e => e.Discriminator).ToList();
						ent.Discriminator.Exclusions = entries.Where(e => e.Discriminator.Inclusions?.Contains(ent.Discriminator) ?? false).Select(e => e.Discriminator).ToList();
					}
				}
			}
			ValidateRegistrations();
			initialized = true;
		}
		private List<InvalidTypeDiscriminatorException> ValidateEntries(bool throwException = true)
		{
			var errors = new List<InvalidTypeDiscriminatorException>();
			foreach (var ent in entries)
			{
				// Comprobar incongruencias en el Mode
				if (ent.Types?.Select(t => t.Attribute?.DisableMode).RemoveNulls().Distinct().Count() > 1)
				{
					errors.Add(new InvalidTypeDiscriminatorException($"The type discriminator '{ent.Discriminator.Name}' has incongruence on Mode definition"));
					// ................
				}
				// Comprobar que al deshabilitar todo el arbol de herencia no se han especificado ningún otro parámetro
				foreach (var att in ent.Types.Select(t => t.Attribute)
					.RemoveNulls()
					.Where(a => a.DisableMode != null))
				{
					if (att.AdditionalExclusions != null)
						errors.Add(new InvalidTypeDiscriminatorException($"The type discriminator '{ent.Discriminator.Name}' define '{nameof(att.DisableMode)}' property " +
							$"but define '{nameof(att.AdditionalExclusions)}' parameter too, this is invalid configuration"));
					if (att.AdditionalInclusions != null)
						errors.Add(new InvalidTypeDiscriminatorException($"The type discriminator '{ent.Discriminator.Name}' define '{nameof(att.DisableMode)}' property " +
							$"but define '{nameof(att.AdditionalInclusions)}' parameter too, this is invalid configuration"));
					if (att.AvoidedExclusions != null)
						errors.Add(new InvalidTypeDiscriminatorException($"The type discriminator '{ent.Discriminator.Name}' define '{nameof(att.DisableMode)}' property " +
							$"but define '{nameof(att.AvoidedExclusions)}' parameter too, this is invalid configuration"));
					if (att.AvoidedInclusions != null)
						errors.Add(new InvalidTypeDiscriminatorException($"The type discriminator '{ent.Discriminator.Name}' define '{nameof(att.DisableMode)}' property " +
							$"but define '{nameof(att.AvoidedInclusions)}' parameter too, this is invalid configuration"));
					if (att.ExplicitExclusions != null)
						errors.Add(new InvalidTypeDiscriminatorException($"The type discriminator '{ent.Discriminator.Name}' define '{nameof(att.DisableMode)}' property " +
							$"but define '{nameof(att.ExplicitExclusions)}' parameter too, this is invalid configuration"));
					if (att.ExplicitInclusions != null)
						errors.Add(new InvalidTypeDiscriminatorException($"The type discriminator '{ent.Discriminator.Name}' define '{nameof(att.DisableMode)}' property " +
							$"but define '{nameof(att.ExplicitInclusions)}' parameter too, this is invalid configuration"));
					if (att.Id != null)
						errors.Add(new InvalidTypeDiscriminatorException($"The type discriminator '{ent.Discriminator.Name}' define '{nameof(att.DisableMode)}' property " +
							$"but define '{nameof(att.Id)}' parameter too, this is invalid configuration"));
					if (att.Name != null)
						errors.Add(new InvalidTypeDiscriminatorException($"The type discriminator '{ent.Discriminator.Name}' define '{nameof(att.DisableMode)}' property " +
							$"but define '{nameof(att.Name)}' parameter too, this is invalid configuration"));
				}
			}
			if (throwException & errors.Count > 0)
				throw new TypeDiscriminatorRegistrationValidationException($"Validation of type discriminators registrations has '{errors.Count}' errors", errors);
			return errors;
		}
		private List<InvalidTypeDiscriminatorException> ValidateRegistrations(bool throwException = true)
		{
#if DEBUG && TRUE
			foreach (var ent in entries)
			{
				Debug.WriteLine($"ENTRY: {ent.Discriminator.Id}");
				Debug.WriteLine($"\tInclusions: {ent.Discriminator.Inclusions.Aggregate("\r\n\t\t", (a, e) => a + "\r\n\t\t" + e.Id, a => a.Trim('\r', '\n', '\t'))}");
				Debug.WriteLine($"\tExclusions: {ent.Discriminator.Exclusions.Aggregate("\r\n\t\t", (a, e) => a + "\r\n\t\t" + e.Id, a => a.Trim('\r', '\n', '\t'))}");
			}
#endif
			var errors = new List<InvalidTypeDiscriminatorException>();
			foreach (var ent in entries)
			{
				// Comprobar que para cada inclusion hay una exclusion
				foreach (var dis in ent.Discriminator.Inclusions.Where(i => !i.Exclusions.Contains(ent.Discriminator)))
					errors.Add(new InvalidTypeDiscriminatorException($"The type discriminator '{ent.Discriminator.Name}' include the type discriminator '{dis.Name}', but '{dis.Name}' not exclude '{ent.Discriminator.Name}'"));
				foreach (var dis in ent.Discriminator.Exclusions.Where(i => !i.Inclusions.Contains(ent.Discriminator)))
					errors.Add(new InvalidTypeDiscriminatorException($"The type discriminator '{ent.Discriminator.Name}' exclude the type discriminator '{dis.Name}', but '{dis.Name}' not include '{ent.Discriminator.Name}'"));

				// Comprobar que todos los tipos incluidos y excluidos explicitamente con el atributo estan en la lista
				foreach (var id in ent.Types?
					.SelectMany(t => t.Attribute?.ExplicitInclusions ?? new string[] { })
					.Where(id => !entries.Select(e => e.Id).Contains(id)) ?? Enumerable.Empty<string>())
					errors.Add(new InvalidTypeDiscriminatorException($"The type discriminator '{ent.Discriminator.Name}' include the type discriminator '{id}', but '{id}' is not registered"));
				foreach (var id in ent.Types?
					.SelectMany(t => t.Attribute?.ExplicitExclusions ?? new string[] { })
					.Where(id => !entries.Select(e => e.Id).Contains(id)) ?? Enumerable.Empty<string>())
					errors.Add(new InvalidTypeDiscriminatorException($"The type discriminator '{ent.Discriminator.Name}' exclude the type discriminator '{id}', but '{id}' is not registered"));
			}
			if (throwException && errors.Count > 0)
				throw new TypeDiscriminatorRegistrationValidationException($"Validation of type discriminators registrations has '{errors.Count}' errors", errors);
			return errors;
		}

		public void RegisterTree<T>(params Type[] types) => RegisterTree(typeof(T), types);
		public void RegisterTree(Type baseType, params Type[] types)
			=> Register((types.IsNullOrEmpty() ? baseType.GetTypeInfo().Assembly.DefinedTypes.Select(ti => ti.AsType()).ToArray() : types)
				.Where(type => baseType == type ||
					(baseType.GetTypeInfo().IsGenericTypeDefinition
						? type.IsSubclassOfRawGeneric(baseType)
						: type.GetTypeInfo().IsSubclassOf(baseType))).ToArray());
		public void Register<T>() => Register(typeof(T));
		public void Register(params Type[] types)
		{
			if (initialized)
				throw new InvalidOperationException("Cannot do registers after obtain discriminators, please, do all registers before use");
			types = types.RemoveNulls();
			foreach (var type in types)
			{
				var att = type.GetTypeInfo().GetCustomAttribute<TypeDiscriminatedAttribute>(false, false, true);
				var ent = new Entry(new TypeDiscriminator(TypeDiscriminator.TypeDiscriminatorId, DiscriminatorTypeName)
				{
					Id = GetIdFunction(type, att),
					Name = GetNameFunction(type, att)
				});
				var typ = new EntryType(ent, type, att);
				ent.Types = new[] { typ }.ToList();
				var existent = entries.FirstOrDefault(e => e.Discriminator.Id == ent.Discriminator.Id);
				if (existent != null)
				{
					// La entrada ya existe, agrego el tipo a la lista de tipos de la entrada existente
					if (AllowMoreThanOneTypeByDiscriminator)
					{
						existent.Types.AddRange(ent.Types.Transform(t => t.Entry = existent));
					}
					else
						throw new Exception($"Type '{type.FullName}' cannot be registered because the id '{ent.Discriminator.Id}' already registered for '{existent.Discriminator.Id}'");
				}
				else
					entries.Add(ent);
			}
		}
		public void ClearRegistrations()
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
			if (res.Count() > 1)
				throw new InvalidStateException($"More than one discriminator for the id '{id}'. Use '{nameof(AllFromId)}' method instead");
			return res.FirstOrDefault();
		}
		public IEnumerable<TypeDiscriminator> AllFromId(string id)
		{
			if (!initialized)
				Initialize();
			return entries.Where(e => e.Discriminator.Id == id).Select(e => e.Discriminator);
		}
		public IEnumerable<Type> TypesFromId(string id)
		{
			if (!initialized)
				Initialize();
			return entries[id].Types?.Select(t => t.Type) ?? new Type[] { };
		}
	}
	public class TypeDiscriminatorRegistrationValidationException : FuxionAggregateException<InvalidTypeDiscriminatorException>
	{
		public TypeDiscriminatorRegistrationValidationException(string message, IEnumerable<InvalidTypeDiscriminatorException> innerExceptions) : base(message, innerExceptions) { }
	}
	public class InvalidTypeDiscriminatorException : FuxionException
	{
		public InvalidTypeDiscriminatorException(string message) : base(message) { }
	}
}
