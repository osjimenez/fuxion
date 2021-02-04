using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Syncfusion.Blazor.Layouts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Ordinem.Shell.Web.Launcher.Pages
{
	public partial class Index
	{
		[Inject]
		IJSRuntime? jsRuntime { get; set; }
		[Inject]
		IndexJsInterop? JS { get; set; }
		SfDashboardLayout? dashboardObject;

		public async Task Click(MouseEventArgs e)
		{
			if (jsRuntime is not null)
			{
				//await jsRuntime.InvokeVoidAsync("showPrompt", "https://www.google.com");
				//await jsRuntime.InvokeVoidAsync("ShowAlert", "Hola mundo");

				var module = await jsRuntime.InvokeAsync<IJSObjectReference>(
					"import", "./js/demo.js");
				await module.InvokeVoidAsync("showPrompt", "fetchdata");
			}
		}
	}
	public class IndexJsInterop : IAsyncDisposable
	{
		private readonly Lazy<Task<IJSObjectReference>> moduleTask;

		public IndexJsInterop(IJSRuntime jsRuntime)
		{
			moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
			   "import", "./_content/Ordinem.Tasks.Shell.Web/exampleJsInterop.js").AsTask());
		}

		public async ValueTask<string> Prompt(string message)
		{
			var module = await moduleTask.Value;
			return await module.InvokeAsync<string>("showPrompt", message);
		}

		public async ValueTask DisposeAsync()
		{
			if (moduleTask.IsValueCreated)
			{
				var module = await moduleTask.Value;
				await module.DisposeAsync();
			}
		}
	}
}
