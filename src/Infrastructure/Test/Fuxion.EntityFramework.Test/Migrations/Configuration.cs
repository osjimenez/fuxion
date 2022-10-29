using System.Data.Entity.Migrations;

namespace Fuxion.EntityFramework.Test.Migrations;

public sealed class Configuration : DbMigrationsConfiguration<TestContext>
{
	public Configuration() => AutomaticMigrationsEnabled = false;
	protected override void Seed(TestContext context)
	{
		//  This method will be called after migrating to the latest version.

		//  You can use the DbSet<T>.AddOrUpdate() helper extension method 
		//  to avoid creating duplicate seed data.
	}
}