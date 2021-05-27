namespace Microsoft.Extensions.DependencyInjection
{
	using Fuxion.Reflection;
	using Fuxion.Shell;
	using Fuxion.Shell.ViewModels;
	using Fuxion.Shell.Views;
	using System;
	using System.Linq;
	using System.Windows;

	public static class ShellDIExtensions
	{
		public static IFuxionBuilder Shell(this IFuxionBuilder me, Action<IShellBuilder> builder)
		{
			me.Services.AddSingleton<MenuManager>();
			me.Services.AddSingleton<DockingManager>();
			me.Services.AddSingleton<ShellWindowViewModel>();
			me.Services.AddSingleton<ShellWindow>();

			me.AddToPreRegistrationList(serviceProvider =>
			{
				foreach (var module in serviceProvider.GetServices<IModule>())
					module.PreRegister(me.Services);
			});
			me.AddToRegistrationList(serviceProvider =>
			{
				foreach (var dic in serviceProvider.GetServices<ShellResourceDictionary>())
					Application.Current.Resources.MergedDictionaries.Add(dic);
				foreach (var module in serviceProvider.GetServices<IModule>())
					module.Register(me.Services);
			});
			var shellBuilder = new ShellBuilder(me);
			me.AddToAutoActivateList<ShellWindow>(preAction: serviceProvider =>
			{
				foreach (var module in serviceProvider.GetServices<IModule>())
					module.Initialize(serviceProvider);
			}, postAction: (_, win) =>
			{
				shellBuilder.ShellWindow = win;
				if (shellBuilder.ShowWindow)
					win.Show();
			});

			builder(shellBuilder);

			return me;
		}
		public static IShellBuilder Module(this IShellBuilder me, Type type)
		{
			me.FuxionBuilder.Services.AddSingleton(typeof(IModule), type);
			return me;
		}
		public static IShellBuilder Module<T>(this IShellBuilder me) where T : class, IModule => Module(me, typeof(T));
		public static IShellBuilder ModulesFromAssemblyOf(this IShellBuilder me, Type typeOfAssembly)
		{
			foreach (var module in typeOfAssembly.Assembly.GetTypes().Where(t => typeof(IModule).IsAssignableFrom(t)))
				Module(me, module);
			return me;
		}
		public static IShellBuilder ModulesFromAssemblyOf<T>(this IShellBuilder me) => ModulesFromAssemblyOf(me, typeof(T));

		public static IShellBuilder ShellFactory(this IShellBuilder me, out Func<ShellWindow> shellFactory)
		{
			shellFactory = () => ((ShellBuilder)me).GetShellWindow() ?? throw new InvalidProgramException("ShellFactory error, Shell window is not ready jet");
			((ShellBuilder)me).ShowWindow = false;
			return me;
		}
		public static void AddResource<TResource>(this IServiceCollection me) where TResource : ShellResourceDictionary => me.AddSingleton<ShellResourceDictionary, TResource>();
		public static void AddMenu(this IServiceCollection me, object header, Action? clickAction = null) => me.AddSingleton<IMenu>(new GenericMenu(header, clickAction));
		public static void AddMenu<TMenu>(this IServiceCollection me) where TMenu : class, IMenu => me.AddSingleton<IMenu, TMenu>();
		public static void AddPanel<TPanelView, TPanel>(this IServiceCollection me, PanelPosition defaultPosition = PanelPosition.Document, bool removeOnHide = true, bool isPinned = true)
			where TPanelView : FrameworkElement
			where TPanel : class, IPanel
		{
			me.AddTransient<TPanelView>();
			me.AddTransient<TPanel>();
			me.AddSingleton<IPanelDescriptor>(sp => new GenericPanelDescriptor(typeof(TPanel).GetTypeKey(), typeof(TPanelView), defaultPosition, removeOnHide, isPinned));
		}
		public static void AddPanel<TPanel>(this IServiceCollection me, PanelPosition defaultPosition = PanelPosition.Document, bool removeOnHide = true, bool isPinned = true)
			where TPanel : class, IPanel
		{
			me.AddTransient<TPanel>();
			me.AddSingleton<IPanelDescriptor>(sp => new GenericPanelDescriptor(typeof(TPanel).GetTypeKey(), typeof(TPanel), defaultPosition, removeOnHide, isPinned));
		}
	}
	public interface IShellBuilder
	{
		IFuxionBuilder FuxionBuilder { get; }
	}
	internal class ShellBuilder : IShellBuilder
	{
		public ShellBuilder(IFuxionBuilder fuxionBuilder) => FuxionBuilder = fuxionBuilder;
		public IFuxionBuilder FuxionBuilder { get; }

		public bool ShowWindow { get; set; } = true;
		public ShellWindow? ShellWindow { get; set; }
		public ShellWindow? GetShellWindow() => ShellWindow;
	}
	internal class GenericPanelDescriptor : IPanelDescriptor
	{
		public GenericPanelDescriptor(string name, Type viewType, PanelPosition defaultPosition, bool removeOnHide, bool isPinned)
		{
			Name = PanelName.Parse(name);
			ViewType = viewType;
			DefaultPosition = defaultPosition;
			RemoveOnHide = removeOnHide;
			IsPinned = isPinned;
		}
		public Type ViewType { get; }
		public PanelPosition DefaultPosition { get; }
		public PanelName Name { get; }
		public bool RemoveOnHide { get; }
		public bool IsPinned { get; }
	}
	internal class GenericMenu : IMenu
	{
		public GenericMenu(object header, Action? clickAction = null)
		{
			Header = header;
			this.clickAction = clickAction;
		}

		private readonly Action? clickAction;
		public object Header { get; }
		public void OnClick() => clickAction?.Invoke();
	}
}
