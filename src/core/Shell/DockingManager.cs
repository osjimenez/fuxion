using Fuxion.Logging;
using Fuxion.Shell.UIMessages;
using Microsoft.Extensions.DependencyInjection;
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
		public DockingManager(IServiceProvider serviceProvider)
		{
			this.serviceProvider = serviceProvider;
			panelDescritors = serviceProvider.GetServices<IPanelDescriptor>().ToList();

			MessageBus.Current.Listen<OpenPanelUIMessage>()
				.Subscribe(message => OpenPanel(message.Name, message.Arguments));

			MessageBus.Current.Listen<ReadModeUIMEssage>()
				.Subscribe(message =>
				{
					foreach (var panelInstance in panelInstances)
					{
						// Quita la cabecera de los paneles pineados
						panelInstance.RadPane.PaneHeaderVisibility = Visibility.Collapsed;
						// Quita la cabecera de los panales del DocumentHost. No se podrá cambiar de pestañas
						//panelInstance.RadPane.Visibility = Visibility.Collapsed;

						//panelInstance.RadPane.CanUserClose = false;
						panelInstance.RadPane.CanUserPin = false;
						panelInstance.RadPane.CanFloat = false;
					}
				});

			MessageBus.Current.Listen<LoadLayoutUIMessage>()
				.Subscribe(message =>
				{
					if (File.Exists(layoutFilePath))
						using (var str = File.OpenRead(layoutFilePath))
						{
							docking.LoadLayout(str);
						}
				});

			MessageBus.Current.Listen<SaveLayoutUIMessage>()
				.Subscribe(message =>
				{
					if (File.Exists(layoutFilePath)) File.Delete(layoutFilePath);
					using (var mem = new MemoryStream())
					{
						docking.SaveLayout(mem);
						mem.Position = 0;
						File.AppendAllText(layoutFilePath, XDocument.Load(mem).ToString());
					}
				});
		}

		private readonly string layoutFilePath = "layout.xml";
		private readonly ILog log = LogManager.Create<DockingManager>();
		private readonly IServiceProvider serviceProvider;
		private readonly List<IPanelDescriptor> panelDescritors;
		private RadDocking docking;
		private ObservableCollection<PanelInstance> panelInstances = new ObservableCollection<PanelInstance>();

		public void AttachDocking(RadDocking docking)
		{
			log.Verbose("AttachDocking");
			if (this.docking != null) throw new InvalidOperationException($"Only one RadDocking are supported");
			this.docking = docking;
			void AdaptPane(PanelPosition state)
			{
				var split = new RadSplitContainer { InitialPosition = (DockState)(int)state };
				var pane = new RadPaneGroup { Tag = state, TabStripPlacement = Dock.Top };
				RadDocking.SetSerializationTag(pane, state.ToString());
				split.Items.Add(pane);
				docking.Items.Add(split);
			}
			AdaptPane(PanelPosition.DockedTop);
			AdaptPane(PanelPosition.DockedBottom);
			AdaptPane(PanelPosition.DockedLeft);
			AdaptPane(PanelPosition.DockedRight);
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
				log.Verbose("AddPane");
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
				log.Verbose($"Dock_ElementLoading - AffectedElementSerializationTag: '{e.AffectedElementSerializationTag}'");
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
					log.Error($"ERROR loading '{e.AffectedElementSerializationTag}'", ex);
				}
			};
			docking.ElementLoaded += (_, e) =>
			{
				log.Verbose($"Dock_ElementLoaded - AffectedElementSerializationTag: '{e.AffectedElementSerializationTag}'");
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
				log.Verbose($"Dock_ElementLayoutSaving '{e.AffectedElementSerializationTag}'");
				if (e.ElementProperties.ContainsKey("IsHidden") && e.ElementProperties["IsHidden"] == "True")
					e.Cancel = true;
			};
			docking.Close += (s, e) =>
			{
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
			log.Verbose($"OpenPanel - name: {name}");
			var panelInstance = panelInstances.FirstOrDefault(i => i.Descriptor.Name == name);
			if (panelInstance != null)
			{
				panelInstance.RadPane.IsHidden = false;
				panelInstance.RadPane.IsActive = true;
			}
			else
			{
				var panelinstance = CreatePanelInstance(name, args);
				PositionPanel(panelinstance.RadPane, panelinstance.Descriptor.DefaultPosition);
			}
		}
		private void PositionPanel(RadPane radPane, PanelPosition position)
		{
			log.Verbose($"PositionPanel - position {position}");
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
			log.Verbose($"CreatePanelInstance - name: {name}");
			var descriptor = panelDescritors.Single(p => p.Name.Name == name.Name);
			var view = serviceProvider.GetService(descriptor.ViewType);
			if (!(view is FrameworkElement)) throw new ArgumentException($"The '{nameof(descriptor.ViewType)}' must be '{nameof(FrameworkElement)}'");
			var radPane = new RadPane { Content = view };
			IPanel panel;
			if (view is IPanel pa) panel = pa;
			else if (view is IPanelView vi) panel = vi.Panel;
			else throw new NotSupportedException($"The '{nameof(descriptor.ViewType)}' must be '{nameof(IPanel)}' or '{nameof(IPanelView)}'");
			var panelInstance = new PanelInstance(string.IsNullOrWhiteSpace(descriptor.Name.Key)
				? new GenericPanelDescriptor(name.ToString(), descriptor.ViewType, descriptor.DefaultPosition, descriptor.RemoveOnHide)
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
	public class ShellDockingPanesFactory : DockingPanesFactory
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
