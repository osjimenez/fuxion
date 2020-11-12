using DemoCoreWpf.ScreenCapture;
using DemoCoreWpf.Validation;
using System.Windows;

namespace DemoCoreWpf
{
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			//var win = new ValidationWindow();
			var win = new ScreenCaptureWindow();
			win.Show();
		}
	}
}
