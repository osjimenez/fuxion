using Fuxion.Shell.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml.Linq;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Docking;

namespace Fuxion.Shell
{
	public class DockingManager
	{
		public DockingManager(IServiceProvider serviceProvider, ILogger<DockingManager> logger)
		{
			this.serviceProvider = serviceProvider;
			this.logger = logger;
			panelDescritors = serviceProvider.GetServices<IPanelDescriptor>().ToList();

			MessageBus.Current.Listen<OpenPanelMessage>()
				.Subscribe(message => OpenPanel(message.Name, message.Arguments));
			MessageBus.Current.Listen<ClosePanelMessage>()
				.Subscribe(message =>
				{
					if (message.Pane != null)
						CloseRadPane(message.Pane);
					else
						ClosePanel(message.Name);
				});
			MessageBus.Current.Listen<CloseAllPanelsWithKeyMessage>()
				.Subscribe(message => CloseAllPanelsWithKey(message.Key));

			MessageBus.Current.Listen<LockMessage>()
				.Subscribe(message =>
				{
					locked = true;
					foreach (var panelInstance in panelInstances)
						LockRadPane(panelInstance.RadPane);
				});
			MessageBus.Current.Listen<UnlockMessage>()
				.Subscribe(message =>
				{
					locked = false;
					foreach (var panelInstance in panelInstances)
						UnlockRadPane(panelInstance.RadPane);
				});

			MessageBus.Current.Listen<LoadLayoutMessage>()
				.Subscribe(message => docking.LoadLayout(message.LayoutFileStream));
			MessageBus.Current.Listen<SaveLayoutMessage>()
				.Subscribe(message => docking.SaveLayout(message.LayoutFileStream));
		}

		private readonly ILogger logger;
		private bool locked;
		private readonly IServiceProvider serviceProvider;
		private readonly List<IPanelDescriptor> panelDescritors;
		private RadDocking docking;
		private ObservableCollection<PanelInstance> panelInstances = new ObservableCollection<PanelInstance>();

		private void GenerateException()
		{
			throw new InvalidOperationException("Operación inválida de prueba");
		}

		public void AttachDocking(RadDocking docking)
		{
			//using var scope = logger.BeginScope(nameof(AttachDocking));
			logger.LogTrace(nameof(AttachDocking));
			//logger.LogDebug("AttachDocking");
			//using (logger.BeginScope("OtroScope"))
			//{
			try
			{
				GenerateException();
			}
			catch (Exception ex)
			{
				//logger.LogError(new EventId(123, "Evento de prueba"), ex, "Mensaje de prueba", "Argumento de prueba", PanelName.Parse("Nombre@Key"));
				logger.LogError(ex, "Mensaje de prueba");
			}
			//}
			if (this.docking != null) throw new InvalidOperationException($"Only one RadDocking can be attached");
			this.docking = docking;
			void AttachPane(PanelPosition state)
			{
				var split = new RadSplitContainer { InitialPosition = (DockState)(int)state };
				//var pane = new RadPaneGroup { Tag = state, TabStripPlacement = Dock.Top };
				var pane = new RadPaneGroup { Tag = state };
				RadDocking.SetSerializationTag(pane, state.ToString());
				split.Items.Add(pane);
				docking.Items.Add(split);
			}
			AttachPane(PanelPosition.DockedTop);
			AttachPane(PanelPosition.DockedBottom);
			AttachPane(PanelPosition.DockedLeft);
			AttachPane(PanelPosition.DockedRight);
			var sp = new RadSplitContainer();
			var gr = new RadPaneGroup
			{
				Tag = PanelPosition.Document,
				TabStripPlacement = Dock.Left
			};
			RadDocking.SetSerializationTag(gr, PanelPosition.Document.ToString());
			sp.Items.Add(gr);
			docking.DocumentHost = sp;
			docking.DockingPanesFactory = new ShellDockingPanesFactory((dock, radPane) =>
			{
				//logger.LogDebug("AddPane");
				logger.LogTrace($"{nameof(docking)}.{nameof(docking.DockingPanesFactory)}.AddPane");
				IPanelDescriptor descriptor;
				if (radPane.Content is IPanel panel)
					descriptor = panelInstances.Single(i => i.Panel == panel).Descriptor;
				else if (radPane.Content is IPanelView view)
					descriptor = panelInstances.Single(i => i.Panel == view.Panel).Descriptor;
				else throw new NotSupportedException($"The '{nameof(RadPane)}.{nameof(radPane.Content)}' must be '{nameof(IPanel)}' or '{nameof(IPanelView)}'");
				PositionPanel(radPane, descriptor.DefaultPosition);
			}, (dock, item) => item is RadPane radPane ? radPane : null);
			docking.ElementLoading += (_, e) =>
			{
				//logger.LogDebug($"Dock_ElementLoading - AffectedElementSerializationTag: '{e.AffectedElementSerializationTag}'");
				logger.LogTrace($"{nameof(docking)}.{nameof(docking.ElementLoading)} - {nameof(e.AffectedElementSerializationTag)}: {e.AffectedElementSerializationTag}");
				try
				{
					var name = PanelName.Parse(e.AffectedElementSerializationTag);
					if (panelDescritors.Any(p => p.Name.Name == name.Name))
							// Element is a known panel
							e.SetAffectedElement(CreatePanelInstance(name, e.ElementProperties.ToDictionary(p => p.Key, p => (object)p.Value)).RadPane);
					else if (Enum.GetNames(typeof(PanelPosition)).Contains(e.AffectedElementSerializationTag) && e.AffectedElement is RadPaneGroup pane)
							// Element is a main structural group panel
							pane.Tag = Enum.Parse(typeof(PanelPosition), e.AffectedElementSerializationTag);
					else
							// Element is unknown
							e.Cancel = true;
				}
				catch (Exception ex)
				{
					//logger.LogError($"ERROR loading '{e.AffectedElementSerializationTag}'", ex);
					logger.LogError(ex, $"ERROR loading '{e.AffectedElementSerializationTag}'");
				}
			};
			docking.ElementLoaded += (_, e) =>
			{
				//logger.LogDebug($"Dock_ElementLoaded - AffectedElementSerializationTag: '{e.AffectedElementSerializationTag}'");
				logger.LogTrace($"{nameof(docking)}.{nameof(docking.ElementLoaded)} - {nameof(e.AffectedElementSerializationTag)}: {e.AffectedElementSerializationTag}");
				if (e.AffectedElement is RadPane radPane)
				{
					IPanel panel;
					if (radPane.Content is IPanel pa) panel = pa;
					else if (radPane.Content is IPanelView vi) panel = vi.Panel;
					else throw new NotSupportedException($"Must be '{nameof(IPanel)}' or '{nameof(IPanelView)}'");
					radPane.SetBinding(RadPane.TitleProperty, new Binding(nameof(panel.Title)) { Source = panel });
					radPane.SetBinding(RadPane.HeaderProperty, new Binding(nameof(panel.Header)) { Source = panel });
				}
			};
			docking.ElementLayoutSaving += (_, e) =>
			{
				//logger.LogDebug($"Dock_ElementLayoutSaving '{e.AffectedElementSerializationTag}'");
				logger.LogTrace($"{nameof(docking)}.{nameof(docking.ElementLayoutSaving)} - {nameof(e.AffectedElementSerializationTag)}: {e.AffectedElementSerializationTag}");
				if (e.ElementProperties.ContainsKey("IsHidden") && e.ElementProperties["IsHidden"] == "True")
					e.Cancel = true;
			};
			docking.Close += (s, e) =>
			{
				logger.LogTrace($"{nameof(docking)}.{nameof(docking.Close)}");
				foreach (var radPane in e.Panes)
				{
					var panelInstance = panelInstances.Single(i => i.View == radPane.Content);
					if (panelInstance.Descriptor.RemoveOnHide)
					{
						panelInstance.RadPane.RemoveFromParent();
						panelInstances.Remove(panelInstance);
					}
				}
			};
		}
		private void OpenPanel(PanelName name, Dictionary<string, object> args)
		{
			//logger.LogDebug($"OpenPanel - name: {name}");
			logger.LogTrace($"{nameof(OpenPanel)}({nameof(name)}: {name}, {nameof(args)}: {args})");
			var panelInstance = panelInstances.FirstOrDefault(i => i.Descriptor.Name == name);
			if (panelInstance != null)
				panelInstance.RadPane.IsHidden = false;
			else
			{
				panelInstance = CreatePanelInstance(name, args);
				PositionPanel(panelInstance.RadPane, panelInstance.Descriptor.DefaultPosition);
			}
			panelInstance.RadPane.IsActive = true;
		}

		internal void CloseRadPane(RadPane pane)
		{
			pane.IsHidden = true;
			pane.IsActive = false;
		}
		private void ClosePanel(PanelName name)
		{
			//logger.LogDebug($"ClosePanel - name: {name}");
			logger.LogTrace($"{nameof(ClosePanel)}({nameof(name)}: {name})");
			var panelInstance = panelInstances.FirstOrDefault(i => i.Descriptor.Name == name);
			if (panelInstance != null)
				CloseRadPane(panelInstance.RadPane);
		}
		private void CloseAllPanelsWithKey(string key)
		{
			//logger.LogDebug($"CloseAllPanelsWithKey - key: {key}");
			logger.LogTrace($"{nameof(CloseAllPanelsWithKey)}({nameof(key)}: {key})");
			foreach (var panelInstance in panelInstances.Where(i => i.Descriptor.Name.Key == key).ToList())
				CloseRadPane(panelInstance.RadPane);
		}

		private void LockRadPane(RadPane pane)
		{
			// Quita la cabecera de los paneles pineados
			pane.PaneHeaderVisibility = Visibility.Collapsed;
			// Quita la cabecera de los panales del DocumentHost. No se podrá cambiar de pestañas
			//pane.Visibility = Visibility.Collapsed;

			//pane.CanUserClose = false;
			pane.CanUserPin = false;
			pane.CanFloat = false;
			pane.IsDockable = false;

		}
		private void UnlockRadPane(RadPane pane)
		{
			// Quita la cabecera de los paneles pineados
			pane.PaneHeaderVisibility = Visibility.Visible;
			// Quita la cabecera de los panales del DocumentHost. No se podrá cambiar de pestañas
			//pane.Visibility = Visibility.Collapsed;

			//pane.CanUserClose = false;
			pane.CanUserPin = true;
			pane.CanFloat = true;
			pane.IsDockable = true;
			
		}

		private void PositionPanel(RadPane radPane, PanelPosition position)
		{
			//logger.LogDebug($"PositionPanel - position {position}");
			logger.LogTrace($"{nameof(PositionPanel)}({nameof(position)}: {position})");
			if (locked)
				LockRadPane(radPane);
			else
				UnlockRadPane(radPane);
			switch (position)
			{
				case PanelPosition.DockedBottom:
				case PanelPosition.DockedLeft:
				case PanelPosition.DockedRight:
				case PanelPosition.DockedTop:
				case PanelPosition.Document:
					{
						if (docking.SplitItems.ToList().FirstOrDefault(i => i.Control.Tag is PanelPosition pos && pos == position) is RadPaneGroup group)
							group.Items.Add(radPane);
						return;
					}
				case PanelPosition.FloatingDockable:
				case PanelPosition.FloatingOnly:
					{
						var split = docking.GeneratedItemsFactory.CreateSplitContainer();
						var group = docking.GeneratedItemsFactory.CreatePaneGroup();
						split.Items.Add(group);
						group.Items.Add(radPane);
						docking.Items.Add(split);
						if (position == PanelPosition.FloatingDockable) radPane.MakeFloatingDockable();
						else radPane.MakeFloatingOnly();
						return;
					}
				default:
					throw new ArgumentException($"Panel default position '{position}' is not a valid value");
			}
		}
		private PanelInstance CreatePanelInstance(PanelName name, Dictionary<string,object> args)
		{
			//logger.LogDebug($"CreatePanelInstance - name: {name}");
			logger.LogTrace($"{nameof(CreatePanelInstance)}({nameof(name)}: {name}, {nameof(args)}: {args})");
			var descriptor = panelDescritors.Single(p => p.Name.Name == name.Name);
			var view = serviceProvider.GetService(descriptor.ViewType);
			if (!(view is FrameworkElement)) throw new ArgumentException($"The '{nameof(descriptor.ViewType)}' must be '{nameof(FrameworkElement)}'");
			var radPane = new RadPane { Content = view, IsPinned = descriptor.IsPinned };
			IPanel panel;
			if (view is IPanel pa) panel = pa;
			else if (view is IPanelView vi) panel = vi.Panel;
			else throw new NotSupportedException($"The '{nameof(descriptor.ViewType)}' must be '{nameof(IPanel)}' or '{nameof(IPanelView)}'");
			var panelInstance = new PanelInstance(string.IsNullOrWhiteSpace(descriptor.Name.Key)
				? new GenericPanelDescriptor(name.ToString(), descriptor.ViewType, descriptor.DefaultPosition, descriptor.RemoveOnHide, descriptor.IsPinned)
				: descriptor, panel, (FrameworkElement)view, radPane);
			panelInstances.Add(panelInstance);
			radPane.SetBinding(RadPane.TitleProperty, new Binding(nameof(panel.Title)) { Source = panel });
			radPane.SetBinding(RadPane.HeaderProperty, new Binding(nameof(panel.Header)) { Source = panel });
			RadDocking.SetSerializationTag(radPane, name.ToString());
			if (panel is IInitializablePanel iniPanel)
				iniPanel.Initialize(name, args);
			return panelInstance;
		}
	}
	internal class ShellDockingPanesFactory : DockingPanesFactory
	{
		public ShellDockingPanesFactory(Action<RadDocking, RadPane> addPane, Func<RadDocking, object, RadPane> getPaneFromItem)
		{
			this.addPane = addPane;
			this.getPaneFromItem = getPaneFromItem;
		}

		private readonly Action<RadDocking, RadPane> addPane;
		private readonly Func<RadDocking, object, RadPane> getPaneFromItem;
		protected override void AddPane(RadDocking dock, RadPane pane) => addPane(dock, pane);
		protected override RadPane GetPaneFromItem(RadDocking docking, object item) => getPaneFromItem(docking, item);
	}
}
