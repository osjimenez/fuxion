using Fuxion.Factories;
using Fuxion.Logging;
using Fuxion.Shell;
using Microsoft.Extensions.DependencyInjection;
using Ordinem.Calendar.Shell.Wpf;
using Ordinem.Tasks.Shell.Wpf;
using ReactiveUI;
using System.Reflection;
using System.Windows;

namespace Ordinem.Shell.Wpf.Launcher
{
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			var services = new ServiceCollection();

			//XmlDocument log4netConfig = new XmlDocument();
			//log4netConfig.Load(File.OpenRead("log4net.config"));
			var repo = log4net.LogManager.CreateRepository(Assembly.GetEntryAssembly(),
					   typeof(log4net.Repository.Hierarchy.Hierarchy));
			//log4net.Config.XmlConfigurator.Configure(repo);//, log4netConfig["log4net"]);
			//var logger = log4net.LogManager.GetLogger(typeof(App));
			//logger.Info("Info MESSAGE");

			//var logger2 = repo.GetLogger("LoggerDePrueba");
			//logger2.Log(new LoggingEvent(new LoggingEventData
			//{
			//	Level = Level.Notice,
			//	Message = "Mensaje"
			//}));
			//log4net.Util.LogLog.InternalDebugging = true;

			// TODO - https://issues.apache.org/jira/browse/LOG4NET-467
			Factory.AddInjector(new InstanceInjector<ILogFactory>(new Log4netFactory()));
			//log4net.Config.XmlConfigurator.Configure(repo);

			var log = LogManager.Create<App>();
			log.Verbose("Verbose");
			log.Trace("Trace");
			log.Debug("Debug");
			log.Info("Info");
			log.Notice("Notice");
			log.Warn("Warn");
			log.Error("Error");
			log.Severe("Severe");
			log.Critical("Critical");
			log.Alert("Alert");
			log.Fatal("Fatal");
			log.Emergency("Emergency");

			//Log.Logger = new LoggerConfiguration()
			//  .Enrich.FromLogContext()
			//  .WriteTo.Seq("http://localhost:5341")
			//  .CreateLogger();

			// add logging
			//services.AddLogging(_ =>
			//{
			//	_.SetMinimumLevel(LogLevel.Debug);
			//	_.AddSerilog(dispose: true);
			//	//_.AddFile("Logs/{Date}.json", LogLevel.Debug, isJson: true);
			//	_.AddConsole(o => o.IncludeScopes = true);
			//	_.AddDebug();
			//});

			services.AddAutoMapper();

			//Func<ShellWindow> shellFactory = null;
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
