namespace Fuxion.Licensing;

public class LicenseValidationException : FuxionException
{
	public LicenseValidationException(string message) : base(message) { }
}