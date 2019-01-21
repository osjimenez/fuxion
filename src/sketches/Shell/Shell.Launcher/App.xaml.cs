using Fuxion.Shell.UIMessages;
using Fuxion.Shell.Views;
using Fuxion.Shell;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Fuxion.Factories;
using Fuxion.Logging;
using Fuxion.Shell.ViewModels;

namespace Shell.Launcher
{
	public partial class App : System.Windows.Application
	{
		public App()
		{
			// A helper method that will register all classes that derive off IViewFor 
			// into our dependency injection container. ReactiveUI uses Splat for it's 
			// dependency injection by default, but you can override this if you like.
			//Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetCallingAssembly());
			//Locator.CurrentMutable.RegisterViewsForViewModels(typeof(ShellWindow).Assembly);
		}
		protected override void OnStartup(StartupEventArgs e)
		{
			//Factory.AddInjector(new InstanceInjector<ILogFactory>(new Log4netFactory()));

			//setup our DI
			var services = new ServiceCollection()
				.AddSingleton<IModule, LauncherModule>()
				.AddSingleton<IModule, ModuleA.ModuleA>()
				.AddSingleton<MenuManager>()
				.AddSingleton<DockingManager>()
				.AddSingleton<ShellViewModel>()
				.AddSingleton<ShellWindow>();

			foreach(var module in services.BuildServiceProvider().GetServices<IModule>())
				module.Register(services);

			foreach (var module in services.BuildServiceProvider().GetServices<IModule>())
				module.Initialize();

			services.BuildServiceProvider().GetRequiredService<ShellWindow>().Show();

			MessageBus.Current.SendMessage(new LoadLayoutUIMessage());
		}
	}
}
