using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
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
			me.Services.AddSingleton<Profile, T>();
			return me;
		}
	}
	public interface IAutoMapperBuilder
	{
		IServiceCollection Services { get; }
	}
	class AutoMapperBuilder : IAutoMapperBuilder
	{
		public AutoMapperBuilder(IServiceCollection services) => this.Services = services;
		public IServiceCollection Services { get; }
	}
}
