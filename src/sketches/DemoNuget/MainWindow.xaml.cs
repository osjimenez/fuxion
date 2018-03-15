using Fuxion.Threading.Tasks;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DemoNuget
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			t = TaskManager.StartNew(() => Task.Delay(TimeSpan.FromDays(1), TaskManager.Current.GetCancellationToken().Value));
		}
		Task t;
		private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			t.Cancel();
			await t;
		}
	}
}
