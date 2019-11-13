using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System;

namespace Ordinem.Shell.Wpf.Gateway
{
	public class GatewayStartup
	{
		public static void Main(string[] args)
		{
			Console.Title = "Gateway";
			Host
				.CreateDefaultBuilder(args)
				.ConfigureAppConfiguration(builder =>
				{
					builder.AddJsonFile("ocelot.json", false, true);
				})
				.ConfigureWebHostDefaults(builder =>
				{
					builder.UseStartup<GatewayStartup>();
				})
				.Build()
				.Run();
		}

		public GatewayStartup(IConfiguration configuration) => Configuration = configuration;

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvcCore(options => options.EnableEndpointRouting = false).SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
			services.AddOcelot(Configuration);
			//.AddCacheManager(x => {
			//	x.WithMicrosoftLogging(log =>
			//	{
			//		log.AddConsole(LogLevel.Trace);
			//		log.AddDebug(LogLevel.Trace);
			//	})
			//	.WithDictionaryHandle();
			//});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public async void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
				app.UseDeveloperExceptionPage();
			else
				app.UseHsts();
			app.UseHttpsRedirection();
			app.UseMvc();
			await app.UseOcelot();
		}
	}
}
