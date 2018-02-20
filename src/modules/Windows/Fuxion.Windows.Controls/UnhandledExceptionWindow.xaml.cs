using Fuxion.ComponentModel;
using Fuxion.Windows.Documents;
using Fuxion.Windows.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Fuxion.Windows.Controls
{
	public partial class UnhandledExceptionWindow : Window, INotifyPropertyChanged
	{
		public UnhandledExceptionWindow(Exception ex)
		{
			this.ex = ex;
			// Create commands
			CloseCommand = new GenericCommand(() => this.Close(), () => Buttons == UnhandledExceptionWindowButtons.CloseWindow);
			RestartApplicationCommand = new GenericCommand(() =>
			{
				System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
				Application.Current.Shutdown();
			}, () => Buttons == UnhandledExceptionWindowButtons.RestartApplication);
			ShowDetailsCommand = new GenericCommand(() => ShowDetails = !ShowDetails, () => CanShowDetails);
			// Inicializar interfaz
			InitializeComponent();
			// Populate Document
			Document.Blocks.AddRange(ex.ToBlocks());
		}
		public event PropertyChangedEventHandler PropertyChanged;
		Exception ex;
		string _Message;
		public string Message
		{
			get => _Message;
			set
			{
				_Message = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message)));
			}
		}

		UnhandledExceptionWindowButtons _Commands;
		public UnhandledExceptionWindowButtons Buttons
		{
			get => _Commands;
			set
			{
				_Commands = value;
				CloseCommand.RaiseCanExecuteChanged();
				RestartApplicationCommand.RaiseCanExecuteChanged();
			}
		}
		bool _CanShowDetails;
		public bool CanShowDetails
		{
			get => _CanShowDetails;
			set
			{
				_CanShowDetails = value;
				ShowDetailsCommand.RaiseCanExecuteChanged();
			}
		}
		public bool ShowDetails
		{
			get => ResizeMode == ResizeMode.CanResizeWithGrip;
			set
			{
				ResizeMode = value ? ResizeMode.CanResizeWithGrip : ResizeMode.NoResize;
				SizeToContent = value ? SizeToContent.Manual : SizeToContent.WidthAndHeight;
				WindowStyle = value ? WindowStyle.SingleBorderWindow : WindowStyle.None;
				WindowState = value ? WindowState.Maximized : WindowState.Normal;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowDetails)));
			}
		}
		public static void WriteExceptionDetails(Exception exception, StringBuilder builderToFill, int level)
		{
			var indent = new string(' ', level);

			if (level > 0)
			{
				builderToFill.AppendLine(indent + "=== INNER EXCEPTION ===");
			}

			Action<string> append = (prop) =>
			{
				var propInfo = exception.GetType().GetProperty(prop);
				var val = propInfo.GetValue(exception);

				if (val != null)
				{
					builderToFill.AppendFormat("{0}{1}: {2}{3}", indent, prop, val.ToString(), Environment.NewLine);
				}
			};

			append("Message");
			append("HResult");
			append("HelpLink");
			append("Source");
			append("StackTrace");
			append("TargetSite");

			foreach (DictionaryEntry de in exception.Data)
			{
				builderToFill.AppendFormat("{0} {1} = {2}{3}", indent, de.Key, de.Value, Environment.NewLine);
			}

			if (exception.InnerException != null)
			{
				WriteExceptionDetails(exception.InnerException, builderToFill, ++level);
			}
		}
		public string ReportContent
		{
			get
			{
				return ex.ToJson(settings: new JsonSerializerSettings().Transform<JsonSerializerSettings>(s => s.ContractResolver = new CustomResolver()));
			}
		}
		GenericCommand<string> _SendReportCommand;
		public GenericCommand<string> SendReportCommand
		{
			get => _SendReportCommand;
			set
			{
				_SendReportCommand = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SendReportCommand)));
			}
		}
		public GenericCommand ShowDetailsCommand { get; set; }
		public GenericCommand CloseCommand { get; private set; }
		public GenericCommand RestartApplicationCommand { get; private set; }

		public FlowDocument Document { get; set; } = new FlowDocument
		{
			TextAlignment = TextAlignment.Left,
			PagePadding = new Thickness(10),
			FontFamily = new FontFamily("Segoe UI")
		};
	}
	public enum UnhandledExceptionWindowButtons
	{
		CloseWindow,
		RestartApplication
	}
	class CustomResolver : DefaultContractResolver
	{
		protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
		{
			JsonProperty property = base.CreateProperty(member, memberSerialization);

			property.ShouldSerialize = instance =>
			{
				try
				{
					PropertyInfo prop = (PropertyInfo)member;
					if (prop.CanRead)
					{
						prop.GetValue(instance, null);
						return true;
					}
				}
				catch
				{
				}
				return false;
			};

			return property;
		}
	}
}