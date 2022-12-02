using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Telerik.Windows.Controls;

namespace Fuxion.Shell;

class PanelInstance : IDisposable
{
	public PanelInstance(IPanelDescriptor descriptor, IPanel panel, FrameworkElement view, RadPane radPane, IServiceScope serviceScope)
	{
		this.serviceScope = serviceScope;
		Descriptor = descriptor;
		Panel = panel;
		View = view;
		RadPane = radPane;
	}
	readonly IServiceScope serviceScope;
	public IPanelDescriptor Descriptor { get; }
	public IPanel Panel { get; }
	public FrameworkElement View { get; }
	public RadPane RadPane { get; }
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