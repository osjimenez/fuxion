using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
	 .ConfigureServices((context, services) =>
	 {
	 })
	 .UseWindowsService();
	 
host.Build().Run();