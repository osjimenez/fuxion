using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Diagnostics;
using System.Linq;

namespace Ordinem.Tasks.Projection
{
	public class TasksDbContext : DbContext
	{
		public TasksDbContext(DbContextOptions<TasksDbContext> options) : base(options)
		{
			Debug.WriteLine("***********************************");
		}
		public const string DEFAULT_SCHEMA = "Task";
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.ApplyConfiguration(new ToDoTaskTypeConfiguration());
		}

		public DbSet<ToDoTaskDpo> ToDoTasks { get; set; }
	}
	public class TasksDbContextFactory : IDesignTimeDbContextFactory<TasksDbContext>
	{
		public TasksDbContext CreateDbContext(string[] args)
		{
			var optionsBuilder = new DbContextOptionsBuilder<TasksDbContext>();
			string server = new[] {
						"crono",
						"cronus"
					}.Contains(Environment.MachineName.ToLower())
						? "."
						: @".\sqlexpress";
			optionsBuilder.UseSqlServer($@"Server={server};Database=Ordinem;Trusted_Connection=True");
			return new TasksDbContext(optionsBuilder.Options);
		}
	}
}
