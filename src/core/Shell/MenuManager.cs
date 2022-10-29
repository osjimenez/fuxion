using Microsoft.Extensions.DependencyInjection;
using Telerik.Windows.Controls;

namespace Fuxion.Shell;

public class MenuManager
{
	public MenuManager(IServiceProvider serviceProvider)
	{
		this.serviceProvider = serviceProvider;
		menus                = serviceProvider.GetServices<IMenu>().ToList();
	}
	readonly List<IMenu>      menus;
	readonly IServiceProvider serviceProvider;
	public void PopulateMenu(RadMenu menu)
	{
		foreach (var m in menus)
			if (m.Header is RadMenuItem m2)
			{
				m2.Click += (_, __) => m.OnClick();
				menu.Items.Add(m2);
			} else
			{
				var item = new RadMenuItem
				{
					Header = m.Header
				};
				item.Click += (_, __) => m.OnClick();
				menu.Items.Add(item);
			}
	}
}