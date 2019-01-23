using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Shell.Messages
{
	internal class CloseAllPanelsWithKeyMessage
	{
		public CloseAllPanelsWithKeyMessage(string key)
		{
			Key = key;
		}
		public string Key { get; }
	}
}
