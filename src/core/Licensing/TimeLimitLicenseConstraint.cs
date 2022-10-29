namespace Fuxion.Licensing;

public class TimeLimitLicenseConstraint : LicenseConstraint
{
	public TimeLimitLicenseConstraint(DateTime value) => Value = value;
	public DateTime Value { get; }
	public new bool Validate(out string validationMessage)
	{
		var res = base.Validate(out validationMessage);
		var tp  = Singleton.Get<ITimeProvider>();
		if (tp.UtcNow() > Value)
		{
			validationMessage = "Time limit expired";
			res               = false;
		}
		return res;
	}
}