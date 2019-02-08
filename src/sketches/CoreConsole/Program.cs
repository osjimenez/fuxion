using Fuxion;
using Fuxion.Factories;
using Fuxion.Logging;
using Fuxion.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Context;
using Serilog.Formatting.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CoreConsole
{
	public class Logg : Microsoft.Extensions.Logging.ILogger
	{
		readonly Pro _provider;
		readonly Microsoft.Extensions.Logging.ILogger _logger;
		//public Logg(
		//    Pro provider,
		//    Microsoft.Extensions.Logging.ILogger logger = null,
		//    string name = null)
		//{
		//    if (provider == null) throw new ArgumentNullException(nameof(provider));
		//    _provider = provider;
		//    _logger = logger;

		//    // If a logger was passed, the provider has already added itself as an enricher
		//    _logger = _logger ?? Serilog.Log.Logger.ForContext(new[] { provider });

		//    if (name != null)
		//    {
		//        _logger = _logger.ForContext(Constants.SourceContextPropertyName, name);
		//    }
		//}
		public IDisposable BeginScope<TState>(TState state)
		{
			return this.AsDisposable(l => { });
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return true;
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			Debug.WriteLine("");
		}
	}
	public class Pro : ILoggerProvider
	{
		public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
		{
			return new Logg();
		}

		public void Dispose()
		{

		}
	}
	class Program
	{
		static void Main(string[] args)
		{
			//var fac = new LoggerFactory();
			//fac.AddSerilog(new LoggerConfiguration()
			//    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}{Properties}{NewLine}")
			//    .CreateLogger());
			//Factory.AddInjector(new InstanceInjector<ILoggerFactory>(fac));

			MainLog4Net();
			//MainMEL();
		}
		static void MainLog4Net()
		{
			//Console.ForegroundColor = ConsoleColor.DarkGray;

			Factory.AddInjector(new InstanceInjector<ILogFactory>(new Log4netFactory()));
			ILog log = LogManager.Create<Program>();
			for (int i = 0; i < 1000; i++)
				log.Info("Funciona");


			Console.WriteLine("Pulse ENTER to exit ...");
			Console.ReadLine();

			log4net.LogManager.Shutdown();
		}
		static void MainMEL()
		{
			var fac = new LoggerFactory();
			fac.AddProvider(new Pro());
			//fac.AddConsole(true);
			fac.AddSerilog(new LoggerConfiguration()
				.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}{Properties}{NewLine}")
				.WriteTo.File(new JsonFormatter(), "./my-app-log.json")
				.CreateLogger());

			Factory.AddInjector(new InstanceInjector<ILoggerFactory>(fac));

			using (fac.CreateLogger<Program>().BeginScope("OuterScope"))
			{
				var per = new Person("Oscar", 37);
				per.DoWork();
			}

			Console.WriteLine("PRESIONE ENTER PARA SALIR ...");
			Console.ReadLine();
		}
		static void Serilog()
		{
			var messages = new StringWriter();

			var log = new LoggerConfiguration()
				.Enrich.FromLogContext()
				.Destructure.ByTransforming<Person>(p => $"{p.Name}({p.Age})")
				.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}{Properties}{NewLine}")
				.WriteTo.TextWriter(messages)
				//.WriteTo.Observers(events => events
				//     .Do(evt => {
				//         Console.WriteLine($"Observed event {evt}");
				//     })
				//     .Subscribe())
				.CreateLogger();
			using (LogContext.PushProperty("Oka", "uno"))
			{
				log.Information("Hello, Serilog!");
				using (LogContext.PushProperty("Oka", "dos"))
					log.Error("This is an error");
				log.Information("Hello, Serilog 2!");
			}

			Log.Logger = log;
			Log.Information("The global logger has been configured");

			var per = new Person("Oscar", 37)
			{
				Age = 37,
				Name = "Oscar"
			};
			log.Information("Person '{@Person}' created", per);

			Console.WriteLine("Finished.");
			Console.ReadLine();
		}
		static void Main2(string[] args)
		{
			Console.WriteLine("Starting ...");
			var ran = new Random((int)DateTime.Now.Ticks);

			var tasks = new List<Task<string>>();
			for (int i = 0; i < 3; i++)
			{
				tasks.Add(TaskManager.StartNew(() =>
				{
					return Printer.GetPrinted(() =>
					{
						RunTask(ran);
					});
				}));
			}
			Task.WaitAll(tasks.ToArray());

			foreach (var task in tasks)
			{
				Console.WriteLine($"");
				Console.WriteLine($"Task result:\r\n{task.Result}");
			}

			Console.WriteLine("Finished.");
			Console.ReadLine();
		}
		static void RunTask(Random ran)
		{
			Printer.WriteLine($"Task '{Thread.CurrentThread.ManagedThreadId}' started.");
			var time = ran.Next(100, 2000);
			Printer.WriteLine($"Task '{Thread.CurrentThread.ManagedThreadId}' sleep 1 for '{time}ms'.");
			Thread.Sleep(time);
			time = ran.Next(100, 2000);
			Printer.WriteLine($"Task '{Thread.CurrentThread.ManagedThreadId}' sleep 2 for '{time}ms'.");
			Thread.Sleep(time);
			time = ran.Next(100, 2000);
			Printer.WriteLine($"Task '{Thread.CurrentThread.ManagedThreadId}' sleep 3 for '{time}ms'.");
			Thread.Sleep(time);
			Printer.WriteLine($"Task '{Thread.CurrentThread.ManagedThreadId}' finished.");
		}
	}
	public class Person
	{
		public Person(string name, int age)
		{
			log.LogInformation("Creating Person with name '{Name}' and age '{Age}'", name, age);
			Name = name;
			Age = age;
		}
		Microsoft.Extensions.Logging.ILogger log = Factory.Get<ILoggerFactory>().CreateLogger<Person>();
		public int Age { get; set; }
		public string Name { get; set; }

		public void DoWork()
		{
			log.LogInformation("Doing work for person {@Person}", this);
			var tasks = new List<Task>();
			using (log.BeginScope("Scope {ScopeId}", 1))
			{
				log.LogInformation("Inside {InsideCode}", 1);
				tasks.Add(TaskManager.StartNew(() =>
				{
					Thread.Sleep(1000);
					log.LogInformation("Inside {InsideCode}-Task", 1);
				}));
				//using (log.BeginScope("Scope {ScopeId}", 2))
				using (log.BeginScope(2))
				{
					log.LogInformation("Inside {InsideCode}", 2);
					tasks.Add(TaskManager.StartNew(() =>
					{
						Thread.Sleep(2000);
						log.LogInformation("Inside {InsideCode}-Task", 2);
					}));
				}
			}
			log.LogInformation("Task running ...");
			Task.WaitAll(tasks.ToArray());
		}
	}
	public class Mog
	{
		public Mog()
		{

		}
		ILog log = LogManager.Create<Mog>();
		public void Write(string msg)
		{

		}
	}
	public enum MogLevel
	{
		Trace = 0,
		Debug = 1,
		Information = 2,
		Warning = 3,
		Error = 4,
		Critical = 5,
		None = 6
	}
	public class MogEntry
	{
		public DateTime Timestamp { get; set; }
		public MogLevel Level { get; set; }
		public string Message { get; set; }
		public int EventId { get; set; }
		public Dictionary<string, object> Properties { get; set; }
	}
	public interface IMog
	{
		void Trace(string msg);
		void Debug(string msg);

		bool InfoEnabled { get; }
		void Info(string msg);
		void Info(string msg, Exception ex);
		void Info(MogEntry entry);


		void Warn(string msg);
		void Error(string msg);
		void Critical(string msg);
	}
}