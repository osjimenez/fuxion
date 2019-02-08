using Fuxion.Reflection;
using Fuxion.Shell;
using System.Windows.Controls;

namespace Ordinem.Shell.Wpf.Launcher.Views
{
	[TypeKey("Ordinem.Shell.Wpf.Launcher.Views." + nameof(AboutPanel))]
	public partial class AboutPanel : UserControl, IPanel
	{
		public AboutPanel() => InitializeComponent();

		public string Title => "About";
		public string Header => "About";
	}
}
