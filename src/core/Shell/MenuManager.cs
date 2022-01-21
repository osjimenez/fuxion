namespace Fuxion.Shell;

using Microsoft.Extensions.DependencyInjection;
using Telerik.Windows.Controls;

public class MenuManager
{
	public MenuManager(IServiceProvider serviceProvider)
	{
		this.serviceProvider = serviceProvider;
		menus = serviceProvider.GetServices<IMenu>().ToList();
	}

	private readonly IServiceProvider serviceProvider;
	private readonly List<IMenu> menus;

	public void PopulateMenu(RadMenu menu)
	{
		foreach (var m in menus)
		{
			if (m.Header is RadMenuItem m2)
			{
				m2.Click += (_, __) => m.OnClick();
				menu.Items.Add(m2);
			}
			else
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
}