namespace Fuxion.Telerik_.Wpf;

class GenericMenu(object header, Action? clickAction = null) : IMenu
{
	public object Header { get; } = header;
	public void OnClick() => clickAction?.Invoke();
}