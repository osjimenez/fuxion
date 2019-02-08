using Fuxion.Factories;
using Fuxion.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Net472
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			Factory.AddInjector(new InstanceInjector<ILogFactory>(new Log4netFactory()));

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

			base.OnStartup(e);
		}
	}
}
