using System.Text.Json.Serialization;

namespace Fuxion.Licensing.Test;

public class LicenseMock : License
{
	public LicenseMock()
	{
		ExpirationUtcTime = new(DateTime.UtcNow.AddYears(1));
		DeactivationUtcTime = new(DateTime.UtcNow.AddMonths(1));
		HardwareId = new(null);
		ProductId = new(null);
	}
	[JsonConstructor]
	public LicenseMock(KeyModelLicenseConstraint hardwareId, KeyModelLicenseConstraint productId, TimeLimitLicenseConstraint deactivationUtcTime, TimeLimitLicenseConstraint expirationUtcTime) : this()
	{
		HardwareId = hardwareId;
		ProductId = productId;
		ExpirationUtcTime = expirationUtcTime;
		DeactivationUtcTime = deactivationUtcTime;
	}
	public TimeLimitLicenseConstraint DeactivationUtcTime { get; }
	public TimeLimitLicenseConstraint ExpirationUtcTime { get; }
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
	internal void SetHarwareId(string hardwareId) => HardwareId = new(hardwareId);
	internal void SetProductId(string productId) => ProductId = new(productId);
}