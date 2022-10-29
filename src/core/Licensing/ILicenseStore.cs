namespace Fuxion.Licensing;

public interface ILicenseStore
{
	event EventHandler<EventArgs<LicenseContainer>>? LicenseAdded;
	event EventHandler<EventArgs<LicenseContainer>>? LicenseRemoved;
	IQueryable<LicenseContainer>                     Query();
	void                                             Add(LicenseContainer    license);
	bool                                             Remove(LicenseContainer license);
}