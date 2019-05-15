using Fuxion.Shell;
using Microsoft.Extensions.DependencyInjection;
using Ordinem.Shell.Wpf.Launcher.Views;
using ReactiveUI;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using Telerik.Windows.Controls;

namespace Ordinem.Shell.Wpf.Launcher
{
	public class LauncherModule : IModule
	{
		public void Initialize(IServiceProvider serviceProvider) { }
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
			lockMenu.Click += (_, __) => MessageBus.Current.Lock();
			menuItem.Items.Add(lockMenu);
			var unlockMenu = new RadMenuItem
			{
				Margin = margin,
				Icon = new Viewbox
				{
					Height = 24,
					Width = 24,
					Child = Application.Current.Resources["UnlockPath"] as Path
				},
				Header = "Unlock UI"
			};
			unlockMenu.Click += (_, __) => MessageBus.Current.Unlock();
			menuItem.Items.Add(unlockMenu);

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
			saveMenu.Click += (_, __) => MessageBus.Current.SaveLayout();
			menuItem.Items.Add(saveMenu);

			services.AddMenu(menuItem, () => { });
			services.AddMenu("About", () => MessageBus.Current.OpenAbout());

			services.AddPanel<AboutPanel>(defaultPosition: PanelPosition.DockedRight, isPinned: false);
		}
	}
}
