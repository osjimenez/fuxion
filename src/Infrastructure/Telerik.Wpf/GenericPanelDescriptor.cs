namespace Fuxion.Telerik_.Wpf;

class GenericPanelDescriptor(string name, Type viewType, PanelPosition defaultPosition, bool removeOnHide, bool isPinned) : IPanelDescriptor
{
	public Type ViewType { get; } = viewType;
	public PanelPosition DefaultPosition { get; } = defaultPosition;
	public PanelName Name { get; } = PanelName.Parse(name);
	public bool RemoveOnHide { get; } = removeOnHide;
	public bool IsPinned { get; } = isPinned;
}