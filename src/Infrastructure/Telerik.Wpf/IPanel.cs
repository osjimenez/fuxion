using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Telerik.Windows.Controls;

namespace Fuxion.Telerik_.Wpf;

class PanelInstance(IPanelDescriptor descriptor, IPanel panel, FrameworkElement view, RadPane radPane, IServiceScope serviceScope) : IDisposable
{
	public IPanelDescriptor Descriptor { get; } = descriptor;
	public IPanel Panel { get; } = panel;
	public FrameworkElement View { get; } = view;
	public RadPane RadPane { get; } = radPane;
	public void Dispose() => serviceScope.Dispose();
}

public interface IPanelView
{
	IPanel Panel { get; }
}

public interface IPanel
{
	string Title { get; }
	string Header { get; }
}

public interface IInitializablePanel : IPanel
{
	void Initialize(PanelName name, Dictionary<string, object> args);
}

interface IPanelDescriptor
{
	PanelName Name { get; }
	Type ViewType { get; }
	PanelPosition DefaultPosition { get; }
	bool RemoveOnHide { get; }
	bool IsPinned { get; }
}