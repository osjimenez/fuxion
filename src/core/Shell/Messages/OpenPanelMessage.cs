using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Shell.Messages
{
	internal record OpenPanelMessage(PanelName Name, Dictionary<string, object> Arguments);
}
