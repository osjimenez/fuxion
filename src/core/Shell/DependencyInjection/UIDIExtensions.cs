using Fuxion.Reflection;
using Fuxion.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class UIDIExtensions
	{
		public static void AddMenu(this IServiceCollection me, object header, Action clickAction)
		{
			me.AddSingleton<IMenu>(new GenericMenu(header, clickAction));
		}
		public static void AddMenu<TMenu>(this IServiceCollection me) where TMenu : class, IMenu
		{
			me.AddSingleton<IMenu, TMenu>();
		}
		public static void AddPanel<TPanelView, TPanel>(this IServiceCollection me, PanelPosition defaultPosition = PanelPosition.Document, bool removeOnHide = true) 
			where TPanelView : FrameworkElement
			where TPanel : class, IPanel
		{
			me.AddTransient<TPanelView>();
			me.AddTransient<TPanel>();
			me.AddSingleton<IPanelDescriptor>(sp => new GenericPanelDescriptor(typeof(TPanel).GetTypeKey(), typeof(TPanelView), defaultPosition, removeOnHide));
		}
		public static void AddPanel<TPanel>(this IServiceCollection me, PanelPosition defaultPosition = PanelPosition.Document, bool removeOnHide = true) 
			where TPanel : class, IPanel
		{
			me.AddTransient<TPanel>();
			me.AddSingleton<IPanelDescriptor>(sp => new GenericPanelDescriptor(typeof(TPanel).GetTypeKey(), typeof(TPanel), defaultPosition, removeOnHide));
		}
	}
	public class GenericPanelDescriptor : IPanelDescriptor
	{
		public GenericPanelDescriptor(string name, Type viewType, PanelPosition defaultPosition, bool removeOnHide)
		{
			Name = PanelName.Parse(name);
			ViewType = viewType;
			DefaultPosition = defaultPosition;
			RemoveOnHide = removeOnHide;
		}
		public Type ViewType { get; }
		public PanelPosition DefaultPosition { get; }
		public PanelName Name { get; }
		public bool RemoveOnHide { get; }
	}
	public class GenericMenu : IMenu
	{
		public GenericMenu(object header, Action clickAction)
		{
			Header = header;
			this.clickAction = clickAction;
		}
		Action clickAction;
		public object Header { get; }
		public void OnClick() => clickAction();
	}
}
