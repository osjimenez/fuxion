using Fuxion.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Licensing.Test;

[Collection("Licensing")]
public class LicenseContainerTest : BaseTest<LicenseContainerTest>
{
	public LicenseContainerTest(ITestOutputHelper output) : base(output) { }
	[Fact(DisplayName = "LicenseContainer - Comment")]
	public void LicenseContainer_Comment()
	{
		var hardwareId = Guid.NewGuid().ToString();
		var productId = Guid.NewGuid().ToString();
		var lic = new LicenseMock();
		lic.SetHarwareId(hardwareId);
		lic.SetProductId(productId);
		var con = LicenseContainer.Sign(lic, Const.FULL_KEY);
		con.Comment = "Original comment";
		Output.WriteLine("Original ToJson:");
		Output.WriteLine(con.ToJson());
		Assert.True(con.VerifySignature(Const.PUBLIC_KEY));
		con.Comment = "Change comment";
		Assert.True(con.VerifySignature(Const.PUBLIC_KEY));
		Output.WriteLine("Changed ToJson:");
		Output.WriteLine(con.ToJson());
	}
	[Fact(DisplayName = "LicenseContainer - Serialization")]
	public void LicenseContainer_Serialization()
	{
		var hardwareId = Guid.NewGuid().ToString();
		var productId = Guid.NewGuid().ToString();
		var lic = new LicenseMock();
		lic.SetHarwareId(hardwareId);
		lic.SetProductId(productId);
		var con = new LicenseContainer("signature", lic);
		Output.WriteLine("ToJson:");
		var json = con.ToJson();
		Output.WriteLine(json);
		Output.WriteLine("FromJson:");
		var con2 = json.FromJson<LicenseContainer>()!;
		var json2 = con2.ToJson();
		Output.WriteLine(json2);
		Assert.Equal(json, json2);
		Assert.True(con2.Is<LicenseMock>());
		var lic2 = con2.As<LicenseMock>()!;
		Assert.NotNull(lic2);
		Assert.Equal(lic.HardwareId.Key, lic2.HardwareId.Key);
		Assert.Equal(lic.ProductId.Key, lic2.ProductId.Key);

		//Assert.Equal(con2.LicenseAs<LicenseMock>(), lic);
		var time = new Random((int)DateTime.Now.Ticks).Next(500, 1500);
		Output.WriteLine("Time: " + time);
		Thread.Sleep(time);
		Output.WriteLine("FromJson timed:");
		var con3 = json.FromJson<LicenseContainer>()!;
		var json3 = con3.ToJson();
		Output.WriteLine(json3);
		Assert.Equal(json, json3);
		Assert.Equal(json2, json3);
		Assert.True(con3.Is<LicenseMock>());
		var lic3 = con3.As<LicenseMock>()!;
		Assert.NotNull(lic3);
		Assert.Equal(lic.HardwareId.Key, lic3.HardwareId.Key);
		Assert.Equal(lic.ProductId.Key, lic3.ProductId.Key);

		//Assert.Equal(con3.LicenseAs<LicenseMock>(), lic);
	}
	[Fact(DisplayName = "LicenseContainer - Signing")]
	public void LicenseContainer_Signing()
	{
		var hardwareId = Guid.NewGuid().ToString();
		var productId = Guid.NewGuid().ToString();
		var lic = new LicenseMock();
		lic.SetHarwareId(hardwareId);
		lic.SetProductId(productId);
		var con = LicenseContainer.Sign(lic, Const.FULL_KEY);
		Output.WriteLine("ToJson:");
		Output.WriteLine(con.ToJson());
		Output.WriteLine("License.ToJson:");
		Output.WriteLine(con.RawLicense.ToJson());
		Assert.True(con.VerifySignature(Const.PUBLIC_KEY));
	}
}