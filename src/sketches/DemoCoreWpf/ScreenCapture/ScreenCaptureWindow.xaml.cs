using System.Diagnostics;
using System.Drawing.Imaging;
using System.Windows;

namespace DemoCoreWpf.ScreenCapture;

public partial class ScreenCaptureWindow : Window
{
	public ScreenCaptureWindow() => InitializeComponent();
	void Button_Click(object sender, RoutedEventArgs e)
	{
		var bitmap = Fuxion.Drawing.ScreenCapture.CaptureActiveWindow(true);
		bitmap?.Save($"test - {DateTime.Now.ToString("HH.mm.ss fff")}.jpg", ImageFormat.Jpeg);
		Debug.WriteLine("");
	}
}