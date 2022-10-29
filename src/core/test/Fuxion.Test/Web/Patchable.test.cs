using Fuxion.Web;
using Microsoft.CSharp.RuntimeBinder;

namespace Fuxion.Test.Web;

public class PatchableTest
{
	[Fact(DisplayName = "Patchable - Cast")]
	public void Cast()
	{
		dynamic dyn = new Patchable<ToPatch>();
		//dyn.Id = Guid.Parse("{7F27735C-FDE1-4141-985A-214502599C63}");
		dyn.Id = "{7F27735C-FDE1-4141-985A-214502599C63}";
		var delta = dyn as Patchable<ToPatch>;
		var id    = delta?.Get<Guid>("Id");
		Assert.Equal(Guid.Parse("{7F27735C-FDE1-4141-985A-214502599C63}"), id);
	}
	[Fact(DisplayName = "Patchable - Get")]
	public void Get()
	{
		dynamic dyn = new Patchable<ToPatch>();
		dyn.Integer = 111;
		var delta = dyn as Patchable<ToPatch>;
		Assert.Equal(111, delta?.Get<int>("Integer"));
	}
	[Fact(DisplayName = "Patchable - Indexer")]
	public void Indexer()
	{
		dynamic dyn = new Patchable<ToPatch>();
		dyn.Integer = 111;
		var delta = dyn as Patchable<ToPatch>;
		Assert.True(delta?.Has("Integer"));
		Assert.False(delta?.Has("Integer2"));
		Assert.Equal(111, delta?.Get<int>("Integer"));
	}
	[Fact(DisplayName = "Patchable - List")]
	public void List()
	{
		var toPatch = new ToPatch
		{
			Integer = 123, String = "TEST"
		};
		dynamic dyn = new Patchable<ToPatch>();
		dyn.List = new List<int>();
		dyn.List.Add(1);
		dyn.Patch(toPatch);
		Assert.NotEmpty(toPatch.List);
	}
	[Fact(DisplayName = "Patchable - NonExistingPropertiesMode")]
	public void NonExistingProperties()
	{
		// Create a Patchable
		dynamic dyn = new Patchable<ToPatch>();
		// Set non existing property
		Assert.Throws<RuntimeBinderException>(() =>
		{
			dyn.Integer        = 123;
			dyn.DerivedInteger = 123;
		});
		dyn.NonExistingPropertiesMode = NonExistingPropertiesMode.OnlySet;
		dyn.Integer                   = 123;
		dyn.DerivedInteger            = 123;

		// Path a derived class
		var derived = new DerivedToPatch();
		(dyn as Patchable<ToPatch>)?.ToPatchable<DerivedToPatch>().Patch(derived);
		Assert.Equal(123, derived.Integer);
		Assert.Equal(123, derived.DerivedInteger);

		// Get non existing property
		var  delta = (Patchable<ToPatch>)dyn;
		int? res, derivedRed;
		Assert.Throws<RuntimeBinderException>(() =>
		{
			res        = delta.Get<int>("Integer");
			derivedRed = delta.Get<int>("DerivedInteger");
		});
		dyn.NonExistingPropertiesMode   = NonExistingPropertiesMode.GetAndSet;
		delta.NonExistingPropertiesMode = NonExistingPropertiesMode.GetAndSet;
		res                             = delta?.Get<int>("Integer");
		derivedRed                      = delta?.Get<int>("DerivedInteger");
		Assert.Equal(123, res);
		Assert.Equal(123, derivedRed);
	}
	[Fact(DisplayName = "Patchable - Patch")]
	public void Patch()
	{
		var toPatch = new ToPatch
		{
			Integer = 123, String = "TEST"
		};
		dynamic dyn = new Patchable<ToPatch>();
		dyn.Integer = 111;

		// Serialize and deserialize to simulate network service passthrough
		var ser = ((Patchable<ToPatch>)dyn).ToJson().FromJson<Patchable<ToPatch>>();
		Assert.NotNull(ser);
		ser.Patch(toPatch);
		Assert.Equal(111, toPatch.Integer);
	}
}

public class ToPatch
{
	public int       Integer { get; set; }
	public string?   String  { get; set; }
	public Guid      Id      { get; set; }
	public List<int> List    { get; set; } = new();
}

public class DerivedToPatch : ToPatch
{
	public int DerivedInteger { get; set; }
}