using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    /// <summary>
    /// Interaction logic for ConvertersWindow.xaml
    /// </summary>
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
    }
}
