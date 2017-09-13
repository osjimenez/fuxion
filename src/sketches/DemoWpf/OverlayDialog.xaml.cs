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

namespace DemoWpf
{
    public partial class OverlayDialog : UserControl
    {
        //public OverlayDialogData Data { get { return DataContext as OverlayDialogData; } }
        public OverlayDialog()
        {
            InitializeComponent();
            //DataContext = data;
            //switch (data.Type)
            //{
            //    case OverlayDialogType.Validation:
            //        border.BorderBrush = (Brush)this.FindResource("ValidationBrush");
            //        accentBox.Fill = (Brush)this.FindResource("ValidationBrush");
            //        break;
            //    case OverlayDialogType.Error:
            //        border.BorderBrush = Brushes.Red;
            //        accentBox.Fill = Brushes.Red;
            //        break;
            //}
        }
    }
}
