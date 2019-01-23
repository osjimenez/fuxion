using Fuxion.Shell;
using Fuxion.Shell.UIMessages;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Shell.ModuleA.Views;
using System;
using Fuxion.Reflection;
namespace Shell.ModuleA
{
	public class ModuleA : IModule
	{
		public void Register(IServiceCollection services)
		{
			services.AddMenu("About", () => MessageBus.Current.SendMessage(new OpenPanelUIMessage(PanelName.Parse(typeof(AboutPanel).GetTypeKey()))));

			services.AddPanel<AboutPanel>();
		}
		public void Initialize() { }
		
	}
}
