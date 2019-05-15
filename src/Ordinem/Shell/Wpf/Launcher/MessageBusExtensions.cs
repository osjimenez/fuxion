using Fuxion.Reflection;
using Fuxion.Shell;
using Ordinem.Shell.Wpf.Launcher.Views;
using ReactiveUI;

namespace Ordinem.Shell.Wpf.Launcher
{
	public static class MessageBusExtensions
	{
		public static void OpenAbout(this IMessageBus me)
		{
			me.OpenPanel(PanelName.Parse(typeof(AboutPanel).GetTypeKey()));
		}
	}
}
