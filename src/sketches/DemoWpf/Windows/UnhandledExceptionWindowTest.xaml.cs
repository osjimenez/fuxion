using Fuxion.Threading.Tasks;
using Fuxion.Windows.Controls;
using Fuxion.Windows.Input;
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

namespace DemoWpf.Windows
{
	public partial class UnhandledExceptionWindowTest : Window
	{
		public UnhandledExceptionWindowTest()
		{
			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			var t1 = TaskManager.StartNew(() =>
			{
				throw
					new NotImplementedException("Mensaje de la excepción que puede ser muy largo, pero muy muy muy largo, vamos no te fies que a lo mejor no te cabe en la ventana ni de coña macho,\r\nLOL\r\nLOL\r\nLOL\r\nLOL\r\nLOL\r\nLOL\r\nLOL\r\nLOL\r\nLOL\r\nLOL tendrás que apañartelas como sea para que quede bonito y accesible :)"
					, new KeyNotFoundException("Mensaje de la excepción que puede ser muy largo, pero muy muy muy largo, vamos no te fies que a lo mejor no te cabe en la ventana ni de coña macho, tendrás que apañartelas como sea para que quede bonito y accesible :)")
					);
			});
			var t2 = TaskManager.StartNew(() =>
			{
				throw new ApplicationException("Peto la aplicación :)");
			});
			try
			{

				Task.WaitAll(t1, t2);
			}
			catch (AggregateException ex)
			{
				UnhandledExceptionWindow win = null;
				win = new UnhandledExceptionWindow(ex)
				{
					Message = "Se ha producido un error grave",
					Buttons = UnhandledExceptionWindowButtons.CloseWindow,
					CanShowDetails = true,
					//SendReportCommand = new GenericCommand<string>(content => MessageBox.Show("Report sent with content:\r\n" + content))
				};
				win.ShowDialog();
			}
		}
	}
}
