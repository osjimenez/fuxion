using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Fuxion.Reflection;

public static class TypeKeyExtensions
{
	public static TypeKey? GetTypeKey(this Type me, [DoesNotReturnIf(true)] bool exceptionIfNotFound = true, bool processFullNameIfNotFound = false) =>
		me.GetCustomAttribute<TypeKeyAttribute>(false, exceptionIfNotFound, true)?.KeyChain
		?? (processFullNameIfNotFound
			? me.FullName is not null
				? new(me.FullName.Split('.'))
				: (TypeKey?)null
			: null)
		?? throw new TypeKeyException($"Couldn't be possible obtain TypeKey for type '{me.GetSignature()}'");
}