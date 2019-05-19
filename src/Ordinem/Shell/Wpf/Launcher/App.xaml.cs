using Fuxion.Shell;
using Microsoft.Extensions.DependencyInjection;
using Ordinem.Calendar.Shell.Wpf;
using Ordinem.Tasks.Shell.Wpf;
using ReactiveUI;
using System.Reflection;
using System.Windows;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Ordinem.Shell.Wpf.Launcher
{
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			var services = new ServiceCollection();

			//var repo = log4net.LogManager.CreateRepository(Assembly.GetEntryAssembly(),
			//		   typeof(log4net.Repository.Hierarchy.Hierarchy));
			//// TODO - https://issues.apache.org/jira/browse/LOG4NET-467
			//Factory.AddInjector(new InstanceInjector<ILogFactory>(new Log4netFactory()));

			//var log = LogManager.Create<App>();
			//log.Verbose("Verbose");
			//log.Trace("Trace");
			//log.Debug("Debug");
			//log.Info("Info");
			//log.Notice("Notice");
			//log.Warn("Warn");
			//log.Error("Error");
			//log.Severe("Severe");
			//log.Critical("Critical");
			//log.Alert("Alert");
			//log.Fatal("Fatal");
			//log.Emergency("Emergency");

			services.AddAutoMapper();
			var loggingConfiguration = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
				.Build();
			services.AddLogging(_ =>
			{
				_.AddConfiguration(loggingConfiguration.GetSection("Logging"));
				_.AddConsole();
			});
			services.AddFuxion(_ => _
				.Shell(__ => __
					.Module<LauncherModule>()
					//.ModulesFromAssemblyOf<Ordinem_Shell_Wpf_Tasks>()
					.Module<TasksModule>()
					.Module<CalendarModule>()
				//.ShellFactory(out shellFactory)
				));
			//shellFactory().Show();

			MessageBus.Current.LoadLayout();
		}
	}
}
