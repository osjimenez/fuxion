using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;

namespace Fuxion.Windows.Markup
{
	public class DisplayExtension : MarkupExtension
	{
		public DisplayExtension(string bindPath)
		{
			var pros = bindPath.Split('.');
			for (int i = pros.Length - 1; i >= 0; i--)
			{
				chain.Add(new NotifierChainLink
				{
					NextLink = i == (pros.Length - 1) ? null : chain.FirstOrDefault(l => l.PropertyName == pros[i + 1]),
					PropertyName = pros[i]
				});
			}
			chain.Reverse();
			for (int i = 0; i < chain.Count; i++)
				chain[i].PreviousLink = i == 0 ? null : chain[i - 1];
		}

		public static string NonAttrributePrefix { get; set; }
		public static string NonAttrributeSufix { get; set; }

		IPrinter _Printer;
		internal IPrinter Printer {
			get => _Printer;
			set
			{
				_Printer = value;
				foreach (var link in chain)
					link.printer = value;
			}
		}
		internal List<NotifierChainLink> chain = new List<NotifierChainLink>();
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			if (serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget provider)
			{
				
				if (provider.TargetObject == null || provider.TargetProperty == null) return null;
				
				var first = chain.First();
				
				if(provider.TargetObject is FrameworkElement element)
				{
					first.TargetObject = element;
					element.DataContextChanged += (s, e) =>
					{
						Printer?.WriteLine("DataContextChanged: " + e.NewValue);
						first.DataContext = e.NewValue;
						first.SetValue();
					};
				}
				else if (provider.TargetObject is FrameworkContentElement contentElement)
				{
					first.TargetObject = contentElement;
					contentElement.DataContextChanged += (s, e) =>
					{
						Printer?.WriteLine("DataContextChanged: " + e.NewValue);
						first.DataContext = e.NewValue;
						first.SetValue();
					};
				}
				first.TargetDependencyProperty = provider.TargetProperty as DependencyProperty;
				first.SetValue();
			}
			return null;
		}
	}
	internal class NotifierChainLink
	{
		public NotifierChainLink()
		{
			EventHandler = PropertyChanged;
		}
		internal IPrinter printer;
		private void PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(DataContext != null)
				if(NextLink != null)
					NextLink.DataContext = ContextProperty?.GetValue(DataContext);
			SetValue();
		}

		public NotifierChainLink NextLink { get; set; }
		public NotifierChainLink PreviousLink { get; set; }

		public string PropertyName { get; set; }
		public PropertyChangedEventHandler EventHandler { get; set; }

		public INotifyPropertyChanged ContextNotifier => DataContext != null
			? typeof(INotifyPropertyChanged).IsAssignableFrom(DataContext.GetType()) 
				? (INotifyPropertyChanged)Context 
				: null
			: null;
		object _DataContext;
		public object DataContext
		{
			get => _DataContext;
			set
			{
				_DataContext = value;
				if (ContextNotifier != null)
					ContextNotifier.PropertyChanged += EventHandler;
			}
		}
		object Context => DataContext ?? (PreviousLink?.Context != null
				? PreviousLink?.ContextProperty?.GetValue(PreviousLink.Context)
				: null);
		Type ContextType => Context?.GetType();
		PropertyInfo ContextProperty => ContextType?.GetProperty(PropertyName);
		DisplayAttribute ContextAttribute => ContextProperty?.GetCustomAttribute<DisplayAttribute>(true, false);

		DependencyProperty _TargetDependencyProperty;
		public DependencyProperty TargetDependencyProperty
		{
			get => _TargetDependencyProperty ?? PreviousLink?.TargetDependencyProperty;
			set => _TargetDependencyProperty = value;
		}
		object _TargetElement;
		public object TargetObject
		{
			get => _TargetElement ?? PreviousLink?.TargetObject;
			set => _TargetElement = value;
		}
		public PropertyInfo TargetProperty => TargetObject?.GetType().GetProperty(TargetDependencyProperty?.Name);

		public void SetValue()
		{
			printer?.WriteLine($"Setting value for {PropertyName}");
			if (NextLink == null)
			{
				printer?.WriteLine($"   DataContext: {(DataContext?.ToString() ?? "null")}");
				printer?.WriteLine($"   PreviousLink: {(PreviousLink?.ToString() ?? "null")}");
				printer?.WriteLine($"   Context: {(Context?.ToString() ?? "null")}");
				printer?.WriteLine($"   ContextType: {(ContextType?.Name ?? "null")}");
				printer?.WriteLine($"   ContextProperty: {(ContextProperty?.Name ?? "null")}");
				printer?.WriteLine($"   ContextAttribute: {(ContextAttribute?.ToString() ?? "null")}");
				printer?.WriteLine($"   TargetElement: {(TargetObject?.ToString() ?? "null")}");
				printer?.WriteLine($"   TargetProperty: {(TargetProperty?.Name ?? "null")}");
				if (TargetObject != null)
					TargetProperty?.SetValue(TargetObject, ContextAttribute?.GetName() ?? DisplayExtension.NonAttrributePrefix + ContextProperty?.Name + DisplayExtension.NonAttrributeSufix);
			}
			else NextLink.SetValue();
		}

		public override string ToString() => PropertyName;
	}
}
