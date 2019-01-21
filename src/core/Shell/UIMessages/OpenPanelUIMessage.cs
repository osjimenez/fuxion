using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Shell.UIMessages
{
	public class OpenPanelUIMessage
	{
		public OpenPanelUIMessage(PanelName name)
		{
			Name = name;
		}
		public OpenPanelUIMessage(PanelName name, Dictionary<string, object> args) : this(name)
		{
			Arguments = args;
		}
		public PanelName Name { get; }
		public Dictionary<string, object> Arguments { get; } = new Dictionary<string, object>();
	}
}
