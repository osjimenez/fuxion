using Fuxion;
using Fuxion.ComponentModel;
using Fuxion.Windows.Markup;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

namespace DemoWpf.Windows.Controls
{
	public partial class AutoGridTest : Window
	{
		public AutoGridTest()
		{
			InitializeComponent();
			DisplayExtension.NonAttrributePrefix = "property:";
			Printer.Default.WriteLineAction = m => Debug.WriteLine(m);
			DataContext = new ViewModel();
		}
	}
	public class ViewModel : Notifier<ViewModel>
	{
		[Display(Name = "Nombre",Description ="El nombre del sujeto",GroupName = "Nombres",Order =1,Prompt = "Escribe el nombre")]
		public string FirstName
		{
			get => GetValue(() => "Tomb");
			set => SetValue(value);
		}
		public string LastName
		{
			get => GetValue(() => "Raider");
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
