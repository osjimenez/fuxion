using Fuxion.Factories;
using Fuxion.Logging;
using System;

namespace Core30
{
	class Program
	{
		static void Main(string[] args)
		{
			Factory.AddInjector(new InstanceInjector<ILogFactory>(new Log4netFactory()));

			var log = LogManager.Create<Program>();
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

			Console.ReadLine();
		}
	}
}
