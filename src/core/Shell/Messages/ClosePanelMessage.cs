using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls;

namespace Fuxion.Shell.Messages
{
	internal class ClosePanelMessage
	{
		public ClosePanelMessage(PanelName name)
		{
			Name = name;
		}
		public ClosePanelMessage(RadPane pane)
		{
			Pane = pane;
		}
		public PanelName? Name { get; }
		public RadPane? Pane { get; }
	}
}
