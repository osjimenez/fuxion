using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Fuxion.Reflection;

public static class TypeKeyExtensions
{
	public static TypeKey? GetTypeKey(this Type me, [DoesNotReturnIf(true)] bool exceptionIfNotFound = true, bool processFullNameIfNotFound = false, bool processInheritance = true)
	{
		List<string[]> keyChain = new();
		if (processInheritance)
		{
			var type = me.BaseType;
			Type? failType = null;
			Type? baseType = null;
			while (type is not null)
			{
				try
				{
					var key = type.GetTypeKey(exceptionIfNotFound, processFullNameIfNotFound, false);
					if (key is null)
					{
						failType = type;
						continue;
					}
					keyChain.Add(key.KeyChain);
					if (failType is not null && baseType is null) baseType = type;
				} catch
				{
					failType ??= type;
				} finally
				{
					type = type.BaseType;
				}
			}
			if (failType is not null && baseType is not null) 
				throw new TypeKeyException($"The type '{failType.GetSignature()}' is not adorned with '{nameof(TypeKeyAttribute)}' but type '{baseType.GetSignature()}' is, the inheritance chain is broken");
		}
		keyChain.Reverse();
		var current =
			me.GetCustomAttribute<TypeKeyAttribute>(false, exceptionIfNotFound, true)?.KeyChain 
			?? (processFullNameIfNotFound
				? me.FullName is not null
					? new(me.FullName)
					: (TypeKey?)null
				: null)
			?? throw new TypeKeyException($"Couldn't be possible obtain TypeKey for type '{me.GetSignature()}'");
		return new(keyChain.SelectMany(key => key).Concat(current.KeyChain).ToArray());
	}
}