namespace Fuxion.Shell;

public interface IMenu
{
	object Header { get; }
	void   OnClick();
}