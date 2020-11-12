using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DemoCoreWpf.ScreenCapture
{
	public partial class ScreenCaptureWindow : Window
	{
		public ScreenCaptureWindow()
		{
			InitializeComponent();
		}
		private void Button_Click(object sender, RoutedEventArgs e)
		{
			var bitmap = Fuxion.Drawing.ScreenCapture.CaptureActiveWindow(true);
			bitmap?.Save($"test - {DateTime.Now.ToString("HH.mm.ss fff")}.jpg", ImageFormat.Jpeg);
			Debug.WriteLine("");
		}
	}
}
