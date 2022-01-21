namespace Microsoft.Extensions.DependencyInjection;

using AutoMapper;

public static class AutoMapperDIExtensions
{
	public static IAutoMapperBuilder AddAutoMapper(this IServiceCollection services)
	{
		// Configuration
		services.AddSingleton(sp =>
		{
			return new MapperConfiguration(cfg =>
			{
					//add your profiles (either resolve from container or however else you acquire them)
					foreach (var profile in sp.GetServices<Profile>())
				{
					cfg.AddProfile(profile);
				}
			});
		});

		// Mapper
		services.AddScoped(sp => sp.GetRequiredService<MapperConfiguration>().CreateMapper(sp.GetRequiredService));

		return new AutoMapperBuilder(services);
	}
	public static IAutoMapperBuilder AddProfile<T>(this IAutoMapperBuilder me) where T : Profile
	{
		me.Services.AddTransient<Profile, T>();
		return me;
	}
	public static IServiceCollection AddAutoMapperProfile<T>(this IServiceCollection me) where T : Profile
	{
		me.AddTransient<Profile, T>();
		return me;
	}
}
public interface IAutoMapperBuilder
{
	IServiceCollection Services { get; }
}

internal class AutoMapperBuilder : IAutoMapperBuilder
{
	public AutoMapperBuilder(IServiceCollection services) => Services = services;
	public IServiceCollection Services { get; }
}