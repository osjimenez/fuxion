namespace Fuxion.EntityFramework.Test;

using Fuxion.Testing;
using System.Data.Common;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using Xunit;
using Xunit.Abstractions;

[Collection("Sequences")]
public class Sequences : BaseTest, IDisposable
{
	public Sequences(ITestOutputHelper output) : base(output)
	{
		// https://devblogs.microsoft.com/dotnet/announcing-entity-framework-6-3-preview-with-net-core-support/
		DbProviderFactories.RegisterFactory("System.Data.SqlClient", SqlClientFactory.Instance);

		con = new TestContext();
		if (!con.Database.Exists())
		{
			var mig = new DbMigrator(new Migrations.Configuration());
			mig.Update();
		}
	}
	public void Dispose()
	{
		con.Database.Delete();
		con.Dispose();
	}
	private readonly TestContext con;

	//[Fact(Skip = "Desactivado")]
	[Fact(DisplayName = "Sequences - Create and delete")]
	public void CreateAndDeleteSequence()
	{
		con.CreateSequence("T");
		con.DeleteSequence("T");
	}
	[Fact(DisplayName = "Sequences - Get value")]
	public void GetSequenceValue()
	{
		con.CreateSequence("T");
		var val = con.GetSequenceValue("T");
		Assert.Equal(1, val);
		val = con.GetSequenceValue("T");
		Assert.Equal(2, val);
		val = con.GetSequenceValue("T", false);
		Assert.Equal(2, val);
		con.DeleteSequence("T");
	}
	[Fact(DisplayName = "Sequences - Set value")]
	public void SetSequenceValue()
	{
		con.CreateSequence("T");
		var val = con.GetSequenceValue("T");
		Assert.Equal(1, val);
		con.SetSequenceValue("T", 12);
		val = con.GetSequenceValue("T", false);
		Assert.Equal(12, val);
		con.DeleteSequence("T");
	}
}