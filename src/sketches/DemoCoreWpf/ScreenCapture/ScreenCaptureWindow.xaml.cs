namespace DemoCoreWpf.ScreenCapture;

using System.Diagnostics;
using System.Drawing.Imaging;
using System.Windows;

public partial class ScreenCaptureWindow : Window
{
	public ScreenCaptureWindow() => InitializeComponent();
	private void Button_Click(object sender, RoutedEventArgs e)
	{
		var bitmap = Fuxion.Drawing.ScreenCapture.CaptureActiveWindow(true);
		bitmap?.Save($"test - {DateTime.Now.ToString("HH.mm.ss fff")}.jpg", ImageFormat.Jpeg);
		Debug.WriteLine("");
	}
}