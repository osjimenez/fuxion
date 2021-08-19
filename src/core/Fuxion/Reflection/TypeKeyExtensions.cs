namespace Fuxion.Reflection;

using System.Diagnostics.CodeAnalysis;
using System.Reflection;

public static class TypeKeyExtensions
{
	public static string? GetTypeKey(this Type me,
		[DoesNotReturnIf(true)] bool exceptionIfNotFound = true,
		[NotNullWhen(true)] bool returnFullNameIfNotFound = false)
		=> returnFullNameIfNotFound
			? me.GetCustomAttribute<TypeKeyAttribute>(false, false, true)?.TypeKey ?? me.FullName
			: me.GetCustomAttribute<TypeKeyAttribute>(false, exceptionIfNotFound, true)?.TypeKey;
}