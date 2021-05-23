using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls;

namespace Fuxion.Shell.Messages
{
	internal record ClosePanelMessage(PanelName? Name = null, RadPane? Pane = null);
}
