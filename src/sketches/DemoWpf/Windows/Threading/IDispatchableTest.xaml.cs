using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Fuxion.ComponentModel;
using Fuxion.Factories;
using Fuxion.Threading.Tasks;
using Fuxion.Windows.Threading;
namespace DemoWpf.Windows.Threading
{
	public partial class IDispatchableTest : Window
	{
		public IDispatchableTest()
		{
			InitializeComponent();
			//Factory.AddInjector(new InstanceInjector<IDispatcherProvider>(new InstanceDispatcherProvider(Dispatcher)));
		}

		private void Test1_Click(object sender, RoutedEventArgs e)
		{
			TaskManager.StartNew(() => new ViewModel().Test());
		}
	}

	public class ViewModel : Notifier<ViewModel>
	{
		public ViewModel()
		{
			((IInvokable)this).UseInvoker = false;
		}
		public void Test()
		{
			this.Invoke(() => { Debug.WriteLine(""); });
		}
	}
}
