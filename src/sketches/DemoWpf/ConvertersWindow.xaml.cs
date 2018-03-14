using Fuxion.Windows.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
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

namespace DemoWpf
{
	public partial class ConvertersWindow : Window
	{
		public ConvertersWindow()
		{
			InitializeComponent();
			var vm = new ViewModel();
			vm.List = new List<string>();
			vm.List.Add("Hello");
			DataContext = vm;
		}
	}
	public class ViewModel
	{
		[Display(Name = "Lista")]
		public List<string> List { get; set; }
		public bool IsEnabled1 { get; set; } = false;
		public bool IsEnabled2 { get; set; } = true;
		public MyEnum MyEnum { get; set; } = MyEnum.Two;
	}
	public enum MyEnum
	{
		Zero,
		[Display(Name = "One display")]
		One,
		[Display(Name = "Two display")]
		Two,
		Three,
		Four,
		Five,
		Six,
		Seven,
		Eight,
		Nine,
		Ten
	}
	[ValueConversion(typeof(MyEnum), typeof(int), ParameterType = typeof(int))]
	public class MyEnumToIntConverter : GenericConverter<MyEnum, int, int>
	{
		public override int Convert(MyEnum source, int parameter, CultureInfo culture)
		{
			if (parameter == 0)
				return 0;
			return (int)source * parameter;
		}
	}
	[ValueConversion(typeof(int), typeof(string), ParameterType = typeof(int))]
	public class IntToStringConverter : GenericConverter<int, string, int>
	{
		public override string Convert(int source, int parameter, CultureInfo culture)
		{
			return source + "*" + parameter;
		}
	}
}
