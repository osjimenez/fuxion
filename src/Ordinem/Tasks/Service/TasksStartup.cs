using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ordinem.Tasks.Application;
using Ordinem.Tasks.Domain;
using Ordinem.Tasks.Projection;
using Ordinem.Tasks.Projection.EventHandlers;
using Ordinem.Tasks.Service.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ordinem.Tasks.Service
{
	public class TasksStartup
	{
		public static void Main(string[] args)
		{
			Console.Title = "Tasks";
			Host
				.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(builder =>
				{
					builder.UseStartup<TasksStartup>();
				})
				.Build()
				.Run();
		}

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvcCore()
				.AddNewtonsoftJson()
				.AddFuxionControllers();

			services.AddAutoMapper()
				.AddProfile<ToDoTaskProfile>();

			services.AddFuxion(_ => _
				// Infrastructure
				.RabbitMQ(
					connectionHost: "localhost",
					connectionRetryCount: 5,
					exchangeName: "Ordinem.Bus",
					queueName: "Ordinem.Tasks.Service",
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
				.EntityFrameworkSqlServer<TasksDbContext>(
					dataSource: new[] { "crono", "cronus" }.Contains(Environment.MachineName.ToLower()) ? "." : @".\sqlexpress",
					initialCatalog: "Ordinem.Tasks.Service",
					builder: out var entityFrameworkSqlServer)
				//.EntityFrameworkInMemory<TasksDbContext>(
				//	databaseName: "Ordinem.Tasks",
				//	builder: out var entityFrameworkInMemory)
				// Events
				.Events(__ => __
					.HandlersFromAssemblyOf<Ordinem_Tasks_Application>()
					.HandlersFromAssemblyOf<Ordinem_Tasks_Projection>())
				//.Subscribe<ToDoTaskCreatedEvent>(eventSubscriber: rabbitMQ))
				// Commands
				.Commands(__ => __
					.HandlersFromAssemblyOf<Ordinem_Tasks_Application>())
				// Aggregates
				.Aggregate<ToDoTask, ToDoTaskFactory, ToDoTaskSnapshot>(
					eventStorage: inMemoryEventStorage,
					eventPublisher: rabbitMQ,
					snapshotStorage: inMemorySnapshotStorage,
					snapshotFrecuency: 3));
		}
		public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			if (env.IsDevelopment())
				app.UseDeveloperExceptionPage();
			else
				app.UseHsts();

			app.UseHttpsRedirection();
			//loggerFactory.AddLog4Net();
			var logger = loggerFactory.CreateLogger(typeof(TasksStartup));
			logger.LogInformation("Testing logging ...");
			app.UseMvc();

			using (var serviceScope = app.ApplicationServices.CreateScope())
			{
				var database = serviceScope.ServiceProvider.GetRequiredService<TasksDbContext>().Database;
				if (!database.IsInMemory())
					database.Migrate();
			}
		}		
	}
}
