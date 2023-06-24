using Fuxion.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Licensing.Test;

using static LicenseContainer;
using static LicensingManager;

[Collection("Licensing")]
public class LicensingManagerTest : BaseTest<LicensingManagerTest>
{
	public LicensingManagerTest(ITestOutputHelper output) : base(output)
	{
		services = new();
		services.AddSingleton<ILicenseProvider>(new LicenseProviderMock());
		services.AddSingleton<ILicenseStore>(new LicenseStoreMock());
		services.AddSingleton<IHardwareIdProvider>(new HardwareIdProviderMock());
		Singleton.AddOrSkip<ITimeProvider>(new MockTimeProvider());
		services.AddSingleton(Singleton.Get<ITimeProvider>());
		services.AddTransient<LicensingManager>();
	}
	readonly ServiceCollection services;
	[Theory(DisplayName = "LicensingManager - Validate")]
#nullable disable
	[InlineData("Match", Const.HARDWARE_ID, Const.PRODUCT_ID, 10, true)]
	[InlineData("Deactivated", Const.HARDWARE_ID, Const.PRODUCT_ID, 50, false)]
	[InlineData("Expired", Const.HARDWARE_ID, Const.PRODUCT_ID, 400, false)]
	[InlineData("Product not match", Const.HARDWARE_ID, "{105B337E-EBCE-48EA-87A7-852E3A699A98}", 10, false)]
	[InlineData("Hardware not match", "{AE2C70B9-6622-4341-81A0-10EAA078E7DF}", Const.PRODUCT_ID, 10, false)]
#nullable enable
	public void Validate(string _, string hardwareId, string productId, int offsetDays, bool expectedValidation)
	{
		var pro = services.BuildServiceProvider();
		// Crete LicensingManager
		var man = pro.GetRequiredService<LicensingManager>();
		// Remove all existing licenses
		foreach (var l in man.Store.Query().ToList()) Assert.True(man.Store.Remove(l), "Failed on remove license");
		// Create license with given parameters
		var lic = man.GetProvider().Request(new LicenseRequestMock(hardwareId, productId));
		man.Store.Add(lic);
		var tp = pro.GetRequiredService<ITimeProvider>() as MockTimeProvider;
		tp?.SetOffset(TimeSpan.FromDays(offsetDays));
		// Validate content
		if (expectedValidation)
			man.Validate<LicenseMock>(Const.PUBLIC_KEY, true);
		else
			try
			{
				man.Validate<LicenseMock>(Const.PUBLIC_KEY, true);
			} catch (LicenseValidationException lvex)
			{
				if (expectedValidation) Assert.False(true, lvex.Message);
			}
		tp?.SetOffset(TimeSpan.Zero);
	}
	[Fact(DisplayName = "LicensingManager - License request")]
	public void License_Request()
	{
		var pro = services.BuildServiceProvider();
		var lic = pro.GetRequiredService<LicensingManager>().GetProvider().Request(new LicenseRequestMock(Const.HARDWARE_ID, Const.PRODUCT_ID));
		Output.WriteLine("Created license:");
		Output.WriteLine(new[] {
			lic
		}.SerializeToJson());
	}
	[Fact(DisplayName = "LicensingManager - License generation")]
	public void Licensing_GenerateKey()
	{
		GenerateKey(out var fullKey, out var publicKey);
		Output.WriteLine("Full key:\r\n" + fullKey);
		Output.WriteLine("");
		Output.WriteLine("Public key:\r\n" + publicKey);
	}
}

public class LicenseRequestMock : LicenseRequest
{
	public LicenseRequestMock(string hardwareId, string productId)
	{
		HardwareId = hardwareId;
		ProductId = productId;
	}
	public string HardwareId { get; set; }
	public string ProductId { get; set; }
}

public class HardwareIdProviderMock : IHardwareIdProvider
{
	public Guid GetId() => Guid.Parse(Const.HARDWARE_ID);
}

public class LicenseProviderMock : ILicenseProvider
{
	public LicenseContainer Request(LicenseRequest request)
	{
		if (request is LicenseRequestMock req)
		{
			var lic = new LicenseMock();
			lic.SetHarwareId(req.HardwareId);
			lic.SetProductId(req.ProductId);
			return Sign(lic, Const.FULL_KEY);
		}
		throw new NotSupportedException($"License request of type '{request.GetType().Name}' is not supported by this provider");
	}
	public LicenseContainer Refresh(LicenseContainer oldLicense) => throw new NotImplementedException();
}