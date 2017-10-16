using Fuxion.Factories;
using Fuxion.Licensing;
using Fuxion.Licensing.Test;
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

namespace DemoWpf
{
    /// <summary>
    /// Interaction logic for Licensing.xaml
    /// </summary>
    public partial class Licensing : Window
    {
        public Licensing()
        {
            InitializeComponent();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var man = Factory.Get<LicensingManager>();
            //var lic = man.GetProvider().Request(null);
            //man.Store.Add(lic);
            var lics = man.Store.Query().WithValidSignature(Const.PUBLIC_KEY);

            Debug.WriteLine("Comment: " + lics.FirstOrDefault()?.Comment);
            Debug.WriteLine("");
        }
    }
}
