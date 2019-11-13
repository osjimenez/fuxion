using Microsoft.AspNetCore;
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
			WebHost
				.CreateDefaultBuilder(args)
				.UseStartup<GatewayStartup>()
				.ConfigureAppConfiguration(builder =>
				{
					builder.AddJsonFile("ocelot.json", false, true);
				})
				.Build()
				.Run();
		}

		public GatewayStartup(IConfiguration configuration) => Configuration = configuration;

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvcCore().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
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
		public async void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
		{
			if (env.IsDevelopment())
				app.UseDeveloperExceptionPage();
			//else
			//	app.UseHsts();
			//app.UseHttpsRedirection();
			app.UseMvc();
			await app.UseOcelot();
		}
	}
}
