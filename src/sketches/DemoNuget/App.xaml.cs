using Fuxion.Factories;
using Fuxion.Logging;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DemoNuget
{
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			//Factory.AddInjector(new InstanceInjector<ILogFactory>(new Log4netFactory()));
			Factory.AddInjector(new InstanceInjector<ILogFactory>(new Log4netFactory(builder => builder
				//.RootLevel(Level.All)
				//.WithConfigurationFile("ja.log4net")
				//.WithoutDefaultConfigurationFile()
				.AddAppender(new TestAppender(), new Fuxion.Logging.XmlLayout())
				)));

			ILog log = LogManager.Create("HOLA");

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

			base.OnStartup(e);
		}
	}
}
