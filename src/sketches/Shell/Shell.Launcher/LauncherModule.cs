using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using Fuxion.Shell.UIMessages;
using Fuxion.Shell.Views;
using Fuxion.Shell;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Telerik.Windows.Controls;
using Fuxion.Reflection;
namespace Shell.Launcher
{
	public class LauncherModule : IModule
	{
		public void Initialize() { }
		public void Register(IServiceCollection services)
		{
			var margin = new Thickness(6, 3, 6, 3);
			var menuItem = new RadMenuItem
			{
				Margin = margin,
				Padding = new Thickness(3),
				Icon = new Viewbox
				{
					Height = 24,
					Width = 24,
					Child = Application.Current.Resources["MenuPath"] as Path
				}
			};
			var gcMenu = new RadMenuItem
			{
				Margin = margin,
				Icon = new Viewbox
				{
					Height = 24,
					Width = 24,
					Child = Application.Current.Resources["MemoryPath"] as Path
				},
				Header = "Garbage collect",
			};
			gcMenu.Click += (_, __) => GC.Collect();
			menuItem.Items.Add(gcMenu);

			var lockMenu = new RadMenuItem
			{
				Margin = margin,
				Icon = new Viewbox
				{
					Height = 24,
					Width = 24,
					Child = Application.Current.Resources["LockPath"] as Path
				},
				Header = "Lock UI"
			};
			lockMenu.Click += (_, __) => MessageBus.Current.SendMessage(new ReadModeUIMEssage());
			menuItem.Items.Add(lockMenu);

			var saveMenu = new RadMenuItem
			{
				Margin = margin,
				Icon = new Viewbox
				{
					Height = 24,
					Width = 24,
					Child = Application.Current.Resources["SavePath"] as Path
				},
				Header = "Save layout"
			};
			saveMenu.Click += (_, __) => MessageBus.Current.SendMessage(new SaveLayoutUIMessage());
			menuItem.Items.Add(saveMenu);

			services.AddMenu(menuItem, () => { });
		}
	}
}
