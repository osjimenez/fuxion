namespace Fuxion.Licensing;

public interface IHardwareIdProvider
{
	Guid GetId();
}