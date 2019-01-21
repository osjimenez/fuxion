using Fuxion.Reflection;
using Fuxion.Shell;
using System.Windows.Controls;

namespace Shell.ModuleA.Views
{
	[TypeKey("Shell.ModuleA.Views." + nameof(AboutPanel))]
	public partial class AboutPanel : UserControl, IPanel
	{
		public AboutPanel() => InitializeComponent();

		public string Title => "About";
		public string Header => "About";
	}
}
