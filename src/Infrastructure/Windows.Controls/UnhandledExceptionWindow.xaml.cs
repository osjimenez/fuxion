using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using Fuxion.Windows.Documents;
using Fuxion.Windows.Input;

namespace Fuxion.Windows.Controls;

public partial class UnhandledExceptionWindow : Window, INotifyPropertyChanged
{
	public UnhandledExceptionWindow(Exception ex, bool showDetails, Func<Task>? sendReportFunc = null)
	{
		_CanShowDetails = showDetails;
		this.ex = ex;
		this.sendReportFunc = sendReportFunc;
		// Create commands
		IgnoreCommand = new(() => Close(), () => Buttons.HasFlag(UnhandledExceptionWindowButtons.CloseWindow));
		CloseConsoleCommand = new(() => Application.Current.Shutdown(), () => Buttons.HasFlag(UnhandledExceptionWindowButtons.CloseWindow));
		RestartApplicationCommand = new(() =>
		{
			Process.Start(Application.ResourceAssembly.Location);
			Application.Current.Shutdown();
		}, () => Buttons.HasFlag(UnhandledExceptionWindowButtons.RestartApplication));
		ShowDetailsCommand = new(() => ShowDetails = !ShowDetails, () => CanShowDetails);
		SendReportCommand = new(async () =>
		{
			try
			{
				SendingReport = true;
				if (sendReportFunc != null) await sendReportFunc();
				sendReportFunc = null;
				SendReportCommand?.RaiseCanExecuteChanged();
			} catch (Exception ex2)
			{
				MessageBox.Show(this, ex2.Message, "Error al enviar el mensaje", MessageBoxButton.OK, MessageBoxImage.Error);
				Debug.WriteLine("");
			} finally
			{
				SendingReport = false;
			}
		}, () => sendReportFunc != null);
		// Inicializar interfaz
		InitializeComponent();
		// Populate Document
		Document.Blocks.AddRange(ex.ToBlocks());
	}
	readonly Exception ex;
	bool _CanShowDetails;
	UnhandledExceptionWindowButtons _Commands;
	string? _Message;
	bool _SendingReport;
	Func<Task>? sendReportFunc;
	public string ExceptionType => ex.GetType().FullName ?? throw new InvalidProgramException($"Type '{ex.GetType().Name}' hasn't FullName");
	public string? Message
	{
		get => _Message;
		set
		{
			_Message = value;
			PropertyChanged?.Invoke(this, new(nameof(Message)));
		}
	}
	public bool SendingReport
	{
		get => _SendingReport;
		set
		{
			_SendingReport = value;
			PropertyChanged?.Invoke(this, new(nameof(SendingReport)));
		}
	}
	public UnhandledExceptionWindowButtons Buttons
	{
		get => _Commands;
		set
		{
			_Commands = value;
			IgnoreCommand.RaiseCanExecuteChanged();
			RestartApplicationCommand.RaiseCanExecuteChanged();
			CloseConsoleCommand.RaiseCanExecuteChanged();
		}
	}
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
			PropertyChanged?.Invoke(this, new(nameof(ShowDetails)));
		}
	}
	public GenericCommand SendReportCommand { get; }
	public GenericCommand ShowDetailsCommand { get; }
	public GenericCommand IgnoreCommand { get; }
	public GenericCommand CloseConsoleCommand { get; }
	public GenericCommand RestartApplicationCommand { get; }
	public FlowDocument Document { get; set; } = new()
	{
		TextAlignment = TextAlignment.Left, PagePadding = new(10), FontFamily = new("Segoe UI")
	};
	public event PropertyChangedEventHandler? PropertyChanged;
	void ButtonClose_Click(object sender, RoutedEventArgs e) => CloseButtonsPopUp.IsOpen = true;
}

[Flags]
public enum UnhandledExceptionWindowButtons
{
	CloseWindow = 1,
	RestartApplication = 2,
	CloseWindowAndRestartApplication = 3
}