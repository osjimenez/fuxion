using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ordinem.Calendar.Application;
using Ordinem.Calendar.Domain;
using Ordinem.Calendar.Projection;
using Ordinem.Tasks.Service.AutoMapper;

namespace Ordinem.Calendar.Service
{
	public class CalendarStartup
	{
		public static void Main(string[] args)
		{
			Console.Title = "Calendar";
			Host
				.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(builder =>
				{
					builder.UseStartup<CalendarStartup>();
				})
				.Build()
				.Run();
		}

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers()
				.AddNewtonsoftJson()
				.AddFuxionControllers();

			services.AddAutoMapper()
				.AddProfile<AppointmentProfile>();

			services.AddFuxion(_ => _
				// Infrastructure
				.RabbitMQ(
					connectionHost: "localhost",
					connectionRetryCount: 5,
					exchangeName: "Ordinem.Bus",
					queueName: "Ordinem.Calendar.Service",
					queueRetryCount: 5,
					builder: out var rabbitMQ)
				.EventStore(
					hostName: "localhost",
					port: 1113,
					username: "admin",
					password: "changeit",
					builder: out var eventStore)
				.InMemoryEventStorage(
					dumpFilePath: "events~.json",
					builder: out var inMemoryEventStorage)
				.InMemorySnapshotStorage(
					dumpFilePath: "snapshots~.json",
					builder: out var inMemorySnapshotStorage)
				.EntityFrameworkSqlServer<CalendarDbContext>(
					//dataSource: new[] { "crono", "cronus" }.Contains(Environment.MachineName.ToLower()) ? "." : @".\sqlexpress",
					dataSource: ".",
					initialCatalog: "Ordinem.Calendar.Service",
					builder: out var entityFrameworkSqlServer)
				//.EntityFrameworkInMemory<CalendarDbContext>(
				//	databaseName: "Ordinem.Calendar",
				//	builder: out var entityFrameworkInMemory)
				// Events
				.Events(__ => __
					.HandlersFromAssemblyOf<Ordinem_Calendar_Application>()
					.HandlersFromAssemblyOf<Ordinem_Calendar_Projection>())
				//.Subscribe<AppointmentCreatedEvent>(eventSubscriber: rabbitMQ))
				// Commands
				.Commands(__ => __
					.HandlersFromAssemblyOf<Ordinem_Calendar_Application>())
				// Aggregates
				.Aggregate<Appointment, AppointmentFactory, AppointmentSnapshot>(
					eventStorage: inMemoryEventStorage,
					eventPublisher: rabbitMQ,
					snapshotStorage: inMemorySnapshotStorage,
					snapshotFrecuency: 3));
		}
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
		{
			if (env.IsDevelopment())
				app.UseDeveloperExceptionPage();
			else
				app.UseHsts();

			app.UseHttpsRedirection();
			//loggerFactory.AddLog4Net();
			var logger = loggerFactory.CreateLogger(typeof(CalendarStartup));
			logger.LogInformation("Testing logging ...");
			//app.UseMvc();

			using (var serviceScope = app.ApplicationServices.CreateScope())
			{
				var database = serviceScope.ServiceProvider.GetRequiredService<CalendarDbContext>().Database;
				if (!database.IsInMemory())
					database.Migrate();
			}
		}
	}
}
