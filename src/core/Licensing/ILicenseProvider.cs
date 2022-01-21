namespace Fuxion.Licensing;

public interface ILicenseProvider
{
	LicenseContainer Request(LicenseRequest request);
	LicenseContainer Refresh(LicenseContainer oldLicense);
}