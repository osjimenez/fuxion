using System.Reflection;
using Microsoft.AspNetCore.Routing;

namespace Fuxion.AspNetCore;

public interface IEndpoint
{
	void MapEndpoint(IEndpointRouteBuilder builder);
}

// ReSharper disable once UnusedTypeParameter
public interface IEndpoint<TRouteGroup> : IEndpoint where TRouteGroup : IRouteGroup, new();

public interface IRouteGroup
{
	RouteGroupBuilder Group(IEndpointRouteBuilder builder);
}

// ReSharper disable once UnusedTypeParameter
public interface IRouteGroup<TRouteGroup> : IRouteGroup where TRouteGroup : IRouteGroup, new();

public static class EndpointsExtensions
{
	public static void MapEndpointsForAssembly(this IEndpointRouteBuilder builder, Assembly assembly)
	{
		// Compruebo que no haya endpoints o grupos mal configurados
		foreach (var res in assembly.GetTypes()
			.Where(t => t is { IsInterface: false, IsAbstract: false })
			.Select(t =>
			{
				var isEndpoint = t.GetInterfaces().Any(i => i == typeof(IEndpoint));
				var isEndpointGroup = t.GetInterfaces().Any(i => i.IsSubclassOfRawGeneric(typeof(IEndpoint<>)));
				var isGroup = t.GetInterfaces().Any(i => i == typeof(IRouteGroup));
				var isGroupWithParent = t.GetInterfaces().Any(i => i.IsSubclassOfRawGeneric(typeof(IRouteGroup<>)));
				List<bool> res = [isEndpoint, isEndpointGroup, isGroup, isGroupWithParent];
				return (Type: t, Count: res.Count(r => r));
			}))
			if (res.Count > 2)
				throw new InvalidOperationException($"The type '{res.Type.GetSignature()}' can't implement more than one of IEndpoint, IEndpoint<TRouteGroup>, IRouteGroup, IRouteGroup<TRouteGroup> at the same time");

		// Primero busco los endpoints sin grupo
		var endpoints = assembly.GetTypes()
			.Where(t => t is { IsInterface: false, IsAbstract: false })
			.Where(t => t.GetInterfaces().Any(i => i == typeof(IEndpoint))
				&& !t.GetInterfaces().Any(i => i.IsSubclassOfRawGeneric(typeof(IEndpoint<>))))
			.Select(t =>
			{
				if (t.GetConstructors().Any(c => c.GetParameters().Length != 0))
					throw new InvalidOperationException($"The type '{t.GetSignature()}' only can has one parameterless constructor");
				var endpoint = (IEndpoint)Activator.CreateInstance(t)!;
				return endpoint;
			})
			.ToList();
		// Registro los endpoints sin grupo
		foreach (var endpoint in endpoints) endpoint.MapEndpoint(builder);

		// Busco los grupos
		var allGroups = assembly.GetTypes()
			.Where(t => t is { IsInterface: false, IsAbstract: false })
			.Where(t => t.GetInterfaces().Any(i => i == typeof(IRouteGroup)))
			.Select(t =>
			{
				if (t.GetConstructors().Any(c => c.GetParameters().Length != 0))
					throw new InvalidOperationException($"The type '{t.GetSignature()}' only can has one parameterless constructor");
				var group = (IRouteGroup)Activator.CreateInstance(t)!;
				Type? parentGroupType = null;
				IRouteGroup? parentGroup = null;
				if (t.GetInterfaces().Any(i => i.IsSubclassOfRawGeneric(typeof(IRouteGroup<>))))
				{
					parentGroupType = t.GetInterfaces()
						.First(i => i.IsSubclassOfRawGeneric(typeof(IRouteGroup<>)))
						.GetGenericArguments()[0];
					parentGroup = (IRouteGroup)Activator.CreateInstance(parentGroupType)!;
				}

				return new MapEndpointsForAssemblyData
				{
					GroupType = t,
					Group = group,
					ParentGroupType = parentGroupType,
					ParentGroup = parentGroup
				};
			})
			.ToList();

		// Registro los grupos con su jerarquía
		var currentGroups = allGroups.Where(g => g.ParentGroup == null).ToList();
		while (currentGroups.Count != 0)
		{
			var data = currentGroups.First();
			data.Builder = data.Group.Group(data.ParentGroup != null
				? allGroups.First(g => g.GroupType == data.ParentGroupType).Builder!
				: builder);
			currentGroups.Remove(data);
			currentGroups.AddRange(allGroups.Where(g => g.ParentGroupType == data.GroupType));
		}

		// Busco los endpoints con grupo
		var endpointsWithGroups = assembly.GetTypes()
			.Where(t => t is { IsInterface: false, IsAbstract: false })
			.Where(t => t.GetInterfaces().Any(i => i.IsSubclassOfRawGeneric(typeof(IEndpoint<>))))
			.Select(t =>
			{
				if (t.GetConstructors().Any(c => c.GetParameters().Length != 0))
					throw new InvalidOperationException($"The type '{t.GetSignature()}' only can has one parameterless constructor");
				var endpoint = (IEndpoint)Activator.CreateInstance(t)!;
				return endpoint;
			})
			.ToList();

		// Registro los endpoints con grupo
		foreach (var endpoint in endpointsWithGroups)
		{
			var groupType = endpoint.GetType()
				.GetInterfaces().First(i => i.IsSubclassOfRawGeneric(typeof(IEndpoint<>)))
				.GetGenericArguments()[0];
			endpoint.MapEndpoint(allGroups.First(g => g.GroupType == groupType).Builder!);
		}
	}
}
file class MapEndpointsForAssemblyData
{
	public required Type GroupType { get; set; }
	public required IRouteGroup Group { get; set; }
	public Type? ParentGroupType { get; set; }
	public IRouteGroup? ParentGroup { get; set; }
	public RouteGroupBuilder? Builder { get; set; }
}