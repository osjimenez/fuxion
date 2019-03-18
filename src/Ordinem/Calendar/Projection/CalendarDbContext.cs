using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Diagnostics;
using System.Linq;

namespace Ordinem.Calendar.Projection
{
	public class CalendarDbContext : DbContext
	{
		public CalendarDbContext(DbContextOptions<CalendarDbContext> options) : base(options)
		{
			Debug.WriteLine("***********************************");
		}
		public const string DEFAULT_SCHEMA = "Calendar";
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.ApplyConfiguration(new AppointmentTypeConfiguration());
		}

		public DbSet<AppointmentDpo> Appointments { get; set; }
	}
	public class CalendarDbContextFactory : IDesignTimeDbContextFactory<CalendarDbContext>
	{
		public CalendarDbContext CreateDbContext(string[] args)
		{
			var optionsBuilder = new DbContextOptionsBuilder<CalendarDbContext>();
			string server = new[] {
						"crono",
						"cronus"
					}.Contains(Environment.MachineName.ToLower())
						? "."
						: @".\sqlexpress";
			optionsBuilder.UseSqlServer($@"Server={server};Database=Ordinem;Trusted_Connection=True");
			return new CalendarDbContext(optionsBuilder.Options);
		}
	}
}
