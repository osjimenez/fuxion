namespace Fuxion.Licensing.Test.Mocks;

using System.Text.Json.Serialization;

public class LicenseMock : License
{
	public LicenseMock()
	{
		ExpirationUtcTime = new TimeLimitLicenseConstraint(DateTime.UtcNow.AddYears(1));
		DeactivationUtcTime = new TimeLimitLicenseConstraint(DateTime.UtcNow.AddMonths(1));
		HardwareId = new KeyModelLicenseConstraint(null);
		ProductId = new KeyModelLicenseConstraint(null);
	}
	[JsonConstructor]
	public LicenseMock(KeyModelLicenseConstraint hardwareId, KeyModelLicenseConstraint productId, TimeLimitLicenseConstraint deactivationUtcTime, TimeLimitLicenseConstraint expirationUtcTime) : this()
	{
		HardwareId = hardwareId;
		ProductId = productId;
		ExpirationUtcTime = expirationUtcTime;
		DeactivationUtcTime = deactivationUtcTime;
	}
	public TimeLimitLicenseConstraint DeactivationUtcTime { get; private set; }
	public TimeLimitLicenseConstraint ExpirationUtcTime { get; private set; }
	[JsonInclude]
	public KeyModelLicenseConstraint HardwareId { get; private set; }
	[JsonInclude]
	public KeyModelLicenseConstraint ProductId { get; private set; }
	protected override bool Validate(out string validationMessage)
	{
		var res = base.Validate(out validationMessage);
		res = res && ProductId.Validate(out validationMessage, Const.PRODUCT_ID);
		res = res && HardwareId.Validate(out validationMessage, Const.HARDWARE_ID);
		res = res && DeactivationUtcTime.Validate(out validationMessage);
		res = res && ExpirationUtcTime.Validate(out validationMessage);
		return res;
	}
	internal void SetHarwareId(string hardwareId) => HardwareId = new KeyModelLicenseConstraint(hardwareId);
	internal void SetProductId(string productId) => ProductId = new KeyModelLicenseConstraint(productId);
}