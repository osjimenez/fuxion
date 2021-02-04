using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Ordinem.Shell.Web.Launcher.Pages;
using Syncfusion.Blazor;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Ordinem.Shell.Web.Launcher
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = WebAssemblyHostBuilder.CreateDefault(args);
			builder.RootComponents.Add<App>("#app");

			builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
			//Register Syncfusion license 
			Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MzUxNzY1QDMxMzgyZTMzMmUzME5BbmhCcjM4NzZKMUo5RlQ0WUR5VENCYjFWd2pScnVGMmlwaS9zbHhES2M9");
			builder.Services.AddSyncfusionBlazor();

			builder.Services.AddScoped(sp => new IndexJsInterop(sp.GetRequiredService<IJSRuntime>()));

			await builder.Build().RunAsync();
		}
	}
}
