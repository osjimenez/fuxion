using Fuxion.Shell;
using Fuxion.Shell.Messages;
using Ordinem.Shell.Wpf.Tasks.ViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;
using Fuxion.Reflection;
using Ordinem.Shell.Wpf.Launcher.Views;

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
