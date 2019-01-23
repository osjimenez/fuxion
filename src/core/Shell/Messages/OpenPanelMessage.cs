using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Shell.Messages
{
	internal class OpenPanelMessage
	{
		public OpenPanelMessage(PanelName name, Dictionary<string, object> args)
		{
			Name = name;
			Arguments = args;
		}
		public PanelName Name { get; }
		public Dictionary<string, object> Arguments { get; } = new Dictionary<string, object>();
	}
}
