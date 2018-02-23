using Fuxion.Windows.Documents;
using Fuxion.Windows.Input;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
namespace Fuxion.Windows.Controls
{
	public partial class UnhandledExceptionWindow : Window, INotifyPropertyChanged
	{
		public UnhandledExceptionWindow(Exception ex, Func<Task> sendReportFunc = null)
		{
			this.ex = ex;
			this.sendReportFunc = sendReportFunc;
			// Create commands
			CloseCommand = new GenericCommand(() => Close(), () => Buttons.HasFlag(UnhandledExceptionWindowButtons.CloseWindow));
			RestartApplicationCommand = new GenericCommand(() =>
			{
				Process.Start(Application.ResourceAssembly.Location);
				Application.Current.Shutdown();
			}, () => Buttons.HasFlag(UnhandledExceptionWindowButtons.RestartApplication));
			ShowDetailsCommand = new GenericCommand(() => ShowDetails = !ShowDetails, () => CanShowDetails);
			SendReportCommand = new GenericCommand(async () =>
			{
				try
				{
					SendingReport = true;
					await sendReportFunc.Invoke();
				}
				catch (Exception ex2)
				{
					Debug.WriteLine("");
				}
				finally
				{
					sendReportFunc = null;
					SendReportCommand.RaiseCanExecuteChanged();
					SendingReport = false;
				}
			}, () => sendReportFunc != null);
			// Inicializar interfaz
			InitializeComponent();
			// Populate Document
			Document.Blocks.AddRange(ex.ToBlocks());
		}
		public event PropertyChangedEventHandler PropertyChanged;
		Func<Task> sendReportFunc;
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
		bool _SendingReport;
		public bool SendingReport
		{
			get => _SendingReport;
			set
			{
				_SendingReport = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SendingReport)));
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
		public GenericCommand SendReportCommand { get; private set; }
		public GenericCommand ShowDetailsCommand { get; private set; }
		public GenericCommand CloseCommand { get; private set; }
		public GenericCommand RestartApplicationCommand { get; private set; }

		public FlowDocument Document { get; set; } = new FlowDocument
		{
			TextAlignment = TextAlignment.Left,
			PagePadding = new Thickness(10),
			FontFamily = new FontFamily("Segoe UI")
		};
	}
	[Flags]
	public enum UnhandledExceptionWindowButtons
	{
		CloseWindow = 1,
		RestartApplication = 2,
		CloseWindowAndRestartApplication = 3
	}
}