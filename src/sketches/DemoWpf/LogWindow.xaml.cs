using Fuxion.Logging;
using Fuxion.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MongoDB.Driver;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;

namespace DemoWpf
{
    public partial class LogWindow : Window
    {
        ILog log = LogManager.Create<LogWindow>();
        public LogWindow()
        {
            InitializeComponent();

            // Levels

            // Exceptions

            // Scopes

            log4net.GlobalContext.Properties["NombrePropio"] = "Oskitar";
            log4net.ThreadContext.Stacks["Metodo"].Push("Main");
            log4net.ThreadContext.Properties["NombrePropio"] = "OskitarThread";
            log.Info("Hola");
            TaskManager.StartNew(() => {
                var pro = log4net.ThreadContext.Properties["NombrePropio"];
                log4net.ThreadContext.Properties["NombrePropio"] = "OskitarThread2";
                log4net.ThreadContext.Stacks["Metodo"].Push("Task");
                log4net.ThreadContext.Stacks["Metodo"].Push("Re-Task");
                log.Info("Hola");
            });

			//IServiceCollection sc;
			//sc.AddLogging(_ => _
			//	.AddConsole()
			//	.AddDebug());

   //         ILoggerFactory loggerFactory = new LoggerFactory()
   //             .AddConsole(true)
   //             .AddDebug();
   //         ILogger logger = loggerFactory.CreateLogger<LogWindow>();
   //         using (logger.BeginScope("SCOPE", 1, DateTime.Now))
   //         {
   //             logger.LogInformation(
   //               "This is a test of the emergency broadcast system.");
   //         }
   //         using (logger.BeginScope<string>("SCOPE2"))
   //         {
   //             logger.LogInformation(
   //               "This is a test of the emergency broadcast system.");
   //         }
   //         logger.LogInformation("Otro");
   //         ConsoleLoggerSettings cls = new ConsoleLoggerSettings();
   //         cls.Switches.Add("o", LogLevel.Information);
        }

        //string mongodPath = @"G:\Dev\Fuxion\MongoDB\bin\mongod.exe";
        //string mongodumpPath = @"G:\Dev\Fuxion\MongoDB\bin\mongodump.exe";
        //string mongorestorePath = @"G:\Dev\Fuxion\MongoDB\bin\mongorestore.exe";

        private void Log_Click(object sender, RoutedEventArgs e)
        {
            log.Info("Mas");
        }
        private void DB_Click(object sender, RoutedEventArgs e)
        {
            // Open database (or create if not exits)
            using (var db = new LiteDatabase(@"Log2/MyData.db"))
            {
                // Get customer collection
                var customers = db.GetCollection<Customer>("customers");

                Stopwatch sw = new Stopwatch();
                sw.Start();

                for (int i = 1; i < 100000; i++)
                {
                    if (i % 10000 == 0)
                        log.Info(i + " => " + sw.Elapsed);
                    customers.Insert(new Customer
                    {
                        Id = i,
                        Name = i + " Customer " + i,
                        Phones = new string[] { "1000-0000", "2000-0000" },
                        IsActive = true
                    });
                }
                sw.Stop();
                MessageBox.Show("TOTAL TIME: " + sw.Elapsed);

                //// Create your new customer instance
                //var customer = new Customer
                //{
                //    Name = "John Doe",
                //    Phones = new string[] { "1000-0000", "2000-0000" },
                //    IsActive = true
                //};

                //// Insert new customer document (Id will be auto-incremented)
                //customers.Insert(customer);

                //// Update a document inside a collection
                //customer.Name = "Joana Doe";

                //customers.Update(customer);

                //// Index document using a document property
                //customers.EnsureIndex(x => x.Name);

                //// Use Linq to query documents
                //// Property 'Phones.p' was not mapped into BsonDocument.
                var results = customers.Find(x => x.Name.StartsWith("8"));

                Debug.WriteLine("");
            }
        }
        private async void Mongo_Click(object sender, RoutedEventArgs e)
        {
            // To directly connect to a single MongoDB server
            // (this will not auto-discover the primary even if it's a member of a replica set)
            //var client = new MongoClient();

            // or use a connection string
            var client = new MongoClient("mongodb://localhost:27017");

            // or, to connect to a replica set, with auto-discovery of the primary, supply a seed list of members
            //var client = new MongoClient("mongodb://localhost:27017,localhost:27018,localhost:27019");

            var db = client.GetDatabase("test");
            var cols = await db.ListCollectionsAsync();
            var cols2 = await cols.ToListAsync();
            foreach (var item in cols2)
            {
                Console.WriteLine(item.ToString());
            }
            var col = db.GetCollection<Customer>("customers");


            Stopwatch sw = new Stopwatch();
            sw.Start();

            //for (int i = 1; i < 100000; i++)
            //{
            //    if (i % 10000 == 0)
            //        log.Info(i + " => " + sw.Elapsed);
            //    col.InsertOne(new Customer
            //    {
            //        Id = i,
            //        Name = i + " Customer " + i,
            //        Phones = new string[] { "1000-0000", "2000-0000" },
            //        IsActive = true
            //    });
            //}
            sw.Stop();
            MessageBox.Show("TOTAL TIME: " + sw.Elapsed);

            var results = col.AsQueryable().Where(x => x.Name.StartsWith("8") && x.Phones.Any(p => p.StartsWith("1")));


            Debug.WriteLine(results.Count());
        }
    }
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string[] Phones { get; set; }
        public bool IsActive { get; set; }
    }

    public class Pro : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            throw new NotImplementedException();
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
