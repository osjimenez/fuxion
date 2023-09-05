namespace Fuxion.Licensing;

public class LicenseConstraint
{
	protected virtual bool Validate(out string validationMessage)
	{
		validationMessage = "Success";
		return true;
	}
}