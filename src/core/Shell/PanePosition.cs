using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls.Docking;

namespace Fuxion.Shell
{
	public enum PanelPosition
	{
		DockedLeft = DockState.DockedLeft,
		DockedBottom = DockState.DockedBottom,
		DockedRight = DockState.DockedRight,
		DockedTop = DockState.DockedTop,
		FloatingDockable = DockState.FloatingDockable,
		FloatingOnly = DockState.FloatingOnly,
		Document = 6
	}
}
