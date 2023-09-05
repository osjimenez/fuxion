using Fuxion.AspNetCore.Controllers;

namespace Microsoft.Extensions.DependencyInjection;

public static class AspNetCoreDIExtensions
{
	public static IMvcBuilder AddFuxionControllers(this IMvcBuilder me) => me.AddApplicationPart(typeof(CommandController).Assembly);
	public static IMvcCoreBuilder AddFuxionControllers(this IMvcCoreBuilder me) => me.AddApplicationPart(typeof(CommandController).Assembly);
}