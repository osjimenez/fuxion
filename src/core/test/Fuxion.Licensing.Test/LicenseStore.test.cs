namespace Fuxion.Licensing.Test;

using Fuxion.Licensing.Test.Mocks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

[Collection("Licensing")]
public class LicenseStoreTest
{
	public LicenseStoreTest(ITestOutputHelper output)
	{
		this.output = output;
		services = new ServiceCollection();
		services.AddSingleton<ILicenseProvider>(new LicenseProviderMock());
		services.AddSingleton<ILicenseStore>(new LicenseStoreMock());
		services.AddSingleton<IHardwareIdProvider>(new HardwareIdProviderMock());
		Singleton.AddOrSkip<ITimeProvider>(new MockTimeProvider());
		services.AddSingleton(Singleton.Get<ITimeProvider>());
		services.AddTransient<LicensingManager>();
	}
	private readonly ServiceCollection services;
	private readonly ITestOutputHelper output;
	[Theory(DisplayName = "LicenseStore - OnlyValidLicenses")]
#nullable disable
	[InlineData(new object[] { "Match", Const.HARDWARE_ID, Const.PRODUCT_ID, 10, true })]
	[InlineData(new object[] { "Deactivated", Const.HARDWARE_ID, Const.PRODUCT_ID, 50, false })]
	[InlineData(new object[] { "Expired", Const.HARDWARE_ID, Const.PRODUCT_ID, 400, false })]
	[InlineData(new object[] { "Product not match", Const.HARDWARE_ID, "{105B337E-EBCE-48EA-87A7-852E3A699A98}", 10, false })]
	[InlineData(new object[] { "Hardware not match", "{AE2C70B9-6622-4341-81A0-10EAA078E7DF}", Const.PRODUCT_ID, 10, false })]
#nullable enable
	public void OnlyValidLicenses(string _, string hardwareId, string productId, int offsetDays, bool expectedValidation)
	{
		//TODO - Test this https://github.com/tonerdo/pose for mock DateTime.Now


		var pro = services.BuildServiceProvider();
		// Crete LicensingManager
		var man = pro.GetRequiredService<LicensingManager>();
		// Remove all existing licenses
		foreach (var l in man.Store.Query().ToList())
			Assert.True(man.Store.Remove(l), "Failed on remove license");
		// Create license with given parameters
		var lic = man.GetProvider().Request(new LicenseRequestMock(hardwareId, productId));
		man.Store.Add(lic);
		var tp = (MockTimeProvider)pro.GetRequiredService<ITimeProvider>();
		output.WriteLine($"Current offset is '{tp.Offset}'");
		tp.SetOffset(TimeSpan.FromDays(offsetDays));
		output.WriteLine($"After set offset is '{tp.Offset}'");
		Assert.True(man.Store.Query().OnlyValidOfType<LicenseMock>(Const.PUBLIC_KEY).Count() == (expectedValidation ? 1 : 0));
		tp.SetOffset(TimeSpan.Zero);
		output.WriteLine($"After reset offset is '{tp.Offset}'");
	}
}