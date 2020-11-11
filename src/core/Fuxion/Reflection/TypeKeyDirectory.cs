using Fuxion.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Fuxion.Reflection
{
	public class TypeKeyDirectory
	{
		Dictionary<string, Type> dic = new Dictionary<string, Type>();
		public Type this[string key]
		{
			get
			{
				if (!dic.ContainsKey(key)) throw new TypeKeyNotFoundInDirectoryException($"Key '{key}' not found in '{nameof(TypeKeyDirectory)}'");
				return dic[key];
			}
		}
		public bool ContainsKey(string key) => dic.ContainsKey(key);
		public void RegisterAssemblyOf(Type type, Func<(Type Type, TypeKeyAttribute Attribute), bool>? predicate = null) => RegisterAssembly(type.Assembly, predicate);
		public void RegisterAssemblyOf<T>(Func<(Type Type, TypeKeyAttribute Attribute), bool>? predicate = null) => RegisterAssembly(typeof(T).Assembly, predicate);
		public void RegisterAssembly(Assembly assembly, Func<(Type Type, TypeKeyAttribute Attribute), bool>? predicate = null)
		{
			var query = assembly.GetTypes()
				.Where(t => t.HasCustomAttribute<TypeKeyAttribute>(false))
				// NULLABLE - I check before that custom attribute exist
				.Select(t => (Type: t, Attribute: t.GetCustomAttribute<TypeKeyAttribute>()!));
			if (predicate != null)
				query = query.Where(predicate);
			foreach (var tup in query)
				Register(tup.Type);
		}
		public void Register<T>() => Register(typeof(T));
		public void Register(Type type)
		{
			var key = type.GetTypeKey();
			if (key == null) throw new ArgumentException($"The type '{type.Name}' isn't decorated with '{nameof(TypeKeyAttribute)}' attribute");
			dic.Add(key, type);
		}
	}
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class TypeKeyAttribute : Attribute
	{
		public TypeKeyAttribute(string typeKey)
		{
			if (string.IsNullOrWhiteSpace(typeKey)) throw new ArgumentException($"The key for attribute '{nameof(TypeKeyAttribute)}' cannot be null, empty or white spaces string", nameof(typeKey));
			TypeKey = typeKey;
		}
		public string TypeKey { get; }
	}
	public static class TypeKeyExtensions
	{
		public static string GetTypeKey(this Type me) => me.GetCustomAttribute<TypeKeyAttribute>(false, true).TypeKey;
		public static string GetTypeKey(this Type me, [DoesNotReturnIf(true)] bool exceptionIfNotFound) => me.GetCustomAttribute<TypeKeyAttribute>(false, exceptionIfNotFound, true)?.TypeKey;
	}
	public class TypeKeyNotFoundInDirectoryException : FuxionException
	{
		public TypeKeyNotFoundInDirectoryException(string message) : base(message) { }
	}
}
