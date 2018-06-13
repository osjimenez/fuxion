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
using System.Windows.Shapes;

namespace DemoWpf.ServiceModel
{
	public partial class ClientWindow : Window
	{
		public ClientWindow()
		{
			InitializeComponent();
			DataContext = new Proxy();
		}

		public Proxy Proxy => DataContext as Proxy;

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Proxy.Connect();
				Proxy.Test();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Error");
			}
		}
	}
}
