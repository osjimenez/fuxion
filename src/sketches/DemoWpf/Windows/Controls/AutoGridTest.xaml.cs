using Fuxion.ComponentModel;
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

namespace DemoWpf.Windows.Controls
{
	public partial class AutoGridTest : Window
	{
		public AutoGridTest()
		{
			InitializeComponent();

			TextBlock tb = new TextBlock();
			TextBox tbox = new TextBox();
			Rectangle r = new Rectangle();
			tb.Visibility = Visibility.Collapsed;
		}
	}
	public class ViewModel : Notifier<ViewModel>
	{
		public string Name
		{
			get => GetValue(() => "Tomb");
			set => SetValue(value);
		}
		public string Genre
		{
			get => GetValue<string>();
			set => SetValue(value);
		}
		public int Age
		{
			get => GetValue(() => 22);
			set => SetValue(value);
		}
	}
}
