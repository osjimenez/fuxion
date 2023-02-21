using System.Reflection;
using Fuxion.EntityFrameworkCore;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjectionExtensions
{
	public static IServiceCollection AddDbTrigger(this IServiceCollection me, Type type, bool failIfNotImplementTriggersInterfaces = true)
	{
		var tBeforeContext = type.GetSubclassOfRawGeneric(typeof(IBeforeSaveTrigger<>));
		var tAfterContext = type.GetSubclassOfRawGeneric(typeof(IBeforeSaveTrigger<>));
		if (tBeforeContext is not null) me.AddScoped(typeof(IBeforeSaveTrigger<>).MakeGenericType(tBeforeContext), type);
		if (tAfterContext is not null) me.AddScoped(typeof(IAfterSaveTrigger<>).MakeGenericType(tAfterContext), type);
		if (tBeforeContext is null && tAfterContext is null && failIfNotImplementTriggersInterfaces)
			throw new NotSupportedException($"The type '{type.GetSignature(true)}' must implement 'IBeforeSaveTrigger<>' or 'IAfterSaveTrigger<>' interfaces");
		return me;
	}
	public static IServiceCollection AddDbTriggersFromAssembly(this IServiceCollection me, Assembly assembly)
	{
		foreach (var type in assembly.GetTypes()) AddDbTrigger(me, type, false);
		return me;
	}
}