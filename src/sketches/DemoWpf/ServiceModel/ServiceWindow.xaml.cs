using Fuxion.ServiceModel;
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
	public partial class ServiceWindow : Window
	{
		public ServiceWindow()
		{
			InitializeComponent();
			DataContext = new Service();
		}

		public Service Service => DataContext as Service;

		private void Publish_Click(object sender, RoutedEventArgs e)
		{
			Service.Publish();
		}
	}
}
